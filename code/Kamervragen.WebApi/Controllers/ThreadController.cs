using Domain;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Data;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web.Resource;
using Microsoft.Azure.Cosmos;
using System.Text.Json;
using WebApi.Helpers;
using System.Text.RegularExpressions;
using WebApi.Entities;
using Azure.Search.Documents.Models;
using Azure.Search.Documents;
using Microsoft.OpenApi.Services;
using System.Threading;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Kamervragen.Domain;
using WebApi.Utils;

namespace WebApi.Controllers
{

    [Route("threads")]
    [Authorize]
    [ApiController]
    [RequiredScope("chat")]

    public class ThreadController : ControllerBase
    {
        private readonly IThreadRepository _threadRepository;
        private readonly IDocumentRegistry _documentRegistry;
        private readonly ILogger<ThreadController> _logger;
        private readonly IConfiguration _configuration;
        private readonly Kernel _kernel;
        private readonly VectorStoreTextSearch<IndexDoc> _vectorSearch;
        private readonly ISearchService _searchService;
        private readonly PromptHelper _promptHelper;

        public ThreadController(
            ILogger<ThreadController> logger,
            IThreadRepository cosmosThreadRepository,
            IConfiguration configuration,
            Kernel kernel,
            VectorStoreTextSearch<IndexDoc> vectorSearch,
            PromptHelper promptHelper,
            ISearchService searchService,
            IDocumentRegistry documentRegistry
            )
        {
            _threadRepository = cosmosThreadRepository;
            _configuration = configuration;
            _logger = logger;
            _kernel = kernel;
            _vectorSearch = vectorSearch;
            _promptHelper = promptHelper;
            _searchService = searchService;
            _documentRegistry = documentRegistry;
        }

        [HttpGet("")]
        public async Task<IActionResult> GetThreads()
        {
            string? userId = HttpContext.GetUserId();

            if (userId == null)
            {
                return BadRequest();
            }

            _logger.LogInformation("Fetching threads from CosmosDb for userId : {0}", userId);
            
            List<Domain.Thread> threads = await _threadRepository.GetThreadsAsync(userId);

            _logger.LogInformation("Fetched threads from CosmosDb for userId : {0}", userId);
            return Ok(threads);
        }

        [HttpPost("")]
        public async Task<IActionResult> CreateThread()
        {
            string? userId = HttpContext.GetUserId();

            if (userId == null)
            {
                return BadRequest();
            }
            _logger.LogInformation("Creating thread in CosmosDb for userId : {0}", userId);

            Domain.Thread thread = await _threadRepository.CreateThreadAsync(userId);

            if(thread == null)
            {
                _logger.LogInformation("Failed to create thread in CosmosDb for userId : {0}", userId);
            }

            _logger.LogInformation("Created thread in CosmosDb for userId : {0}", userId);
            
            await _threadRepository.PostMessageAsync(userId, thread.Id, "You are a helpful assistant that helps people find information.", "system");

            return Ok(thread);
        }

        [HttpDelete("{threadId}")]
        public async Task<IActionResult> DeleteThread([FromRoute] string threadId)
        {
            string? userId = HttpContext.GetUserId();

            if (userId == null)
            {
                return BadRequest();
            }

            bool result = await _threadRepository.MarkThreadAsDeletedAsync(userId, threadId);

            if (result)
            {
                return Ok();
            } 

            return BadRequest();
           
        }

        [HttpGet("{threadId}/messages")]
        public async Task<IActionResult> Get([FromRoute] string threadId)
        {
            _logger.LogInformation("Fetching thread messages from CosmosDb for threadId : {0}", threadId);
            string userId = HttpContext.User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;

            if (userId == null)
            {
                return BadRequest();
            }

            List<ThreadMessage> result = await _threadRepository.GetMessagesAsync(userId, threadId);
            return Ok(result);
        }

        [HttpDelete("{threadId}/messages")]
        public async Task<IActionResult> DeleteMessages([FromRoute] string threadId)
        {
            _logger.LogInformation("Deleting messages in CosmosDb for threadId : {0}", threadId);

            string userId = HttpContext.User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;

            if (userId == null)
            {
                return BadRequest();
            }

            bool result = await _threadRepository.DeleteMessages(userId, threadId);

            if (result)
            {
                return Ok();
            }

            return BadRequest();

        }

