using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;

namespace FileManagement
{

    public class FileManager
    {
        public StringComparison Comparison { get; set; } = StringComparison.OrdinalIgnoreCase;
        public Action<string> WriteLine { get; set; } = (text) => Debug.WriteLine(text);

        /// <summary>List of Drives that are available on current system</summary>
        public Dictionary<string, Drive> Drives { get; }

        private IFileRepository Database { get; }

        public FileManager(IFileRepository database)
        {
            Database = database;
            Drives = new Dictionary<string, Drive>(StringComparer.OrdinalIgnoreCase);
            LoadCurrentDrives();
            Database.SaveChanges();
        }

        public int SaveChanges() => Database.SaveChanges();

        #region Drives

        public void LoadCurrentDrives()
        {
            foreach (var info in DriveInfo.GetDrives())
            {
                if (info.IsReady)
                {
                    var drive = GetDrive(info);
                    if (drive != null)
                    {
                        Drives[drive.DriveLetter] = drive;
                    }
                    try
                    {
                        var jsonFile = new FileInfo(Path.Combine(info.Name, "DriveInfo.json"));
                        if (info.IsReady == true && !jsonFile.Exists)
                        {
                            var options = new JsonSerializerOptions { WriteIndented = true };
                            string json = JsonSerializer.Serialize(drive, options: options);
                            System.IO.File.WriteAllText(jsonFile.FullName, json);
                        }
                    }
                    catch (Exception)
                    {
                        // ignore... it wasn't too important.
                    }
                }
            }
            Database.SaveChanges();
        }

        /// <summary>Retrieve Drive Information</summary>
        /// <param name="driveLetterChar">Drive Letter and Colon such as C:</param>
        public Drive GetDrive(DriveInfo info)
        {
            if (Drives.ContainsKey(info.Name))
                return Drives[info.Name];

            var drive = Database.FindOrAdd(info);

            Drives[drive.DriveLetter] = drive;
            return drive;
        }

        [Obsolete("Use file.Directory")]
        public Drive GetDrive(FileInfo file) => GetDrive(file.Directory);

        public Drive GetDrive(DirectoryInfo directory)
        {
            string driveLetter = directory.GetDriveLetter();
            return
                Drives.ContainsKey(driveLetter) ?
                Drives[driveLetter] :
                GetDrive(new DriveInfo(driveLetter));
        }

        public int SetDriveModelAndHardwareSerialNumber(string driveLetter, string volumeSerialNumber, string volumeLabel, string hardwareSerialNumber, string model)
        {
            if (string.IsNullOrEmpty(driveLetter) || !Drives.TryGetValue(driveLetter, out var drive) || drive == null)
            {
                return 0;
            }

            int changeCount = 0;
            drive.VolumeSerialNumber = CheckChange(drive.VolumeSerialNumber, volumeSerialNumber?.Trim());
            drive.VolumeLabel = CheckChange(drive.VolumeLabel, volumeLabel?.Trim());
            drive.HardwareSerialNumber = CheckChange(drive.HardwareSerialNumber, hardwareSerialNumber?.Trim());
            drive.Model = CheckChange(drive.Model, model?.Trim());

            if (changeCount > 0)
            {
                Database.SaveChanges();
            }
            return changeCount;

            string CheckChange(string original, string newValue)
            {
                if (string.IsNullOrEmpty(original) && !string.IsNullOrEmpty(newValue))
                {
                    changeCount++;
                    return newValue;
                }
                return original;
            }
        }

        #endregion

        #region Files

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
                        var item = AddFile(file, computeHash);
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
                    var drive = GetDrive(file.Directory);
                    var item = new File(file)
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

        public File[] Find(FileInfo file)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));
            if (file.DirectoryName == null) throw new ArgumentNullException(nameof(file.DirectoryName));
            var query = Database.Files.Where(x => file.Name.Equals(x.FileName) && file.DirectoryName.Equals(x.DirectoryPath));

            var drive = GetDrive(file.Directory);
            if (drive != null)
            {
                query = query.Where(x => x.DriveId == drive.DriveId);
            }

            var found = query.ToArray();
            return found;
        }

        /// <summary>Hash all Files on a Drive that is missing a <see cref="File.HashCode"/ or <see cref="File.Crc32"/>></summary>
        public void HashFilesForDrive(Drive drive, int batchSize = 50, int rowsMax = short.MaxValue, CancellationToken token = default)
        {
            int rows;
            do
            {
                rows = 0;
                var rowsToHash =
                    Database.Files
                    .Where(x => x.DriveId == drive.DriveId && (string.IsNullOrEmpty(x.HashCode) || x.Crc32 == null || (x.Crc32 == 0 && x.Length != 0)))
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

        public int DeleteFile(IEnumerable<FileInfo> files, bool deleteFromFileSystem)
        {
            int count = 0;
            foreach (var file in files)
            {
                count += DeleteFile(file, deleteFromFileSystem);
            }
            return count;
        }

        public int DeleteFile(FileInfo file, bool deleteFromFileSystem)
        {
            int deleteCount = 0;
            if (deleteFromFileSystem && file.Exists)
            {
                deleteCount = 1;
                file.Delete();
            }

            var rows =
                Database.Files
                .Where(x => file.Name.Equals(x.FileName, Comparison) && file.DirectoryName != null && file.DirectoryName.Equals(x.DirectoryPath));

            int rowCount = 0;
            foreach (var row in rows)
            {
                if (row.Drive?.MachineNames != null && !row.Drive.MachineNames.Contains(Environment.MachineName))
                {
                    // Wrong Machine
                    continue;
                }

                row.Exists = false;
                row.DateDeletedUtc = DateTime.UtcNow;
                rowCount += SaveChanges();
            }

            return Math.Max(rowCount, deleteCount);
        }

        public int Scan(IEnumerable<FileInfo> files, bool computeHash = false)
        {
            const int batchSize = 50;
            Debug.Assert(files != null);
            int newFileCount = 0;
            foreach (var file in files)
            {
                var found = Database.FindFile(file, GetDrive);
                if (!found.Any())
                {
                    var drive = GetDrive(file.Directory);
                    var item = new File(file)
                    {
                        Drive = drive,
                        DriveId = drive?.DriveId,
                    };
                    if (computeHash)
                    {
                        item.ComputeHash();
                    }
                    Database.Add(item);
                    WriteLine($"{item.DirectoryPath}\t{item.FileName}");
                    newFileCount++;
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

        /// <summary>Look at all Files on Active Drives and verify if they still exist</summary>
        /// <exception cref="NotImplementedException"></exception>
        public int VerifyFilesExist(Drive drive, bool computeHash = false)
        {
            int count = 0;
            if (drive == null)
                return count;

            var files = Database.Files.Where(x => x.DriveId == drive.DriveId && x.Exists != false);
            foreach (var file in files)
            {
                file.Refresh(computeHash);
                count++;

                if (count % 50 == 0)
                {
                    Database.SaveChanges();
                }
            }
            Database.SaveChanges();

            return count;
        }

        private File AddFile(FileInfo file, bool computeHash)
        {
            var drive = GetDrive(file.Directory);
            var item = new File(file)
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

        #endregion

    }
}