using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Azure.Identity;
using Azure.Search.Documents.Indexes;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Infrastructure.Helpers;
using Infrastructure;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Azure.Search.Documents;
using Microsoft.SemanticKernel;
using Kamervragen.Domain.Blob;
using Spectre.Console;
using Microsoft.Extensions.Azure;
using System.Text;
using Kamervragen.Domain.Chat;
using Kamervragen.Domain.Cosmos;
using System.Text.Json;
using Spectre.Console.Json;
using System.Linq.Expressions;

namespace Kamervragen.ConsoleApp
{
    public enum DocumentAction
    {
        Download,
        Extract,
        Chat
    }

    internal class Program
    {
        private static void Main(string[] args)
        {
            // first do the plumbing
            var services = CreateServices();

            AnsiConsole.Write(
            new FigletText("KamerVragen")
                .LeftJustified()
                .Color(Color.Green));

            // Ask for the user's favorite fruit
            var selection = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Do you want to chat, download or extract documents")
                    .AddChoices(["Chat", "Download", "Extract"]));

            // Echo the fruit back to the terminal
            Console.WriteLine($"Ok, {selection} selected");

            switch (selection)
            {
                case "Download":
                    DownloadDocs download = services.GetRequiredService<DownloadDocs>();
                    var downloadPrompt = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title("Download documents")
                            .AddChoices(new[] {
                                "Download all documents",
                                "Download specific document"
                            }));
                    if (downloadPrompt == "Download specific document")
                    {
                        var documentId = AnsiConsole.Ask<string>("Enter the document id");
                        download.Run(documentId).GetAwaiter().GetResult();
                    }
                    else
                    {
                        var filter = AnsiConsole.Prompt(
                            new TextPrompt<string>("Enter the filter (e.g. Soort eq 'Antwoord schriftelijke vragen'")
                            .DefaultValue("Soort eq 'Antwoord schriftelijke vragen'"));

                        var sort = AnsiConsole.Prompt(
                            new TextPrompt<string>("Enter the sort (e.g. Datum desc)")
                            .DefaultValue("Datum desc"));

                        var top = AnsiConsole.Prompt(
                            new TextPrompt<string>("Enter the top (e.g. 100)")
                            .DefaultValue("100"));

                        download.Run(filter, sort, top).GetAwaiter().GetResult();
                    }
                    break;
                case "Extract":
                    ExtractDocs extract = services.GetRequiredService<ExtractDocs>();
                    var extractPrompt = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title("Extraction of documents")
                            .AddChoices(new[] {
                                "Process all documents", 
                                "Process specific document"
                            }));

                    if (extractPrompt == "Process specific document")
                    {
                        var documentId = AnsiConsole.Ask<string>("Enter the document id");
                        extract.Run(documentId).GetAwaiter().GetResult();
                    }
                    else
                    {
                        extract.Run(string.Empty).GetAwaiter().GetResult();
                    }

                    break;
                case "Chat":
                    Chat chat = services.GetRequiredService<Chat>();
                    chat.Run().GetAwaiter().GetResult();
                    break;
            }
        }

        private static ServiceProvider CreateServices()
        {
            var builder = new ConfigurationBuilder()
               .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);

            DefaultAzureCredentialOptions azureCredentialOptions = DefaultCredentialOptions.GetDefaultAzureCredentialOptions("Development");
            var azureCredential = new DefaultAzureCredential(azureCredentialOptions);

            var configuration = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.development.json", optional: true, reloadOnChange: true)
               .Build();


            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<IConfiguration>(configuration)    
                .AddSingleton(sp =>
                {
                    var configuration = sp.GetRequiredService<IConfiguration>();
                    var serviceUri = new Uri(configuration["Storage:ServiceUri"]);
                    return new BlobServiceClient(serviceUri, azureCredential);
                })
                .AddSingleton(sp =>
                {
                    var configuration = sp.GetRequiredService<IConfiguration>();
                    var serviceUri = new Uri(configuration["Search:EndPoint"]);
                    return new SearchIndexClient(serviceUri, azureCredential);
                })
                .AddSingleton(sp =>
                {
                    var configuration = sp.GetRequiredService<IConfiguration>();
                    var accountEndpoint = configuration["Cosmos:AccountEndpoint"];
                    var cosmosClientOptions = new CosmosClientOptions
                    {
                        ConnectionMode = ConnectionMode.Direct,
                        RequestTimeout = TimeSpan.FromSeconds(30)
                    };
                    return new CosmosClient(accountEndpoint, azureCredential, cosmosClientOptions);
                })
                .AddSingleton(sp =>
                {
                    var configuration = sp.GetRequiredService<IConfiguration>();
                    var completionModel = configuration["OpenAI:CompletionModel"];
                    var endpoint = configuration["OpenAI:EndPoint"];

                    var kernelBuilder = Kernel.CreateBuilder();
                    kernelBuilder.AddAzureOpenAIChatCompletion(completionModel, endpoint, azureCredential);
                    return kernelBuilder.Build();
                })
                .AddSingleton<IDocumentStore, BlobDocumentStore>()
                .AddSingleton<ISearchService, AISearchService>()
                .AddSingleton<IThreadRepository, CosmosThreadRepository>()
                .AddSingleton<IAIService, SemanticKernelService>()
                .AddSingleton<IDocumentRegistry, CosmosDocumentRegistry>()
                .AddSingleton<DownloadDocs>()
                .AddSingleton<ExtractDocs>()
                .AddSingleton<Chat>()
                .BuildServiceProvider();

            return serviceProvider;
        }
    }
}
