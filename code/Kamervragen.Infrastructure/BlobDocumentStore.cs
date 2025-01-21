using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class BlobDocumentStore : IDocumentStore
    {
        private BlobServiceClient _blobServiceClient;

        public BlobDocumentStore(BlobServiceClient client)
        {
            _blobServiceClient = client;
        }

        public async Task<DocsPerThread> AddDocumentAsync(string userId, IFormFile document, string documentName, string threadId, string folder)
        {
            var documentId = Guid.NewGuid().ToString();
            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(folder);
            var blobClient = blobContainerClient.GetBlobClient(documentId);

            //Upload the document
            await blobClient.UploadAsync(document.OpenReadStream());

            //set meta data
            var metadata = new Dictionary<string, string>
            {
                { "threadId", threadId },
                { "documentId", documentId },
                { "filename", documentName }
            };
            blobClient.SetMetadata(metadata);

            // creating the document object to be returned so that the controller can store it in the cosmos db
            DocsPerThread docsPerThread = new()
            {
                Id = documentId,
                DocumentId = documentId,
                ThreadId = threadId,
                DocumentName = documentName,
                UserId = userId,
                FileSize = document.Length,
                UploadDate = DateTime.Now
            };

            return docsPerThread;
        }

        public async Task<bool> DeleteDocumentAsync(string documentName, string folder)
        {
            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(folder);
            var blobClient = blobContainerClient.GetBlobClient(documentName);
            var result = await blobClient.DeleteIfExistsAsync();
            return result;
        }

        public Task<bool> DocumentExistsAsync(string documentName, string folder)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<string>> GetDocumentsAsync(string threadId, string folder)
        {
            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(folder);
            var results = new List<string>();

            await foreach (BlobItem blob in containerClient.GetBlobsAsync())
            {
                var blobClient = containerClient.GetBlobClient(blob.Name);
                var properties = await blobClient.GetPropertiesAsync();
                var metadata = properties.Value.Metadata;

                // Optionally, you can filter blobs based on metadata, e.g., by threadId
                if (metadata.TryGetValue("threadId", out var blobThreadId) && blobThreadId == threadId)
                {
                    results.Add(blob.Name);
                }
            }

            return results;
        }

        public Task UpdateDocumentAsync(string documentName, string documentUri)
        {
            throw new NotImplementedException();
        }

        public async Task<List<string>> GetDocumentIdsFromBlob(string folder)
        {
            List<string> documentIds = new List<string>();
            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(folder);

            await foreach (BlobItem blobItem in blobContainerClient.GetBlobsAsync())
            {
                var blobClient = blobContainerClient.GetBlobClient(blobItem.Name);
                BlobProperties properties = await blobClient.GetPropertiesAsync();

                if (properties.Metadata.TryGetValue("documentId", out string documentId))
                {
                    documentIds.Add(documentId);
                }
            }

            return documentIds;
        }
    }
}