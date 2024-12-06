using Azure.Search.Documents.Models;
using Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class SemanticKernelService
    {
        private Kernel _kernel;
        private IChatCompletionService _chatCompletionService;
        private readonly ILogger<SemanticKernelService> _logger;
        private readonly IConfiguration _configuration;

        private const string extractQuery = "wat zijn alle vragen en antwoorden in dit document";

        public SemanticKernelService(
            Kernel kernel,
            IConfiguration configuration,
            ILogger<SemanticKernelService> logger)
        {
            kernel = _kernel;
            _chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
            _configuration = configuration;
            _logger = logger;

        }

        public async Task<DocumentResult> ExtractQuestionsAndAnsewers(string query, string documentId, List<IndexDoc> searchResults)
        {
            ChatHistory history = [];
            history.AddUserMessage(query);

            string documents = "";
            foreach (IndexDoc doc in searchResults)
            {
                string chunkId = doc.ChunkId;
                string pageNumber = chunkId.Split("_pages_")[1];
                documents += $"PageNumber: {pageNumber}\n";
                documents += $"FileName: {doc.FileName}\n";
                documents += $"Content: {doc.Content}\n\n";
                documents += "------\n\n";
            }

            string systemPrompt = $@"
                Documents
                -------    


                {documents}

                You are an AI assistant that extracts data from documents and outputs the result in JSON format, using the following syntax:
                {{
                    ""Id"": ""documentId"",
                    ""Title"": ""Document Title"",
                    ""Subject"": ""Document Subject"",
                    ""Reference"": ""Document Reference"",
                    ""Date"": ""Document Date"",
                    ""Questions_and_answers"": [
                        {{
                            ""Question"": ""Question 1"",
                            ""Answer"": ""Answer 1""
                        }},
                        {{
                            ""Question"": ""Question 2"",
                            ""Answer"": ""Answer 2""
                        }}
                    ]
                }}

                These documents contain questions and answers, all questions are in bold and answers are in normal text.
                Summarize the title, subject, reference and date of the document when starting the conversation.
    
            ";

            history.AddSystemMessage(systemPrompt);

            var chatResponse = await _chatCompletionService.GetChatMessageContentsAsync(
                                chatHistory: history,
                                kernel: _kernel
                            );

            var assistantResponse = "";
            foreach (var chunk in chatResponse)
            {
                assistantResponse += chunk.Content;
            }
            assistantResponse = assistantResponse.Replace("```json", string.Empty);
            assistantResponse = assistantResponse.Replace("```", string.Empty);
            DocumentResult _documentResult = null;
            try
            {
                _documentResult = JsonSerializer.Deserialize<DocumentResult>(assistantResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deserializing document result: {assistantResponse}");
                return null;
            }

            DocumentResult documentResult = new DocumentResult
            {
                Id = documentId,
                Title = _documentResult.Title,
                Subject = _documentResult.Subject,
                Reference = _documentResult.Reference,
                Date = _documentResult.Date,
                Questions_and_answers = _documentResult.Questions_and_answers
            };
            return documentResult;
        }


    }
}
