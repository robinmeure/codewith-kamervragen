using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Data;
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

    public required string FileName { get; init; }

    [JsonPropertyName("content_vector")]
    [VectorStoreRecordVector(1536)]
    public ReadOnlyMemory<float> ContentVector { get; init; }

    public List<string>? Highlights { get; set; }

    public string Answer { get; set; }

    public double Score { get; set; }
}

