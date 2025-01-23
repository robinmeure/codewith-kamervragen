using Infrastructure;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kamervragen.ConsoleApp
{
    public class ExtractDocs
    {
        private readonly ILogger<ExtractDocs> _logger;
        private readonly ISearchService _searchService;
        private readonly IDocumentStore _documentStore;
        private readonly IAIService _semanticKernelService;

        private string containerName = "documents";

        public ExtractDocs(
            ILogger<ExtractDocs> logger,
            ISearchService searchService,
            IDocumentStore documentStore,
            IAIService semanticKernelService)
        {
            _searchService = searchService;
            _documentStore = documentStore;
            _semanticKernelService = semanticKernelService;
            _logger = logger;
        }

        public async Task Run(string documentId)
        {
            if (!string.IsNullOrEmpty(documentId))
            {
                await ProcessDocumentAsync(documentId);
                Spectre.Console.AnsiConsole.MarkupLine("[bold]Done[/]");
                return;
            }

            var documents = await _searchService.GetExtractedDocs();

            int processed = 0;
            int maxValue = documents.Count;

            foreach (var document in documents)
            {
              //  AnsiConsole.Status.
                await ProcessDocumentAsync(document.DocumentId);
                processed++;
            }

            AnsiConsole.MarkupLine("[bold]Done[/]");
        }

        private async Task ProcessDocumentAsync(string documentId)
        {
            await AnsiConsole.Status()
                .StartAsync($"Processing document {documentId}", async ctx =>
                {
                    AnsiConsole.MarkupLine($"LOG: {DateTime.Now.ToShortTimeString()} Extracting {documentId} ...");
                    ctx.Status($"Searching document {documentId}");
                    ctx.Spinner(Spinner.Known.Star);
                    ctx.SpinnerStyle(Style.Parse("green"));
                    // Query the document
                    var chunks = await _searchService.QueryDocumentAsync(documentId);

                    // Update the status and spinner
                    ctx.Status($"Extracting document {documentId}");
                    ctx.Spinner(Spinner.Known.Aesthetic);
                    ctx.SpinnerStyle(Style.Parse("green"));

                    // Simulate some work
                    if (chunks.FirstOrDefault()?.Intent != null)
                        return;

                    await ExtractDocumentAsync(chunks, documentId);
                }
            );
        }

        private async Task ExtractDocumentAsync(List<TweedeKamerVragenDoc> chunks, string documentId)
        {
            try
            {
                var extractedDoc = await _semanticKernelService.ExtractDocument(chunks, documentId);
               
                if (extractedDoc != null)
                {
                    await _searchService.Ingest(extractedDoc, chunks);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting document data.");
            }
        }
    }
}
