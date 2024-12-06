//read the appsettings.json file
using Azure;
using Azure.Identity;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using Azure.Storage.Blobs;
using Domain;
using Infrastructure;
using Infrastructure.Helpers;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.IO.Enumeration;
using System.Net.Http;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs.Models;
using WebApi.Helpers;
using Microsoft.Azure.Cosmos.Linq;
using System.Reflection.Metadata;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;

var builder = new ConfigurationBuilder()
    .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);



var configuration = builder.Build();


DefaultAzureCredentialOptions azureCredentialOptions = DefaultCredentialOptions.GetDefaultAzureCredentialOptions("Development");
var azureCredential = new DefaultAzureCredential(azureCredentialOptions);

var accountEndpoint = configuration["Cosmos:AccountEndpoint"];
var cosmosClientOptions = new CosmosClientOptions
{
    ConnectionMode = ConnectionMode.Direct,
    RequestTimeout = TimeSpan.FromSeconds(30)
};
var cosmosClient = new CosmosClient(accountEndpoint, azureCredential, cosmosClientOptions);
Logger<CosmosDocumentRegistry> logger = new Logger<CosmosDocumentRegistry>(new LoggerFactory());
var documentRegistry = new CosmosDocumentRegistry(cosmosClient, configuration, logger);

string storageUri = configuration["Storage:ServiceUri"];
string containerName = configuration["Storage:ContainerName"];

BlobServiceClient blobServiceClient = new BlobServiceClient(new Uri(storageUri), azureCredential);
var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);

List<string> documentIds = new List<string>();

//await foreach (BlobItem blobItem in blobContainerClient.GetBlobsAsync())
//{
//    var blobClient = blobContainerClient.GetBlobClient(blobItem.Name);
//    BlobProperties properties = await blobClient.GetPropertiesAsync();
//    if (properties.Metadata.ContainsKey("documentId") && bool.Parse(properties.Metadata["isProcessed"]) == false)
//        documentIds.Add(properties.Metadata["documentId"]);
//}

Kernel _kernel = new Kernel();
var kernelBuilder = Kernel.CreateBuilder();
kernelBuilder.AddAzureOpenAIChatCompletion("gpt-4o", "https://openai-.openai.azure.com/", azureCredential);
_kernel = kernelBuilder.Build();

var searchClient = new SearchClient(new Uri(configuration["Search:Endpoint"]), configuration["Search:IndexName"], azureCredential);
string query = "wat zijn alle vragen en antwoorden in dit document";
documentIds.Add("3eb05ed7-d3dc-44d4-89ba-64e906f23fcc");

