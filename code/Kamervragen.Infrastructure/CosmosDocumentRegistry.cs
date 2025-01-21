using Kamervragen.Domain.Blob;
using Kamervragen.Domain.Search;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Configuration;
using System.Reflection.Metadata;
using Container = Microsoft.Azure.Cosmos.Container;

namespace Infrastructure
{
    public class CosmosDocumentRegistry : IDocumentRegistry
    {
        private readonly CosmosClient _client;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CosmosDocumentRegistry> _logger;

        private Container _extractedDocumentsContainer;
        private Container _documentsPerThreadContainer;

        public CosmosDocumentRegistry(CosmosClient client, IConfiguration configuration, ILogger<CosmosDocumentRegistry> logger)
        {
            _logger = logger;
            _client = client;
            _configuration = configuration;
            string databaseName = _configuration.GetValue<string>("Cosmos:DatabaseName") ?? "chats";
            string extractedDocumentsContainerName = _configuration.GetValue<string>("Cosmos:DocumentsContainerName") ?? "documents";
            string documentsPerThreadContainerName = _configuration.GetValue<string>("Cosmos:DocumentPerThreadContainerName") ?? "documentsperthread";

            _extractedDocumentsContainer = _client.GetContainer(databaseName, extractedDocumentsContainerName);
            _documentsPerThreadContainer = _client.GetContainer(databaseName, documentsPerThreadContainerName);
        }

        public async Task<string> AddDocumentToThreadAsync(DocsPerThread docsPerThread)
        {
            try
            {
                var response = await _documentsPerThreadContainer.UpsertItemAsync(docsPerThread, new PartitionKey(docsPerThread.UserId));
                if (response.StatusCode != System.Net.HttpStatusCode.Created && response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new Exception("Failed to add document to Document Registry");
                }
                return response.Resource.Id;
            }
            catch (CosmosException cosmosEx)
            {
                _logger.LogError(cosmosEx, "CosmosException occurred while adding document to thread.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while adding document to thread.");
                throw;
            }
        }

        public async Task<string> AddExtractedDocumentAsync(DocumentResult docsPerThread)
        {
            try
            {
                var response = await _extractedDocumentsContainer.UpsertItemAsync(docsPerThread, new PartitionKey(docsPerThread.Id));
                if (response.StatusCode != System.Net.HttpStatusCode.Created && response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new Exception("Failed to add document to Document Registry");
                }
                return response.Resource.Id;
            }
            catch (CosmosException cosmosEx)
            {
                _logger.LogError(cosmosEx, "CosmosException occurred while adding extracted document.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while adding extracted document.");
                throw;
            }
        }

        public async Task<string> UpdateDocumentAsync(DocsPerThread docsPerThread)
        {
            try
            {
                var response = await _documentsPerThreadContainer.ReplaceItemAsync(docsPerThread, docsPerThread.Id, new PartitionKey(docsPerThread.Id));
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new Exception("Failed to change document");
                }
                return response.Resource.Id;
            }
            catch (CosmosException cosmosEx)
            {
                _logger.LogError(cosmosEx, "CosmosException occurred while updating document.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while updating document.");
                throw;
            }
        }

        public async Task<bool> DeleteDocumentAsync(DocsPerThread docsPerThread)
        {
            try
            {
                var response = await _documentsPerThreadContainer.DeleteItemAsync<DocsPerThread>(docsPerThread.Id, new PartitionKey(docsPerThread.Id));
                if (response.StatusCode != System.Net.HttpStatusCode.NoContent)
                {
                    throw new Exception("Failed to delete document");
                }
                return true;
            }
            catch (CosmosException cosmosEx)
            {
                _logger.LogError(cosmosEx, "CosmosException occurred while deleting document.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while deleting document.");
                throw;
            }
        }

