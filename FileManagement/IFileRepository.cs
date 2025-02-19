using ContentManagement;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FileManagement
{

    public interface IFileRepository 
    {

        #region DBSet   s
        /// <summary>Drives found on a system</summary>
        IQueryable<Drive> Drives { get; }

        /// <summary>Directories of interest on a drive</summary>
        IQueryable<Directory> Directories { get; }

        /// <summary>Files scanned or stored</summary>
        IQueryable<File> Files { get; }

        /// <summary>Connector table for connecting files that may match or be duplicated/backups/etc.</summary>
        IQueryable<FileDuplicates> FileDuplicates { get; }
        #endregion

        int SaveChanges();


        /// <summary>Add a Drive</summary>
        void Add(Drive drive);

        /// <summary>Add a Directory</summary>
        void Add(Directory directory);

        /// <summary>Add a File</summary>
        void Add(File file);

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
        IEnumerable<File> FindFile(FileInfo file, Func<DirectoryInfo, Drive> driveLookup);

    }

}
