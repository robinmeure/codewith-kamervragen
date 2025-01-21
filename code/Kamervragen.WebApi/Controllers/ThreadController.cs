using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web.Resource;
using System.Text.Json;
using Kamervragen.Domain.Chat;
using Kamervragen.Domain.Cosmos;

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
        private readonly ISearchService _searchService;
        private readonly IAIService _semanticKernelService;

        public ThreadController(
            ILogger<ThreadController> logger,
            IThreadRepository cosmosThreadRepository,
            IConfiguration configuration,
            ISearchService searchService,
            IDocumentRegistry documentRegistry,
            IAIService semanticKernelService
            )
        {
            _threadRepository = cosmosThreadRepository;
            _configuration = configuration;
            _logger = logger;
            _searchService = searchService;
            _documentRegistry = documentRegistry;
            _semanticKernelService = semanticKernelService;
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
            
            List<Kamervragen.Domain.Cosmos.Thread> threads = await _threadRepository.GetThreadsAsync(userId);

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

            Kamervragen.Domain.Cosmos.Thread thread = await _threadRepository.CreateThreadAsync(userId);

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

        [HttpPost("{threadId}/messages")]
        [Produces("application/json")]
        [Consumes("application/json")]
        public async Task<IActionResult> Post([FromRoute] string threadId, [FromBody] MessageRequest messageRequest)
        {
            const bool suggestFollowupQuestions = true; // TODO: Configure this appropriately
            const bool keepTrackOfThoughts = true; // TODO: Configure this appropriately
            var thoughts = new List<Thoughts>();

            _logger.LogInformation("Adding thread message to CosmosDb for threadId : {0}", threadId);

            var userId = HttpContext.User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID not found.");
            }

            // Create the user's question message
            var question = new ThreadMessage
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

            // Build the conversation history
            var messages = await _threadRepository.GetMessagesAsync(userId, threadId);
            var history = _semanticKernelService.BuildConversationHistory(messages, messageRequest.Message);

            // Rewrite the query if needed
            var rewrittenQuery = messageRequest.Message;
            thoughts.Add(new Thoughts("Prompt to generate search query", rewrittenQuery));

            var searchResults = new List<TweedeKamerVragenDoc>();

            // Check if the user has uploaded any documents
            var documents = await _documentRegistry.GetDocsPerThreadAsync(threadId);
            if (documents.Count > 0 && messageRequest.includeDocs)
            {
                List<string> docIds = new List<string>();
                foreach (var doc in documents)
                {
                    docIds.Add(doc.DocumentId);
                }
                searchResults = await _searchService.SearchForDocuments(rewrittenQuery, docIds);
                thoughts.Add(new Thoughts("Documents in current conversation", JsonSerializer.Serialize(SimpleIndexDocs(searchResults))));
            }

            // Augment history with selected QA pairs
            if (messageRequest.SelectedQAPair?.Count > 0)
            {
                _semanticKernelService.AugmentQA(history, messageRequest.SelectedQAPair);
                thoughts.Add(new Thoughts("Using provided QA pairs", JsonSerializer.Serialize(messageRequest.SelectedQAPair)));
            }

            // Utilize documents and contents from search
            var genericResults = await _searchService.SearchForDocuments(rewrittenQuery, null);
            searchResults.AddRange(genericResults);
            _semanticKernelService.AugmentHistoryWithSearchResultsUsingSemanticRanker(history, searchResults);

            var assistantAnswer = await _semanticKernelService.GetAnswerAndThougthsResponse(history);

            // Prepare supporting content records
            var supportingContents = assistantAnswer.References
                .Select(usedDocId => searchResults.FirstOrDefault(x => x.FileName == usedDocId))
                .Where(doc => doc != null)
                .Select(doc => new SupportingContentRecord
                {
                    DocumentId = doc.DocumentId,
                    FileName = doc.FileName,
                    ChunkId = doc.ChunkId,
                    Datum = doc.Datum,
                    Intent = doc.Intent,
                    Members = doc.Members,
                    Onderwerp = doc.Onderwerp,
                    Summary = doc.Summary,
                    QuestionsAndAnswers = doc.QuestionsAndAnswers
                })
                .ToList();

            // Get follow-up questions
            string[] followUpQuestionList = null;
            if (suggestFollowupQuestions)
            {
                followUpQuestionList = await _semanticKernelService.GenerateFollowUpQuestionsAsync(
                    history, assistantAnswer.Answer, messageRequest.Message);
            }

            thoughts.Add(new Thoughts("Answer", assistantAnswer.Thoughts));

            // Create response to send back to the frontend
            var responseMessage = new ResponseMessage("assistant", assistantAnswer.Answer);
            var responseContext = new ResponseContext(
                FollowupQuestions: followUpQuestionList ?? Array.Empty<string>(),
                DataPointsContent: supportingContents.ToArray(),
                Thoughts: thoughts.ToArray());

            var answer = new ThreadMessage
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

            // Post the messages to the repository
            await _threadRepository.PostMessageAsync(userId, question);
            await _threadRepository.PostMessageAsync(userId, answer);

            return Ok(answer);
        }

        public static List<SimpleIndexDoc> SimpleIndexDocs(List<TweedeKamerVragenDoc> results)
        {
            List<SimpleIndexDoc> simpleIndexDocs = new List<SimpleIndexDoc>();
            foreach (var result in results)
            {
                simpleIndexDocs.Add(
                   new SimpleIndexDoc()
                        {
                            DocumentId = result.DocumentId,
                            FileName = result.FileName,
                        }
                    );
            }
            return simpleIndexDocs;
        }
    }
}
