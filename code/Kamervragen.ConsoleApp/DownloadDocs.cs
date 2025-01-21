using Azure.Storage.Blobs;
using Kamervragen.Domain.Blob;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http.Json;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Kamervragen.ConsoleApp
{
    public class DownloadDocs
    {
        private readonly ILogger<DownloadDocs> _logger;
        private readonly BlobServiceClient _blobServiceClient;
        private IConfiguration _config;

        private string containerName = "documents";
        private BlobContainerClient blobContainerClient = null;
        private static HttpClient client = new HttpClient();

        private string baseUrl = "https://gegevensmagazijn.tweedekamer.nl/OData/v4/2.0/Document";
        private string resourceUrlTemplate;

        public DownloadDocs(
            ILogger<DownloadDocs> logger,
          BlobServiceClient blobServiceClient,
            IConfiguration config)
        {
            _logger = logger;
            _blobServiceClient = blobServiceClient;
            _config = config;
            resourceUrlTemplate = baseUrl + "({0})/resource";
            blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);

        }

        public async Task Run(string documentId)
        {
           await Task.CompletedTask;
        }

        public async Task Run(string filter, string sort, string top)
        {
            // Initialize a list to hold query parameters
            var queryParams = new List<string>();

            // Add non-empty parameters to the query
            if (!string.IsNullOrWhiteSpace(filter))
            {
                queryParams.Add($"$filter={Uri.EscapeDataString(filter)}");
            }

            if (!string.IsNullOrWhiteSpace(sort))
            {
                queryParams.Add($"$orderby={Uri.EscapeDataString(sort)}");
            }

            if (!string.IsNullOrWhiteSpace(top))
            {
                queryParams.Add($"$top={Uri.EscapeDataString(top)}");
            }

            // Construct the URL with only the provided query parameters
            string url = baseUrl;
            if (queryParams.Any())
            {
                url += "?" + string.Join("&", queryParams);
            }

            // Fetch the list of documents with the constructed URL
            var documents = await FetchDocumentsAsync(url);

            // Asynchronous
            await AnsiConsole.Progress()
                .StartAsync(async ctx =>
                {
                    var docTask = ctx.AddTask("[green]Adding docs[/]");
                    docTask.MaxValue = documents.Count;

                    foreach (var document in documents)
                    {
                        if (document.Id != null && !document.Verwijderd)
                        {
                            string resourceUrl = string.Format(resourceUrlTemplate, document.Id);
                            await DownloadAndUploadBlobAsync(resourceUrl, document);
                   
                            docTask.Increment(1);
                        }
                    }

                    docTask.StopTask();
                }
            );

            Spectre.Console.AnsiConsole.MarkupLine("[bold]Done[/]");
        }

        private async Task<List<TweedeKamerDocument>> FetchDocumentsAsync(string url)
        {
            List<TweedeKamerDocument> documents = new List<TweedeKamerDocument>();

            while (!string.IsNullOrEmpty(url))
            {
                var response = await client.GetFromJsonAsync<ODataResponse>(url);
                if (response?.Value != null)
                {
                    documents.AddRange(response.Value);
                }

                url = response?.NextLink;
            }

            return documents;
        }

        private async Task DownloadAndUploadBlobAsync(string url, TweedeKamerDocument document)
        {
            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var contentStream = await response.Content.ReadAsStreamAsync();
                await UploadToAzureBlobStorageAsync(contentStream, document);
            }
            else
            {
                AnsiConsole.WriteLine($"Failed to download blob for document {document.Id}");
            }
           
        }

        async Task UploadToAzureBlobStorageAsync(Stream contentStream, TweedeKamerDocument document)
        {
            var blobClient = blobContainerClient.GetBlobClient($"{document.Id}.blob");

            try
            {
                await blobClient.UploadAsync(contentStream, overwrite: false);
            }
            catch (Azure.RequestFailedException e)
            {
                if (e.ErrorCode == "BlobAlreadyExists")
                {
                    AnsiConsole.WriteLine($"Blob {document.Id} already exists in Azure Blob Storage");
                    return;
                }
                else
                {
                    AnsiConsole.WriteException(e);
                    return;
                }
            }

            // Set metadata
            var metadata = new Dictionary<string, string>
            {
                { "documentId", document.Id },
                { "soort", document.Soort ?? string.Empty },
                { "fileName", document.DocumentNummer ?? string.Empty },
                { "onderwerp", Utilities.SanitizeFileName(document.Onderwerp) ?? string.Empty },
                { "datum", document.Datum },
                { "vergaderjaar", document.Vergaderjaar ?? string.Empty },
                { "kamer", document.Kamer.ToString() ?? string.Empty },
                { "volgnummer", document.Volgnummer.ToString() ?? string.Empty },
                { "aanhangselnummer", document.Aanhangselnummer ?? string.Empty },
                { "organisatie", document.Organisatie ?? string.Empty },
                { "isProcessed", "false" },
                { "threadId", string.Empty}
            };

            try
            {
                await blobClient.SetMetadataAsync(metadata);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to set metadata for blob {document.Id}: {e.Message}");
            }
        }
    }
}
