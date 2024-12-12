using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Azure;
using Azure.Identity;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using Domain;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;


namespace Infrastructure
{
    public class AISearchService : ISearchService
    {
        private SearchClient _searchClient;
        private SearchIndexClient _indexClient;
        private SearchIndexerClient _indexerClient;
        private readonly ILogger<AISearchService> _logger;
        private readonly IConfiguration _configuration;

        private string indexerName;
        private string indexName;

        private readonly string semanticSearchConfigName = "2nd";
        private readonly string language = "NL-nl";

        public AISearchService(SearchIndexClient indexClient, IConfiguration configuration, ILogger<AISearchService> logger)
        {
            _logger = logger;
            _configuration = configuration;

            indexerName = configuration.GetValue<string>("Search:IndexerName") ?? "onyourdata-indexer";
            indexName = configuration.GetValue<string>("Search:IndexName") ?? "onyourdata";

            _indexClient = indexClient;
            _searchClient = indexClient.GetSearchClient(indexName);
            _indexerClient = new SearchIndexerClient(_searchClient.Endpoint, new DefaultAzureCredential());
        }

        public async Task<bool> StartIndexing()
        {
            var response = await _indexerClient.RunIndexerAsync(indexerName);
            if (response.IsError)
                return false;
            return true;
        }

        public async Task<bool> DeleteDocumentAsync(BlobDocumenResult document)
        {
            try
            {
                
                var searchOptions = new SearchOptions
                {
                    Size = 500,
                    Select = { "chunk_id", "documentId" },
                    Filter = string.Format("documentId eq '{0}'", document.Id)
                };
                SearchResults<SearchDocument> response = await _searchClient.SearchAsync<SearchDocument>("*", searchOptions);

                IndexDocumentsBatch<SearchDocument> batch = new IndexDocumentsBatch<SearchDocument>();
                await foreach (SearchResult<SearchDocument> searchResult in response.GetResultsAsync())
                {
                    var deleteAction = IndexDocumentsAction.Delete("chunk_id", searchResult.Document["chunk_id"].ToString());
                    batch.Actions.Add(deleteAction);
                }

                IndexDocumentsResult result = await _searchClient.IndexDocumentsAsync(batch);

            }
            catch (RequestFailedException ex)
            {
                return false;
            }

            return true;
        }

        public async Task<DocsPerThread> IsChunkingComplete(DocsPerThread docPerThread)
        {
            List<DocsPerThread> docsPerThread = new List<DocsPerThread> { docPerThread };
            var result = await IsChunkingComplete(docsPerThread);
            return result.First();
        }

        public async Task<List<DocsPerThread>> IsChunkingComplete(List<DocsPerThread> docsPerThreads)
        {
            for ( int x = 0; x < docsPerThreads.Count; x++)
            {
                var doc = docsPerThreads[x];
                var searchOptions = new SearchOptions
                {
                    Size = 1,
                    IncludeTotalCount = true,
                    Select = { "chunk_id", "documentId", "threadId" },
                    Filter = string.Format("documentId eq '{0}'", doc.Id)
                };
                SearchResults<SearchDocument> response = await _searchClient.SearchAsync<SearchDocument>("*", searchOptions);
                doc.AvailableInSearchIndex = response.TotalCount > 0;
            }

            return docsPerThreads;
        }

        public async Task<List<IndexDoc>> SearchForDocuments(string query, List<string>? documentIds = null)
        {
            List<IndexDoc> docs = new List<IndexDoc>();

            // Construct filter string for multiple document IDs if provided
            string? filterString = null;
            if (documentIds != null && documentIds.Any())
            {
                filterString = string.Join(" or ", documentIds.Select(id => $"documentId eq '{id}'"));
            }

            SearchResults<IndexDoc> response = await _searchClient.SearchAsync<IndexDoc>(
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
                    Size = 5,
                    Filter = $"threadId eq null",
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
                searchResult.Document.Score = searchResult.SemanticSearch.RerankerScore ?? 0;
                docs.Add(searchResult.Document);
            }

            if (response.SemanticSearch.Answers != null)
            {
                foreach (QueryAnswerResult result in response.SemanticSearch.Answers)
                {
                    var matchingDoc = docs.FirstOrDefault(doc => doc.ChunkId == result.Key);
                    if (matchingDoc != null)
                    {
                        var indexDoc = new IndexDoc
                        {
                            Answer = result.Text,
                            DocumentId = matchingDoc.DocumentId,
                            ChunkId = result.Key,
                            Content = result.Text,
                            FileName = matchingDoc.FileName,
                            Score = result.Score ?? 0
                        };
                        docs.Add(indexDoc);
                    }
                }
            }

            docs = docs.OrderByDescending(doc => doc.Score).ToList();
            return docs;
        }

