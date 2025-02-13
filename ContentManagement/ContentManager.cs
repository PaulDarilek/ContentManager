using ContentManagement.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace ContentManagement
{
    public class ContentManager
    {
        public IContentStore Database { get; }
        public FileManager DriveManager { get; }
        public Action<string> WriteLine { get; set; } = (text) => Debug.WriteLine(text);


        public ContentManager(IContentStore contentStore)
        {
            Database = contentStore;
            DriveManager = new FileManager(contentStore);
        }

        public int AddFiles(IEnumerable<FileInfo> files, bool computeHash = false)
        {
            const int batchSize = 50;
            Debug.Assert(files != null);
            int newFileCount = 0;
            foreach (var file in files)
            {
                var found = Find(file);
                switch (found.Length)
                {
                    case 0:
                        var item = AddFile(file,computeHash);
                        WriteLine($"{item.DirectoryPath}\t{item.FileName}");
                        newFileCount++;
                        break;

                    case 1:
                        // Single File: Update the information
                        found[0].CopyFrom(file); 
                        break;
                    
                    default:
                        break;
                }
                if (found.Length == 0)
                {
                    var drive = DriveManager.GetDrive(file.Directory);
                    var item = new Models.File(file)
                    {
                        DriveLetter = drive?.DriveLetter,
                        DriveId = drive?.DriveId,
                    };
                    if (computeHash)
                    {
                        item.ComputeHash();
                    }
                    Database.Add(item);
                    if (newFileCount % batchSize == 0)
                    {
                        Database.SaveChanges();
                    }
                }

            }
            if (newFileCount % batchSize != 0)
            {
                Database.SaveChanges();
            }
            return newFileCount;
        }

        public Models.File[] Find(FileInfo file)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));
            if (file.DirectoryName == null) throw new ArgumentNullException(nameof(file.DirectoryName));
            var query = Database.Files.Where(x => file.Name.Equals(x.FileName) && file.DirectoryName.Equals(x.DirectoryPath));

            var drive = DriveManager.GetDrive(file.Directory);
            if (drive != null)
            {
                query = query.Where(x => x.DriveId == drive.DriveId);
            }

            var found = query.ToArray();
            return found;
        }

        /// <summary>Get Files on a Drive Letter that neeed to a HashCode and Hash them.</summary>
        /// <param name="driveLetter"></param>
        /// <param name="batchSize"></param>
        /// <param name="rowsMax"></param>
        /// <param name="token"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void HashFilesForDrive(string driveLetter, int batchSize = 50, int rowsMax = short.MaxValue, CancellationToken token = default)
        {
            WriteLine($"{nameof(HashFilesForDrive)}({driveLetter})");

            if (string.IsNullOrEmpty(driveLetter) || driveLetter == "*")
            {
                foreach (var drive in DriveManager.Drives.Values)
                {
                    WriteLine($"{nameof(HashFilesForDrive)}({drive.DriveLetter})");
                    HashFilesForDrive(drive.DriveLetter, batchSize, rowsMax, token);
                }
            }
            else
            {
                var drive =
                    DriveManager.GetDrive(new DriveInfo(driveLetter)) ??
                    throw new ArgumentOutOfRangeException(nameof(driveLetter));

                HashFilesForDrive(drive, batchSize, rowsMax, token);
            }

        }

        public void HashFilesForDrive(Drive drive, int batchSize = 50, int rowsMax = short.MaxValue, CancellationToken token = default)
        {
            int rows;
            do
            {
                rows = 0;
                var rowsToHash =
                    Database.Files
                    .Where(x => x.DriveId == drive.DriveId && (string.IsNullOrEmpty(x.HashCode) || x.Crc32 == null))
                    .Take(batchSize)
                    .OrderBy(x => x.Length)
                    ;
                foreach (var row in rowsToHash)
                {
                    if (token.IsCancellationRequested)
                        break;
                    rows++;
                    row.ComputeHash();
                    WriteLine($"{row.FileName} {row.Length} {row.HashCode}");
                }

                if (rows > 0)
                {
                    Database.SaveChanges();
                    rowsMax -= rows;
                }

            } while (rows == batchSize && rowsMax > 0 && !token.IsCancellationRequested);
        }

        private Models.File AddFile(FileInfo file, bool computeHash)
        {
            var drive = DriveManager.GetDrive(file.Directory);
            var item = new Models.File(file)
            {
                DriveLetter = drive?.DriveLetter,
                DriveId = drive?.DriveId,
            };
            if (computeHash)
            {
                item.ComputeHash();
            }
            Database.Add(item);
            WriteLine($"{item.DirectoryPath}\t{item.FileName}");
            return item;
        }

    }
}
