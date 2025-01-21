using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Data;
using Newtonsoft.Json;
using System.Numerics;
using System.Text.Json.Serialization;

public record IndexDoc
{
    [JsonPropertyName("documentId")]
    [VectorStoreRecordData]
    [TextSearchResultLink]

    public required string DocumentId { get; init; }

    [JsonPropertyName("chunk_id")]
    [VectorStoreRecordKey]
    [TextSearchResultName]
    public required string ChunkId { get; init; }

    [JsonPropertyName("content")]
    [VectorStoreRecordData]
    [TextSearchResultValue]

    public required string Content { get; init; }

    [JsonPropertyName("fileName")]
    [VectorStoreRecordData]
    [TextSearchResultLink]

    public required string FileName { get; set; }

    [JsonPropertyName("content_vector")]
    [VectorStoreRecordVector(3072)]
    public ReadOnlyMemory<float> ContentVector { get; init; }
}

public record TweedeKamerVragenDoc : IndexDoc
{
    [VectorStoreRecordData]
    [TextSearchResultLink]
    [JsonPropertyName("members")]
    public string Members { get; set; }

    [VectorStoreRecordData]
    [TextSearchResultLink]
    [JsonPropertyName("summary")]
    public string Summary { get; set; }

    [VectorStoreRecordData]
    [TextSearchResultLink]
    [JsonPropertyName("intent")]
    public string Intent { get; set; }

    [VectorStoreRecordData]
    [TextSearchResultLink]
    [JsonPropertyName("onderwerp")]
    public string Onderwerp { get; set; }

    [VectorStoreRecordData]
    [TextSearchResultLink]
    [JsonPropertyName("datum")]
    public string Datum { get; set; }

    [VectorStoreRecordData]
    [TextSearchResultLink]
    [JsonPropertyName("vergaderjaar")]
    public string Vergaderjaar { get; set; }

    [VectorStoreRecordData]
    [TextSearchResultLink]
    [JsonPropertyName("soort")]
    public string Soort { get; set; }

    [VectorStoreRecordData]
    [TextSearchResultLink]
    [JsonPropertyName("questionandanswers")]
    public QuestionsAndAnswers[]? QuestionsAndAnswers { get; set; }
}

public record QuestionsAndAnswers
{
    [JsonPropertyName("question")]
    public string Question { get; set; }
    [JsonPropertyName("answer")]
    public string Answer { get; set; }
}

public record SemanticResult : TweedeKamerVragenDoc
{
    public List<string>? Highlights { get; set; }

    public string Answer { get; set; }

    public double Score { get; set; }
}

public record SimpleIndexDoc
{ 
    public required string DocumentId { get; set; }
    public required string FileName { get; set; }
    public List<string>? Highlights { get; set; }
}
    
