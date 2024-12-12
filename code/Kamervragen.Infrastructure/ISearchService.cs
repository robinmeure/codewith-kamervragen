using Azure.Search.Documents.Models;
using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure;

public interface ISearchService
{
    //Task<bool> IsChunkingComplete(string threadId);
    Task<List<DocsPerThread>> IsChunkingComplete(List<DocsPerThread> docsPerThreads);
    Task<DocsPerThread> IsChunkingComplete(DocsPerThread docsPerThreads);

    Task<bool> StartIndexing();
    //Task<bool> ExtractDocuments(List<BlobDocumenResult> documents);
    //Task<bool> DeleteDocumentAsync(BlobDocumenResult document);
    Task<List<IndexDoc>> SearchForDocuments(string query, List<string>? documentIds);
    Task<List<IndexDoc>> QueryDocumentAsync(string documentId);
    Task<SupportingContentRecord[]> QueryDocumentsAsync(
       string? query = null,
       CancellationToken cancellationToken = default,
       List<string>? documentIds = null
           );
}
