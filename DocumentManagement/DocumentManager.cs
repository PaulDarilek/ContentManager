using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace DocumentManagement
{
    public class DocumentManager
    {
        private IDocumentDatabase Database { get; }
        public Action<Document, string> Notify { get; set; } = (document, action) => { };


        public DocumentManager(IDocumentDatabase contentStore)
        {
            Database = contentStore;
        }

        public Guid Insert(Document document)
        {
            document.DocumentGuid =
                document.DocumentGuid == null || document.DocumentGuid == Guid.Empty ?
                Guid.NewGuid() :
                document.DocumentGuid;

            Database.Add(document);
            Database.SaveChanges();

            SendNotification(document);

            return document.DocumentGuid ?? Guid.Empty;
        }
        public Document Read(Guid documentGuid)
        {
            var document = Database.Documents.FirstOrDefault(x => x.DocumentGuid == documentGuid);
            SendNotification(document);
            return document;
        }

        public Document Delete(Guid documentGuid)
        {
            var document = Read(documentGuid);
            SendNotification(document);
            return document;
        }

        public IEnumerable<Document> Search(Func<Document, bool> predicate) => Database.SearchDocuments(predicate);


        private void SendNotification(Document document, [CallerMemberName] string method = null)
        {
            if (document != null && Notify != null)
                Notify(document, method);
        }

    }
}
