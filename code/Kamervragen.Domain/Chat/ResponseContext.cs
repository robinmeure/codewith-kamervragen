using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Data;
using System.Text.Json.Serialization;

public record ResponseContext(
    [property: JsonPropertyName("dataPointsContent")] SupportingContentRecord[]? DataPointsContent,
    [property: JsonPropertyName("followup_questions")] string[] FollowupQuestions,
    [property: JsonPropertyName("thoughts")] Thoughts[] Thoughts);


public record SupportingContentRecord
{
    public  string DocumentId { get; set; }
    public string ChunkId { get; set; }
    public string FileName { get; set; }
    public string Members { get; set; }
    public string Summary { get; set; }
    public string Intent { get; set; }
    public string Onderwerp { get; set; }
    public string Datum { get; set; }
    public string Vergaderjaar { get; set; }
    public QuestionsAndAnswers[]? QuestionsAndAnswers { get; set; }
}

public record DataPoints(
[property: JsonPropertyName("text")] string[] Text)
{ }

