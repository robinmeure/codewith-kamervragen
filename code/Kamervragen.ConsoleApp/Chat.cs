using Infrastructure;
using Kamervragen.Domain.Chat;
using Kamervragen.Domain.Cosmos;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Json;

namespace Kamervragen.ConsoleApp
{
    public class Chat
    {
        private readonly ILogger<Chat> _logger;
        private readonly ISearchService _searchService;
        private readonly IAIService _semanticKernelService;
        private bool suggestFollowupQuestions = true; // need to configure this
        private bool keepTrackOfThoughts = true;
        
        public Chat(
            ILogger<Chat> logger,
            ISearchService searchService,
            IAIService semanticKernelService)
        {
            _searchService = searchService;
            _semanticKernelService = semanticKernelService;
            _logger = logger;
        }

        public async Task Run()
        {
            
            AnsiConsole.WriteLine("Welcome to the Kamervragen chatbot. Ask me anything about the Dutch parliament.");
            AnsiConsole.WriteLine("Type 'exit' to quit the chatbot.");
            string query = AnsiConsole.Prompt(
                    new TextPrompt<string>("What is your question?"));
            Console.WriteLine($"Let's work with your question '{query}'");

            List<ThreadMessage> messages = new List<ThreadMessage>();
           

            while (true)
            {
                if (string.Equals(query.Trim(), "exit", StringComparison.OrdinalIgnoreCase))
                {
                    AnsiConsole.MarkupLine("[bold]Exiting the chatbot. Goodbye![/]");
                    break;
                }

                var thoughts = new List<Thoughts>();
                var searchResults = new List<TweedeKamerVragenDoc>();
                string[] followUpQuestionList = null;
                AnswerAndThougthsResponse assistantAnswer = null;

                await AnsiConsole.Status()
                    .StartAsync("Processing question", async ctx =>
                    {
                        ctx.Status($"Getting conversation history");
                        ctx.Spinner(Spinner.Known.Star);
                        ctx.SpinnerStyle(Style.Parse("green"));
                        var history = _semanticKernelService.BuildConversationHistory(messages, query);
                        //string rewrittenQuery = await _semanticKernelService.RewriteQueryAsync(history);
                        string rewrittenQuery = query;
                        thoughts.Add(new Thoughts("Prompt to generate search query", rewrittenQuery));

                        // Search for relevant documents based on the query
                        ctx.Status($"Searching for relevant documents");
                        ctx.Spinner(Spinner.Known.Star);
                        ctx.SpinnerStyle(Style.Parse("green"));
                        var genericResults = await _searchService.SearchForDocuments(rewrittenQuery, null);
                        searchResults.AddRange(genericResults);
                        AnsiConsole.WriteLine();
                        AnsiConsole.MarkupLine("Found some documents!");
                        foreach(var result in searchResults)
                        {
                            AnsiConsole.MarkupLine($"[bold]{result.FileName} - {result.Onderwerp}[/]");
                        }

                        _semanticKernelService.AugmentHistoryWithSearchResultsUsingSemanticRanker(history, searchResults);

                        // Update the status and spinner
                        ctx.Status($"Passing found documents to the LLM");
                        ctx.Spinner(Spinner.Known.Aesthetic);
                        ctx.SpinnerStyle(Style.Parse("green"));
                        assistantAnswer = await _semanticKernelService.GetAnswerAndThougthsResponse(history);

                        ctx.Status($"Generating follow up prompts");
                        ctx.Spinner(Spinner.Known.Aesthetic);
                        ctx.SpinnerStyle(Style.Parse("green"));

                        if (suggestFollowupQuestions)
                        {
                            followUpQuestionList = await _semanticKernelService.GenerateFollowUpQuestionsAsync(
                                history, assistantAnswer.Answer, query);
                        }
                    }
                );

                var usedDocIds = assistantAnswer.References.Select(x => x).ToList();

                List<SupportingContentRecord> supportingContents = new List<SupportingContentRecord>();
                foreach (string usedDocId in usedDocIds)
                {
                    var doc = searchResults.FirstOrDefault(x => x.FileName == usedDocId);
                    if (doc == null)
                    {
                        continue;
                    }
                    supportingContents.Add(
                        new SupportingContentRecord()
                        {
                            DocumentId = doc.DocumentId,
                            FileName = doc.FileName,
                            ChunkId = doc.ChunkId,
                            Datum = doc.Datum,
                            Intent = doc.Intent,
                            Members = doc.Members,
                            Onderwerp = doc.Onderwerp,
                            Summary = doc.Summary,
                            QuestionsAndAnswers = doc.QuestionsAndAnswers
                        });
                }

                // Create response to send back to the frontend
                var responseMessage = new ResponseMessage("assistant", assistantAnswer.Answer);
                var responseContext = new ResponseContext(
                    FollowupQuestions: Array.Empty<string>(),
                    DataPointsContent: supportingContents.ToArray(),
                    Thoughts: thoughts.ToArray());

                var root = new Tree("Chat").Guide(TreeGuide.Ascii);
                var user = root.AddNode("[bold]User[/]");
                user.AddNode(
                    new Panel(query)
                        .Header("[bold]Query[/]", Justify.Right)
                        .Collapse()
                        .RoundedBorder()
                        .BorderColor(Color.White));

                var json = new JsonText(System.Text.Json.JsonSerializer.Serialize(assistantAnswer));

                var assistantAnswerEscaped = assistantAnswer.Answer.Replace("[", "[[").Replace("]", "]]");
                var assistantThoughtsEscaped = assistantAnswer.Thoughts.Replace("[", "[[").Replace("]", "]]");
                string assistantReferencesEscaped = string.Empty;
                foreach (string reference in assistantAnswer.References)
                {
                    assistantReferencesEscaped += reference.Replace("[", "[[").Replace("]", "]]") + "\n";
                }

               
                var assistant = root.AddNode("[bold]Assistant[/]");
                assistant.AddNode(
                   new Panel(assistantAnswerEscaped)
                       .Header("[bold]Answer[/]", Justify.Left)
                       .Collapse()
                       .RoundedBorder()
                       .BorderColor(Color.Yellow));
                assistant.AddNode(
                    new Panel(assistantThoughtsEscaped)
                        .Header("[bold]Thoughts[/]", Justify.Left)
                        .Collapse()
                        .RoundedBorder()
                        .BorderColor(Color.Yellow));
                assistant.AddNode(
                   new Panel(assistantReferencesEscaped)
                       .Header("[bold]References[/]", Justify.Left)
                       .Collapse()
                       .RoundedBorder()
                       .BorderColor(Color.Yellow));


                AnsiConsole.Write(root);

                var followUpPrompts = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Follow up prompt suggestions")
                        .PageSize(10)
                        .AddChoices(followUpQuestionList));

                query = followUpPrompts;
            }
        }
    }

}
