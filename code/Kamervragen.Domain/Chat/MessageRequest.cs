// this call is used to send a message to the chatbot (e.g. coming from the frontend)

namespace Kamervragen.Domain.Chat
{
    public record MessageRequest
    {
        public required string Message { get; set; }
        public List<SelectedQAPair>? SelectedQAPair { get; set; }

        public bool includeQA { get; set; }
        public bool includeDocs { get; set; }
    }
}
