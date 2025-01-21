//using Infrastructure;
//using Kamervragen.Domain.Chat;
//using Kamervragen.Domain.Cosmos;
//using Kamervragen.Domain.Search;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Logging;

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//using System.Text;
//using System.Threading.Tasks;

//namespace Kamervragen.Infrastructure
//{
//    public class AzureOpenAIService : IAIService
//    {
       
//        private readonly ILogger<SemanticKernelService> _logger;
//        private readonly IConfiguration _configuration;

//        public AzureOpenAIService(ILogger<SemanticKernelService> logger, IConfiguration configuration)
//        {
//            _logger = logger;
//            _configuration = configuration;
//        }

//        public ChatHistory AugmentHistoryWithSearchResultsUsingSemanticRanker(ChatHistory history, List<TweedeKamerVragenDoc> searchResults)
//        {
//            throw new NotImplementedException();
//        }

//        public ChatHistory AugmentQA(ChatHistory history, List<SelectedQAPair> selectedQAPairs)
//        {
//            throw new NotImplementedException();
//        }

//        public ChatHistory BuildConversationHistory(List<ThreadMessage> messages, string newMessage)
//        {
//            throw new NotImplementedException();
//        }

//        public Task<VraagStukResult> ExtractDocument(List<TweedeKamerVragenDoc> chunks, string documentId)
//        {
//            throw new NotImplementedException();
//        }

//        public Task<string[]> GenerateFollowUpQuestionsAsync(ChatHistory history, string assistantResponse, string question)
//        {
//            throw new NotImplementedException();
//        }

//        public Task<AnswerAndThougthsResponse> GetAnswerAndThougthsResponse(ChatHistory history)
//        {
//            throw new NotImplementedException();
//        }

//        public Task<string> RewriteQueryAsync(ChatHistory history)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}
