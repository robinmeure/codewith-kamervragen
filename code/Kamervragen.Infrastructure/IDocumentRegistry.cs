using Domain;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure;
public interface IDocumentRegistry
{
    Task<string> AddDocumentToThreadAsync(BlobDocumenResult docsPerThread);
    Task<string> UpdateDocumentAsync(DocumentResult docsPerThread);
    Task<bool> RemoveDocumentFromThreadAsync(List<DocumentResult> docsPerThread); // this is the soft delete method for an entire thread
    Task<bool> RemoveDocumentAsync(DocumentResult document);// this is the soft delete method for a single document
    Task<bool> DeleteDocumentAsync(DocumentResult document); // this is the hard delete method for a single document
    Task<List<DocumentResult>> GetDocsPerThreadAsync(string threadId);
}