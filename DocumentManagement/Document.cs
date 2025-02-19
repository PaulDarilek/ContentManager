using ContentManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace DocumentManagement
{

    public interface IDocumentGuid
    {
        /// <summary>Unique Identifier for Document</summary>
        /// <remarks><see cref="Guid.Empty"/> is used for uncataloged files</remarks>
        Guid? DocumentGuid { get; set; }
    }

    public interface IDocument : IDocumentGuid, IDateCreated, IDateUpdated, IDateRetention, INotes, ITags, IProperties
    {
    }

    /// <summary>Connector for Storing Content on a Database <see cref="Data"/> or in one or more files <see cref="StoragePaths"/></summary>
    public class Document : IDocument
    {
        [Required]
        public Guid? DocumentGuid { get; set; }

        public DateTime? DateCreatedUtc { get; set; }

        public DateTime? DateUpdatedUtc { get; set; }

        public DateTime? DateRetention { get; set; }


        [DataType(DataType.Text)]
        public string Notes { get; set; }

        [DataType(DataType.Text)]
        public HashSet<string> Tags { get; set; }

        [DataType(DataType.MultilineText)]
        public Dictionary<string, string> Properties { get; set; }

        public virtual List<DocumentBlob> Blobs { get; set; }


        public Document()
        {
            EnsureNotNull();
        }

        /// <summary>Ensure Collections are not null</summary>
        public virtual void EnsureNotNull()
        {
            DocumentGuid = DocumentGuid ?? Guid.NewGuid();
            DateCreatedUtc = DateCreatedUtc ?? DateTime.UtcNow;
            DateUpdatedUtc = DateUpdatedUtc ?? DateCreatedUtc;
            Notes = Notes ?? string.Empty;
            Tags = Tags ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            Properties = Properties ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            Blobs = Blobs ?? new List<DocumentBlob>();
        }

        /// <summary>Combine Tags and Properties and Update Notes if Null or Empty</summary>
        /// <param name="other"></param>
        public void UnionWith(Document other)
        {
            EnsureNotNull();
            if (other != null)
            {
                other.EnsureNotNull();
                Notes = Notes.FirstNotNullOrWhiteSpace(other.Notes);
                Tags.UnionWith(other.Tags);
                Properties.UnionWith(other.Properties, replace: false);
            }
        }


        public override string ToString() => JsonSerializer.Serialize(this);
    }
}
