using Newtonsoft.Json;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json.Serialization;

public record ResponseChoice
{
    [JsonProperty(PropertyName = "id")]
    public required string Id { get; set; }

    [JsonProperty(PropertyName = "message")]
    public required ResponseMessage Message { get; set; }

    [JsonProperty(PropertyName = "context")]
    public required ResponseContext Context { get; set; }

    [JsonProperty(PropertyName = "created")]
    public required DateTime Created { get; set; }

    [JsonProperty(PropertyName = "citationBaseUrl")]
    public string? CitationBaseUrl { get; set; }

}



//public record ResponseChoice
//    (
    
//    [property: JsonPropertyName("id")] string Id,
//    [property: JsonPropertyName("message")] ResponseMessage Message,
//    [property: JsonPropertyName("context")] ResponseContext Context,
//    [property: JsonPropertyName("created")] DateTime Created,
//    [property: JsonPropertyName("citationBaseUrl")] string CitationBaseUrl)
//{


//}

public record Thoughts(
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("props")] (string, string)[]? Props = null)
{ }