foreach (string documentId in documentIds)
{
    Console.WriteLine("Processing {0}", documentId);
    string? filterString = string.Format("documentId eq '{0}'", documentId);

    IChatCompletionService completionService = _kernel.GetRequiredService<IChatCompletionService>();
    ChatHistory history = [];
    history.AddUserMessage(query);

    List<IndexDoc> searchResults = new List<IndexDoc>();
    SearchResults<IndexDoc> response = await searchClient.SearchAsync<IndexDoc>(
                    query,
                    new SearchOptions
                    {
                        SemanticSearch = new()
                        {
                            SemanticConfigurationName = "2nd",
                            QueryCaption = new(QueryCaptionType.Extractive),
                            QueryAnswer = new(QueryAnswerType.Extractive),
                            SemanticQuery = query,
                            // Fix for CS0200: Use Add method to add items to the read-only collection
                            //SemanticFields = { "content" }
                        },
                        Size = 100,
                        Filter = filterString,
                        QueryLanguage = "NL-nl",
                        QueryType = SearchQueryType.Semantic
                    });

    await foreach (SearchResult<IndexDoc> searchResult in response.GetResultsAsync())
    {
        // this is to ensure only the 'real' relevant documents are being returned
        if (searchResult.SemanticSearch.RerankerScore < 1 || searchResult.SemanticSearch.RerankerScore == null)
            continue;
        if (searchResult.SemanticSearch.Captions != null)
        {
            searchResult.Document.Highlights = new List<string>();
            foreach (var caption in searchResult.SemanticSearch.Captions)
            {
                if (string.IsNullOrEmpty(caption.Highlights))
                    searchResult.Document.Highlights.Add(caption.Text);
                else
                    searchResult.Document.Highlights.Add(caption.Highlights);
            }
        }
        searchResults.Add(searchResult.Document);
    }

    string documents = "";
    foreach (IndexDoc doc in searchResults)
    {
        string chunkId = doc.ChunkId;
        string pageNumber = chunkId.Split("_pages_")[1];
        documents += $"PageNumber: {pageNumber}\n";
        documents += $"FileName: {doc.FileName}\n";
        documents += $"Content: {doc.Content}\n\n";
        documents += "------\n\n";
    }

    string systemPrompt = $@"
        Documents
        -------    
        {documents}
        You are an AI assistant that extracts the question and answers from the provided documents
        These documents contain questions and answers, all questions are in bold and answers are in normal text.
        Make sure that the question is isolated from the answer and there is no need to add 'question' or 'answer' before the text.
        Summarize the title, subject, reference and date of the document when starting the conversation.
    ";

    string assistantResponse = "";
    history.AddSystemMessage(systemPrompt);

    bool success = false;
    while (!success)
    {
        try
        {
            // Specify response format by setting Type object in prompt execution settings.
            var executionSettings = new AzureOpenAIPromptExecutionSettings
            {
                ResponseFormat = typeof(DocumentResult)
            };

            var chatResponse = await completionService.GetChatMessageContentsAsync(
                                executionSettings: executionSettings,
                                chatHistory: history,
                                kernel: _kernel
                            );

            foreach (var chunk in chatResponse)
            {
                assistantResponse += chunk.Content;
            }
            success = true;
        }
        catch (HttpOperationException httpOperationException)
        {
            string message = httpOperationException.Message;
            int retryAfterSeconds = Utilities.ExtractRetryAfterSeconds(message);
            Console.WriteLine($"Rate limit hit. Retrying after {retryAfterSeconds} seconds...");
            System.Threading.Thread.Sleep(retryAfterSeconds * 1000);
        }
    }

    if (string.IsNullOrEmpty(assistantResponse))
        continue;

    var documentResult = JsonSerializer.Deserialize<DocumentResult>(assistantResponse);
    string parsedDocumentId = documentResult.Id;
    documentResult.Id = documentId;

    var addedDoc = await documentRegistry.AddDocumentToThreadAsync(documentResult);

    //if (addedDoc != null)
    //{
    //    var blobClient = blobContainerClient.GetBlobClient(string.Format("{0}.blob", documentId));
    //    BlobProperties blobProperties = await blobClient.GetPropertiesAsync();
    //    var metadata = new Dictionary<string, string>(blobProperties.Metadata)
    //    {
    //        { "isProcessed", "true" }
    //    };
    //    await blobClient.SetMetadataAsync(metadata);
    //}
    //Console.ReadLine();
}

//Console.Read();

//search_datasource = AzureSearchDatasource(self.env_helper)
//            search_datasource.create_or_update_datasource()
//            search_index = AzureSearchIndex(self.env_helper, self.llm_helper)
//            search_index.create_or_update_index()
//            search_skillset = AzureSearchSkillset(
//                self.env_helper, config.integrated_vectorization_config
//            )
//            search_skillset_result = search_skillset.create_skillset()
//            search_indexer = AzureSearchIndexer(self.env_helper)
//            indexer_result = search_indexer.create_or_update_indexer(
//                self.env_helper.AZURE_SEARCH_INDEXER_NAME,
//                skillset_name = search_skillset_result.name,
//            )

////create a blob client
//var serviceUri = $"https://{configuration["StorageAccountName"]}.blob.core.windows.net";
//var blobServiceClient = new BlobServiceClient(new Uri(serviceUri), new AzureCliCredential());
//IDocumentStore store = new BlobDocumentStore(blobServiceClient);

////create a cosmos client
//var cosmosClient = new CosmosClient(configuration["CosmosDbAccountEndpoint"], new AzureCliCredential());
//var database = cosmosClient.GetDatabase(configuration["CosmosDBDatabase"]);
//var container = database.GetContainer(configuration["CosmosDBContainer"]);



////upload the files in folder Docs

//var threadId = Guid.NewGuid().ToString();

//var docsFolder = Directory.GetCurrentDirectory() + "/Docs";
//var docs = new DirectoryInfo(docsFolder).GetFiles();
//foreach (var doc in docs)
//{
//    var docId = await store.AddDocumentAsync(doc.FullName, threadId, configuration["StorageContainerName"]);
//    Console.WriteLine($"Uploaded {doc.Name}");


//       var entry = new DocsPerThread
//    {
//        Deleted = false,
//        DocumentName = doc.Name,
//        Id = docId,
//        ThreadId = threadId,
//        UserId = "test@microsoft.com"

//    };

//    //upload item to cosmosDb
//    await container.UpsertItemAsync(entry, new PartitionKey(entry.id));





//}



