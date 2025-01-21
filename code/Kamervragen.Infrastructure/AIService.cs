using Kamervragen.Domain.Chat;
using Kamervragen.Domain.Cosmos;
using Kamervragen.Domain.Search;
using Microsoft.SemanticKernel.ChatCompletion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public interface IAIService
    {
        Task<AnswerAndThougthsResponse> GetAnswerAndThougthsResponse(ChatHistory history);
        Task<string[]> GenerateFollowUpQuestionsAsync(ChatHistory history, string assistantResponse, string question);
        ChatHistory AugmentQA(ChatHistory history, List<SelectedQAPair> selectedQAPairs);
        ChatHistory AugmentHistoryWithSearchResultsUsingSemanticRanker(ChatHistory history, List<TweedeKamerVragenDoc> searchResults);
        Task<VraagStukResult> ExtractDocument(List<TweedeKamerVragenDoc> chunks, string documentId);
        Task<string> RewriteQueryAsync(ChatHistory history);
        ChatHistory BuildConversationHistory(List<ThreadMessage> messages, string newMessage);

    }
}
