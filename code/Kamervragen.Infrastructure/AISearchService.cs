using System;
using System.Collections;
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
using Kamervragen.Domain.Blob;
using Kamervragen.Domain.Search;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
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
        private bool useSemanticRanker = false;

        private readonly string semanticSearchConfigName = "2nd";
        private readonly string language = "NL-nl";

        public AISearchService(SearchIndexClient indexClient, IConfiguration configuration, ILogger<AISearchService> logger)
        {
            _logger = logger;
            _configuration = configuration;

            indexerName = configuration.GetValue<string>("Search:IndexerName") ?? "onyourdata-indexer";
            indexName = configuration.GetValue<string>("Search:IndexName") ?? "onyourdata";
            //useSemanticRanker = configuration.GetValue<bool>("Search:UseSemanticRanker");

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

        public async Task<List<TweedeKamerVragenDoc>> SearchForDocuments(string query, List<string>? documentIds = null)
        {
            List<TweedeKamerVragenDoc> docs = new List<TweedeKamerVragenDoc>();
            HashSet<string> foundDocumentIds = new HashSet<string>();
            // Construct filter string for multiple document IDs if provided
            string? filterString = null;
            if (documentIds != null && documentIds.Any())
            {
                filterString = string.Join(" or ", documentIds.Select(id => $"documentId eq '{id}'"));
            }

            SearchOptions searchOptions = new SearchOptions();

            if (useSemanticRanker)
            {
                searchOptions = new SearchOptions
                {
                    SemanticSearch = new()
                    {
                        SemanticConfigurationName = "2nd",
                        QueryCaption = new(QueryCaptionType.Extractive),
                        QueryAnswer = new(QueryAnswerType.Extractive),
                        SemanticQuery = query,
                    },
                    Size = 10,
                    Filter = filterString,
                    QueryLanguage = "NL-nl",
                    QueryType = SearchQueryType.Semantic,
                   // Select = { selectedFields }
                };
                searchOptions.VectorSearch = new()
                {
                    Queries = {
                        new VectorizableTextQuery(text: query)
                        {
                            KNearestNeighborsCount = 3,
                            Fields = { "content_vector" },
                            Exhaustive = false
                        }
                    },
                };
            }
            else
            {
                searchOptions = new SearchOptions
                {
                    Size =50,
                    Filter = filterString,
                    QueryLanguage = "NL-nl",
                    QueryType = SearchQueryType.Full,
                    //Select = { selectedFields }
                };
                searchOptions.VectorSearch = new()
                {
                    Queries = {
                        new VectorizableTextQuery(text: query)
                        {
                            KNearestNeighborsCount = 3,
                            Fields = { "content_vector" },
                            Exhaustive = false
                        }
                    },
                };
            }

            SearchResults<TweedeKamerVragenDoc> response = await _searchClient.SearchAsync<TweedeKamerVragenDoc>(query, searchOptions);

            await foreach (SearchResult<TweedeKamerVragenDoc> searchResult in response.GetResultsAsync())
            {
                // since the result can contain data from a single document which is split into multiple chunks
                if (foundDocumentIds.Add(searchResult.Document.DocumentId))
                {
                    docs.Add(searchResult.Document);
                }
            }

            return docs;
        }

        public async Task<List<TweedeKamerVragenDoc>> QueryDocumentAsync(string documentId)
        {
            List<TweedeKamerVragenDoc> searchResults = new List<TweedeKamerVragenDoc>();

            string? filterString = string.Format("documentId eq '{0}'", documentId);
            string query = "wat zijn alle vragen en antwoorden in dit document";

            SearchOptions searchOptions = new SearchOptions();
            searchOptions = new SearchOptions
            {
                Size = 100,
                Filter = filterString,
                QueryLanguage = language,
                QueryType = SearchQueryType.Full
            };

            SearchResults<TweedeKamerVragenDoc> response = await _searchClient.SearchAsync<TweedeKamerVragenDoc>(query, searchOptions);

            await foreach (SearchResult<TweedeKamerVragenDoc> searchResult in response.GetResultsAsync())
            {
                searchResults.Add(searchResult.Document);
            }

            return searchResults;
        }

        public async Task<bool> Ingest(VraagStukResult document, List<TweedeKamerVragenDoc> chunks)
        {
            bool isSuccess = false;

            IndexDocumentsBatch<TweedeKamerVragenDoc> batch = new IndexDocumentsBatch<TweedeKamerVragenDoc>();

            foreach (var chunk in chunks)
            {
                chunk.Intent = document.Intent;
                chunk.Members = document.Members;
                chunk.Summary = document.Summary;
                chunk.QuestionsAndAnswers = document.QuestionsAndAnswers;
                //debug
                //chunk.FileName = "2024D1341";
                var deleteAction = IndexDocumentsAction.Merge(chunk);

                batch.Actions.Add(deleteAction);
            }

            IndexDocumentsResult result = await _searchClient.IndexDocumentsAsync(batch);
            return isSuccess;
        }

        public async Task<List<TweedeKamerVragenDoc>> GetExtractedDocs()
        { 
            List<TweedeKamerVragenDoc> tweedeKamerVragenDocs = new List<TweedeKamerVragenDoc>();
            HashSet<string> documentIds = new HashSet<string>();

            string query = "*";
            string emptyFilterString = "members eq null";

            SearchOptions searchOptions = new SearchOptions();
            searchOptions = new SearchOptions
            {
                Size = 100,
                Filter = emptyFilterString,
                QueryType = SearchQueryType.Simple
            };

            SearchResults<TweedeKamerVragenDoc> response = await _searchClient.SearchAsync<TweedeKamerVragenDoc>(query, searchOptions);
            await foreach (SearchResult<TweedeKamerVragenDoc> searchResult in response.GetResultsAsync())
            {
                // Only add the document if it is not already in the list
                // since the result can contain data from a single document which is split into multiple chunks
                if (documentIds.Add(searchResult.Document.DocumentId))
                {
                    tweedeKamerVragenDocs.Add(searchResult.Document);
                }
            }

            return tweedeKamerVragenDocs;
        }
    }
}
