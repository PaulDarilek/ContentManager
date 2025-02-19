using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DocumentManagement
{
    public interface IDocumentDatabase
    {
        IQueryable<Document> Documents { get; }
        IQueryable<DocumentBlob> Blobs { get; }

        int SaveChanges();

        /// <summary>Add a Document</summary>
        void Add(Document document);

        /// <summary>Retreive a document by Guid</summary>
        Document GetDocument(Guid documentGuid);

        /// <summary>Search for Documents</summary>
        IEnumerable<Document> SearchDocuments(Func<Document, bool> predicate);

    }
}