        public async Task<List<IndexDoc>> QueryDocumentAsync(string documentId)
        {
            List<IndexDoc> searchResults = new List<IndexDoc>();

            string? filterString = string.Format("documentId eq '{0}'", documentId);
            string query = "wat zijn alle vragen en antwoorden in dit document";

            SearchResults<IndexDoc> response = await _searchClient.SearchAsync<IndexDoc>(
                query,
                new SearchOptions
                {
                    SemanticSearch = new()
                    {
                        SemanticConfigurationName =semanticSearchConfigName,
                        QueryCaption = new(QueryCaptionType.Extractive),
                        QueryAnswer = new(QueryAnswerType.Extractive),
                        SemanticQuery = query
                    },
                    Size = 100,
                    Filter = filterString,
                    QueryLanguage = language,
                    QueryType = SearchQueryType.Semantic
                });

            await foreach (SearchResult<IndexDoc> searchResult in response.GetResultsAsync())
            {
                searchResults.Add(searchResult.Document);
            }

            return searchResults;
        }

       public async Task<SupportingContentRecord[]> QueryDocumentsAsync(
       string? query = null,
       CancellationToken cancellationToken = default,
       List<string>? documentIds = null
           )
        {
            // Construct filter string for multiple document IDs if provided
            string? filterString = null;
            if (documentIds != null && documentIds.Any())
            {
                filterString = string.Join(" or ", documentIds.Select(id => $"id eq '{id}'"));
            }

            SearchOptions searchOptions = new SearchOptions
            {
                SemanticSearch = new()
                {
                    SemanticConfigurationName = semanticSearchConfigName,
                    QueryCaption = new(QueryCaptionType.Extractive),
                    QueryAnswer = new(QueryAnswerType.Extractive),
                    SemanticQuery = query
                },
                Size = 100,
                Filter = filterString,
                QueryLanguage = language,
                QueryType = SearchQueryType.Semantic,
            };

            var searchResultResponse = await _searchClient.SearchAsync<SearchDocument>(
                query, searchOptions, cancellationToken);
            if (searchResultResponse.Value is null)
            {
                throw new InvalidOperationException("fail to get search result");
            }

            SearchResults<SearchDocument> searchResult = searchResultResponse.Value;

            // Assemble sources here.
            // Example output for each SearchDocument:
            // {
            //   "@search.score": 11.65396,
            //   "id": "Northwind_Standard_Benefits_Details_pdf-60",
            //   "content": "x-ray, lab, or imaging service, you will likely be responsible for paying a copayment or coinsurance. The exact amount you will be required to pay will depend on the type of service you receive. You can use the Northwind app or website to look up the cost of a particular service before you receive it.\nIn some cases, the Northwind Standard plan may exclude certain diagnostic x-ray, lab, and imaging services. For example, the plan does not cover any services related to cosmetic treatments or procedures. Additionally, the plan does not cover any services for which no diagnosis is provided.\nIt’s important to note that the Northwind Standard plan does not cover any services related to emergency care. This includes diagnostic x-ray, lab, and imaging services that are needed to diagnose an emergency condition. If you have an emergency condition, you will need to seek care at an emergency room or urgent care facility.\nFinally, if you receive diagnostic x-ray, lab, or imaging services from an out-of-network provider, you may be required to pay the full cost of the service. To ensure that you are receiving services from an in-network provider, you can use the Northwind provider search ",
            //   "category": null,
            //   "sourcepage": "Northwind_Standard_Benefits_Details-24.pdf",
            //   "sourcefile": "Northwind_Standard_Benefits_Details.pdf"
            // }
            var sb = new List<SupportingContentRecord>();
            foreach (var doc in searchResult.GetResults())
            {
                doc.Document.TryGetValue("chunk_id", out var sourcePageValue);
                string? contentValue;
                try
                {
                    var docs = doc.SemanticSearch.Captions.Select(c => c.Text);
                    contentValue = string.Join(" . ", docs);
                }
                catch (ArgumentNullException)
                {
                    contentValue = null;
                }

                string chunkId = sourcePageValue as string;
                string pageNr = chunkId.Split("sourcePage")[1];

                if (sourcePageValue is string sourcePage && contentValue is string content)
                {
                    content = content.Replace('\r', ' ').Replace('\n', ' ');
                    sb.Add(new SupportingContentRecord(sourcePage, sourcePage, pageNr, content));
                }
            }

            return [.. sb];
        }
    }
}
