using System.Text.Json.Serialization;

public record ResponseContext(
[property: JsonPropertyName("dataPointsContent")] SupportingContentRecord[]? DataPointsContent,
[property: JsonPropertyName("followup_questions")] string[] FollowupQuestions,
[property: JsonPropertyName("thoughts")] Thoughts[] Thoughts)
{
    [JsonPropertyName("data_points")]
    public DataPoints DataPoints { get => new DataPoints(DataPointsContent?.Select(x => $"{x.DocumentId}#{x.PageNumber}: {x.Content}").ToArray() ?? Array.Empty<string>()); }
    public string ThoughtsString { get => string.Join("\n", Thoughts.Select(x => $"{x.Title}: {x.Description}")); }

}

public record SupportingContentRecord(string FileName, string DocumentId, string PageNumber, string Content);

public record DataPoints(
[property: JsonPropertyName("text")] string[] Text)
{ }

