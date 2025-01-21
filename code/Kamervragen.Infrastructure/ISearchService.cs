using Azure.Search.Documents.Models;
using Kamervragen.Domain.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure;

public interface ISearchService
{
    Task<List<DocsPerThread>> IsChunkingComplete(List<DocsPerThread> docsPerThreads);
    Task<DocsPerThread> IsChunkingComplete(DocsPerThread docsPerThreads);
    Task<bool> StartIndexing();
    Task<List<TweedeKamerVragenDoc>> SearchForDocuments(string query, List<string>? documentIds);
    Task<List<TweedeKamerVragenDoc>> QueryDocumentAsync(string documentId);
    Task<bool> Ingest(VraagStukResult document, List<TweedeKamerVragenDoc> chunks);

    Task<List<TweedeKamerVragenDoc>> GetExtractedDocs();
}