        internal async Task<bool> MarkDocumentAsDeletedAsync(string documentId)
        {
            var fieldsToUpdate = new Dictionary<string, object>
            {
                { "deleted", true },
            };

            try
            {
                return await UpdateDocumentFieldsAsync(documentId, fieldsToUpdate);
            }
            catch (CosmosException cosmosEx)
            {
                _logger.LogError(cosmosEx, "CosmosException occurred while marking document as deleted.");
                throw new Exception($"Failed to mark document as deleted: {cosmosEx.Message}", cosmosEx);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while marking document as deleted.");
                throw new Exception($"An error occurred while marking document as deleted: {ex.Message}", ex);
            }
        }

        public async Task<bool> RemoveDocumentAsync(DocsPerThread document)
        {
            return await MarkDocumentAsDeletedAsync(document.Id);
        }

        public async Task<bool> RemoveDocumentFromThreadAsync(List<DocsPerThread> documents)
        {
            foreach (var document in documents)
            {
                bool isUpdated = await MarkDocumentAsDeletedAsync(document.Id);
                if (!isUpdated)
                {
                    return false;
                }
            }

            return true;
        }

        internal async Task<bool> UpdateDocumentFieldsAsync(string documentId, Dictionary<string, object> fieldsToUpdate)
        {
            var patchOperations = new List<PatchOperation>();

            foreach (var field in fieldsToUpdate)
            {
                patchOperations.Add(PatchOperation.Set($"/{field.Key}", field.Value));
            }

            try
            {
                var response = await _documentsPerThreadContainer.PatchItemAsync<DocsPerThread>(documentId, new PartitionKey(documentId), patchOperations);
                return response.StatusCode == System.Net.HttpStatusCode.OK;
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, "CosmosException occurred while updating document fields.");
                throw new Exception($"Failed to update document: {ex.Message}", ex);
            }
        }

        public async Task<List<DocumentResult>> GetExtractedDataFromDocument(string documentId)
        {
            try
            {
                var queryable = _extractedDocumentsContainer.GetItemLinqQueryable<DocumentResult>(requestOptions: new QueryRequestOptions { MaxItemCount = 500 })
                                          .Where(d => d.Id == documentId);

                var documents = new List<DocumentResult>();
                using (var iterator = queryable.ToFeedIterator())
                {
                    while (iterator.HasMoreResults)
                    {
                        var response = await iterator.ReadNextAsync();
                        documents.AddRange(response);
                    }
                }

                return documents;
            }
            catch (CosmosException cosmosEx)
            {
                _logger.LogError(cosmosEx, "CosmosException occurred while getting extracted data from document.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while getting extracted data from document.");
                throw;
            }
        }

        public async Task<List<DocsPerThread>> GetDocsPerThreadAsync(string threadId)
        {
            try
            {
                var queryable = _documentsPerThreadContainer.GetItemLinqQueryable<DocsPerThread>(requestOptions: new QueryRequestOptions { MaxItemCount = 500 })
                                          .Where(d => d.ThreadId == threadId && d.Deleted == false);

                var documents = new List<DocsPerThread>();
                using (var iterator = queryable.ToFeedIterator())
                {
                    while (iterator.HasMoreResults)
                    {
                        var response = await iterator.ReadNextAsync();
                        documents.AddRange(response);
                    }
                }

                return documents;
            }
            catch (CosmosException cosmosEx)
            {
                _logger.LogError(cosmosEx, "CosmosException occurred while getting documents per thread.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while getting documents per thread.");
                throw;
            }
        }

        public async Task<string> AddExtractedDocumentAsync(VraagStukResult docsPerThread)
        {
            try
            {
                var response = await _extractedDocumentsContainer.UpsertItemAsync(docsPerThread, new PartitionKey(docsPerThread.Id));
                if (response.StatusCode != System.Net.HttpStatusCode.Created && response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new Exception("Failed to add document to Document Registry");
                }
                return response.Resource.Id;
            }
            catch (CosmosException cosmosEx)
            {
                _logger.LogError(cosmosEx, "CosmosException occurred while adding extracted document.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while adding extracted document.");
                throw;
            }
        }
    }
}
