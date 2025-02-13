using ContentManagement.Models;
using System;
using System.Collections.Generic;

namespace ContentManagement.Interfaces
{
    public interface IDocument : IDocumentGuid, IDateCreated, IDateUpdated, IDateRetention, INotes, ITags, IProperties
    {
        /// <summary>An alternative to storing file in <see cref="Data"/> is to store on file system.</summary>
        /// <remarks>This can be stored in multiple places to facilitate redundancy</remarks>
        List<DocumentBlob> Blobs { get; }
    }

    public interface IDocumentGuid
    {
        /// <summary>Unique Identifier for Document</summary>
        /// <remarks><see cref="Guid.Empty"/> is used for uncataloged files</remarks>
        Guid? DocumentGuid { get; set; }
    }

    public interface IDateCreated
    {
        /// <summary>Date Document was created</summary>
        /// <remarks>When Created (from <see cref="IDateCreated"/>) Example Source is <see cref="DateTime.UtcNow"/> or <see cref="FileSystemInfo.CreationTimeUtc"/></remarks>
        DateTime? DateCreatedUtc { get; set; }

    }

    public interface IDateUpdated
    {
        /// <summary>Date Document was Last Updated</summary>
        /// <remarks>When last Updated (from <see cref="IDateUpdated"/>) Example Source is <see cref="DateTime.UtcNow"/> or <see cref="System.IO.FileSystemInfo.LastWriteTimeUtc"/></remarks>
        DateTime? DateUpdatedUtc { get; set; }

    }

    public interface IDateRetention
    {
        /// <summary>Retention Date</summary>
        /// <remarks>if set, the content may be deleted after this date.</remarks>
        DateTime? DateRetention { get; set; }
    }


}
