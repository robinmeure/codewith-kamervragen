// this class is used to generate a structured output for
// getting the answer/question pairs from the tweedekamer document
// using gpt4 model and ingest this into the search index

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kamervragen.Domain.Search
{
    public record VraagStukResult
    {
        [JsonProperty(PropertyName = "id")]
        public required string Id { get; set; }
        
        [JsonProperty(PropertyName = "title")]
        public required string Title { get; set; }
        
        [JsonProperty(PropertyName = "members")]
        public required string Members { get; set; }
        
        [JsonProperty(PropertyName = "summary")]
        public required string Summary { get; set; }
        
        [JsonProperty(PropertyName = "intent")]
        public required string Intent { get; set; }
        
        //[JsonProperty(PropertyName = "subject")]
        //public required string Subject { get; set; }
        
        [JsonProperty(PropertyName = "reference")]
        public required string Reference { get; set; }
        
        [JsonProperty(PropertyName = "date")]
        public required string Date { get; set; }
        
        [JsonProperty(PropertyName = "questionsAndAnswers")]
        public required QuestionsAndAnswers[] QuestionsAndAnswers { get; set; }
    }
}
