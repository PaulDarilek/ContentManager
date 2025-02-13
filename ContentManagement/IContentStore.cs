using ContentManagement.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ContentManagement
{
    public interface IContentStore : IFileSystemStore, IDocumentStore, ISaveChanges
    {

    }

    public interface IDocumentStore : ISaveChanges
    {
        IQueryable<Document> Documents { get; }
        IQueryable<DocumentBlob> Blobs { get; }

        /// <summary>Add a Document</summary>
        void Add(Document document);

        /// <summary>Retreive a document by Guid</summary>
        Document GetDocument(Guid documentGuid);

        /// <summary>Search for Documents</summary>
        IEnumerable<Document> SearchDocuments(Func<Document,bool> predicate);


    }


    public interface IFileSystemStore : ISaveChanges
    {

        #region DBSets
        /// <summary>Drives found on a system</summary>
        IQueryable<Drive> Drives { get; }

        /// <summary>Directories of interest on a drive</summary>
        IQueryable<Models.Directory> Directories { get; }

        /// <summary>Files scanned or stored</summary>
        IQueryable<Models.File> Files { get; }

        /// <summary>Connector table for connecting files that may match or be duplicated/backups/etc.</summary>
        IQueryable<FileDuplicates> FileDuplicates { get; }
        #endregion


        /// <summary>Add a Drive</summary>
        void Add(Drive drive);

        /// <summary>Add a Directory</summary>
        void Add(Models.Directory directory);

        /// <summary>Add a File</summary>
        void Add(Models.File file);

        /// <summary>Add a Connection of Duplicate files</summary>
        void Add(FileDuplicates fileStorage);

        /// <summary>Return a match from database, or add this one to Database</summary>
        IEnumerable<Drive> FindDrives(string driveLetter = null, string volumeSerialNumber = null, string volumeLabel = null, long? size = null, string machineName = null);

        /// <summary>Return a match from database, or add this one to Database</summary>
        /// <remarks></remarks>
        Drive FindOrAdd(Drive drive);

        /// <summary>Retreive or Create and Add a Drive to the repository</summary>
        Drive FindOrAdd(DriveInfo drive);

        /// <summary>Find a File for the FileInfo</summary>
        IEnumerable<Models.File> Find(FileInfo file, Func<DirectoryInfo, Drive> driveLookup);

    }

    public interface ISaveChanges
    {
        int SaveChanges();
    }

}
