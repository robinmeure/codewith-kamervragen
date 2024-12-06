using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{

    public record BlobDocumenResult
    {
        [JsonProperty(PropertyName = "id")]
        public required string Id { get; set; }
        [JsonProperty(PropertyName = "title")]
        public required string Title { get; set; }
        [JsonProperty(PropertyName = "isDeleted")]
        public bool IsDeleted { get; set; }
        [JsonProperty(PropertyName = "availableInSearchIndex")]
        public bool? AvailableInSearchIndex { get; set; }
    }
    public record DocumentResult : BlobDocumenResult
    {
        [JsonProperty(PropertyName = "subject")]
        public required string Subject { get; set; }
        [JsonProperty(PropertyName = "reference")]
        public required string Reference { get; set; }
        [JsonProperty(PropertyName = "date")]
        public required string Date { get; set; }
        [JsonProperty(PropertyName = "questions_And_Answers")]
        public required Questions_And_Answers[] Questions_and_answers { get; set; }
    }

    public record Questions_And_Answers
    {
        [JsonProperty(PropertyName = "question")]
        public required string Question { get; set; }
        [JsonProperty(PropertyName = "answer")]
        public string? Answer { get; set; }
    }
}