        [HttpGet("{threadId}/search/{documentId}")]
        public async Task<IActionResult> GetAnswers([FromRoute] string documentId)
        {
            var documentResult = await _documentRegistry.GetExtractedDataFromDocument(documentId);
            if (documentResult == null || documentResult.Count == 0)
            {
                var chunks = await _searchService.QueryDocumentAsync(documentId);
                var extractedDoc = await _promptHelper.ExtractDocument(chunks, documentId);
                var addedDoc = await _documentRegistry.AddExtractedDocumentAsync(extractedDoc);
                return NotFound();
            }
            return Ok(documentResult.FirstOrDefault());
        }


        [HttpPost("{threadId}/search")]
        public async Task<IActionResult> Search([FromQuery] string query)
        {
            string userId = HttpContext.User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;
            if (userId == null)
            {
                return BadRequest();
            }
           
            var searchResults = await _searchService.SearchForDocuments(query, null);

            // using this hashset to make sure we don't store the same document multiple times (which can happen because of the chunking)
            HashSet<string> uniqueDocumentIds = new HashSet<string>();
            
            
            List<IndexDoc> results = new List<IndexDoc>();
            foreach (IndexDoc doc in searchResults)
            {
                if (uniqueDocumentIds.Add(doc.FileName))
                {
                    results.Add(new IndexDoc()
                    {
                        DocumentId = doc.DocumentId,
                        FileName = doc.FileName,
                        //Title = doc.Title,
                        //Onderwerp = doc.Onderwerp,
                        //FileName = doc.FileName,
                        //Soort = doc.Soort,
                        //Organisatie = doc.Organisatie,
                        ChunkId = string.Empty,
                        Highlights = (doc.Highlights.Count ==0 ) ? new List<string>() : doc.Highlights,
                        Content = doc.Content,
                        Score = doc.Score
                        //Datum = doc.Datum
                    });
                }
            }
            return Ok(results);
        }


