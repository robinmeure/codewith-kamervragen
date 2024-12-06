namespace WebApi.Entities
{
    public record MessageRequest
    {
        public required string Message { get; set; }
        public List<SelectedQAPair>? SelectedQAPair { get; set; }
    }

    public record SelectedQAPair
    { 
        public required string DocumentId { get; set;}
        public required string Question { get; set; }
        public required string Answer { get; set; }
    }
}