        [HttpPost("{threadId}/messages")]
        [Produces("application/json")]
        [Consumes("application/json")]
        public async Task<IActionResult> Post([FromRoute] string threadId, [FromBody] MessageRequest messageRequest)
        {
            bool suggestFollowupQuestions = true; // need to configure this
            bool keepTrackOfThoughts = true;
            List<Thoughts> thoughts = new List<Thoughts>();

            _logger.LogInformation("Adding thread message to CosmosDb for threadId : {0}", threadId);

            string userId = HttpContext.User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;

            if (userId == null)
            {
                return BadRequest();
            }

            try
            {
                ThreadMessage question = new()
                {

                    Id = Guid.NewGuid().ToString(),
                    Type = "CHAT_MESSAGE",
                    ThreadId = threadId,
                    UserId = userId,
                    Role = "user",
                    Content = messageRequest.Message,
                    Context = null,
                    Created = DateTime.Now
                };


                List<ThreadMessage> messages = await _threadRepository.GetMessagesAsync(userId, threadId);
                ChatHistory history = _promptHelper.BuildConversationHistory(messages, messageRequest.Message);
                IChatCompletionService completionService = _kernel.GetRequiredService<IChatCompletionService>();

                string rewrittenQuery = string.Empty;
                if (messageRequest.includeQA)
                    rewrittenQuery = messageRequest.Message;
                else
                    rewrittenQuery = await _promptHelper.RewriteQueryAsync(history);

                thoughts.Add(new Thoughts("Prompt to generate search query",rewrittenQuery));

                List<IndexDoc> searchResults = new List<IndexDoc>();

                // check if the user has uploaded any documents
                var documents = await _documentRegistry.GetDocsPerThreadAsync(threadId);
                if (documents.Count > 0 && messageRequest.includeDocs)
                {

                    foreach (var document in documents)
                    {
                        var documentResult = await _searchService.QueryDocumentAsync(document.Id);
                        searchResults.AddRange(documentResult);
                    }
                    thoughts.Add(new Thoughts("Documents in current conversation", JsonSerializer.Serialize(SimpleIndexDocs(searchResults))));
                }

                // check if there any selected QA pairs in the payload, if there are, use that to also pass that to the completionservice
                if (messageRequest.SelectedQAPair.Count > 0)
                { 
                    _promptHelper.AugmentQA(history, messageRequest.SelectedQAPair);
                    thoughts.Add(new Thoughts("Using provided QA pairs", JsonSerializer.Serialize(messageRequest.SelectedQAPair)));
                }
                else if (messageRequest.includeDocs)
                {
                    _promptHelper.AugmentHistoryWithSearchResultsUsingSemanticRanker(history, searchResults);
                }
                else
                {
                    // if not than utilize the documents and contents from search
                    var genericResults = await _searchService.SearchForDocuments(rewrittenQuery, null);
                    searchResults.AddRange(genericResults);
                    _promptHelper.AugmentHistoryWithSearchResultsUsingSemanticRanker(history, searchResults);

                    //thoughts.Add(new Thoughts("Using searchresults to define a response ", JsonSerializer.Serialize(SimpleIndexDocs(searchResults))));
                }

                // Specify response format by setting Type object in prompt execution settings.
                var executionSettings = new AzureOpenAIPromptExecutionSettings
                {
                    ResponseFormat = typeof(AnswerAndThougthsResponse)
                };

                // pass the data to the completionservice to formulate a response
                var response = await completionService.GetChatMessageContentsAsync(
                    executionSettings: executionSettings,
                    chatHistory: history,
                    kernel: _kernel
                );

                var assistantResponse = "";
                foreach (var chunk in response)
                {
                    assistantResponse += chunk.Content;
                }

                var assistantAnswer = JsonSerializer.Deserialize<AnswerAndThougthsResponse>(assistantResponse);

                // get follow up questions
                string[]? followUpQuestionList = null;
                if (suggestFollowupQuestions)
                {
                    var _question = messageRequest.Message;
                    followUpQuestionList = await _promptHelper.GenerateFollowUpQuestionsAsync(history, assistantResponse, _question);
                }

                thoughts.Add(new Thoughts("Answer", assistantAnswer.Thoughts));

                // create response which will be send back to the frontend
                var responseMessage = new ResponseMessage("assistant", assistantAnswer.Answer);
                var responseContext = new ResponseContext(
                    FollowupQuestions: followUpQuestionList ?? Array.Empty<string>(),
                    DataPointsContent: searchResults.Select(x => new SupportingContentRecord(x.FileName, x.DocumentId, (x.ChunkId.Split("_pages_")[1]), x.Content)).ToArray(),
                    Thoughts: thoughts.ToArray());

                ThreadMessage answer = new()
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = "CHAT_MESSAGE",
                    ThreadId = threadId,
                    UserId = userId,
                    Role = responseMessage.Role,
                    Content = responseMessage.Content,
                    Context = responseContext,
                    Created = DateTime.Now
                };



                await _threadRepository.PostMessageAsync(userId, question);
                await _threadRepository.PostMessageAsync(userId, answer);

                return Ok(answer);
            }
            catch (HttpOperationException httpOperationException)
            {
                _logger.LogError("An error occurred: {0}", httpOperationException.Message);
                return BadRequest(httpOperationException);

                return RateLimitResponse(httpOperationException);
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred: {0}", ex.Message);
            }
            return new EmptyResult();
        }

        public static List<SimpleIndexDoc> SimpleIndexDocs(List<IndexDoc> results)
        {
            List<SimpleIndexDoc> simpleIndexDocs = new List<SimpleIndexDoc>();
            foreach (var result in results)
            {
                simpleIndexDocs.Add(
                   new SimpleIndexDoc()
                        {
                            DocumentId = result.DocumentId,
                            FileName = result.FileName,
                            Highlights = result.Highlights
                        }
                    );
            }
            return simpleIndexDocs;

        }

        internal IActionResult RateLimitResponse(HttpOperationException httpOperationException)
        {
            string message = httpOperationException.Message;
            int retryAfterSeconds = Utilities.ExtractRetryAfterSeconds(message);
            Response.Headers["retry-after"] = retryAfterSeconds.ToString();
            return StatusCode(429, "Too many requests. Please try again later.");
        }
    }
}
