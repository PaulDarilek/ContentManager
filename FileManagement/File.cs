using ContentManagement;
using ContentManagement.Compression;
using System;
using System.Collections.Generic;
using System.IO;

namespace FileManagement
{
    /// <summary>Contains FileInfo data, but with Drive Letter and Path split out</summary>
    public class File : IDateCreated, IDateUpdated, IDateRetention, IHashCode, INotes, ITags, IProperties
    {
        /// <summary>Identity Column</summary>
        public int FileId { get; set; }

        /// <summary>Database Row Id</summary>
        /// <remarks>Foreign Key to Drive</remarks>
        public int? DriveId { get; set; }

        /// <summary>Drive Letter</summary>
        /// <remarks>Without trailing backslash (C:)</remarks>
        public string DriveLetter { get; set; }

        /// <summary>Folder Path not including <see cref="DriveLetter"/></summary>
        public string DirectoryPath { get; set; }

        /// <summary></summary>
        public string FileName { get; set; }

        /// <summary></summary>
        public FileAttributes Attributes { get; set; }

        /// <summary></summary>
        public bool? Exists { get; set; }

        /// <summary></summary>
        public long? Length { get; set; }

        /// <summary>Creation Time (Utc)</summary>
        public DateTime? DateCreatedUtc { get; set; }

        /// <summary>Last Write Time (Utc)</summary>
        public DateTime? DateUpdatedUtc { get; set; }

        /// <summary>When File was deleted (Utc)</summary>
        public DateTime? DateDeletedUtc { get; set; }

        /// <summary></summary>
        public bool IsReadOnly { get; set; }

        /// <summary></summary>
        public DateTime? DateRetention { get; set; }

        /// <summary></summary>
        public string HashCode { get; set; }

        /// <summary></summary>
        public uint? Crc32 { get; set; }

        /// <summary></summary>
        public string Notes { get; set; }

        /// <summary></summary>
        public HashSet<string> Tags { get; set; }

        /// <summary></summary>
        public Dictionary<string, string> Properties { get; set; }

        public virtual Drive Drive { get; set; }

        /// <summary>Default Constructor</summary>
        public File() { }

        /// <summary>Constructor</summary>
        public File(FileInfo info)
        {
            Tags = Tags ?? new HashSet<string>();
            Properties = Properties ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            CopyFrom(info);
        }

        public string GetFullPath() => Path.Combine($"{Drive?.DriveLetter}{DirectoryPath}", FileName);

        public void Refresh(bool computeHash = false)
        {
            // FileName and DirectoryName are locked.
            var file = new FileInfo(GetFullPath());
            CopyFrom(file);

            if (file.Exists)
            {
                DateDeletedUtc = null;
                if (computeHash)
                {
                    ComputeHash();
                }
            }
            else
            {
                DateDeletedUtc = DateDeletedUtc ?? DateTime.UtcNow;
            }
        }

        public File CopyFrom(FileInfo info)
        {
            FileName = info.Name;
            Exists = info.Exists;
            Attributes = info.Attributes;
            DateCreatedUtc = info.CreationTimeUtc;
            DateUpdatedUtc = info.LastWriteTimeUtc;
            IsReadOnly = info.IsReadOnly;

            if (Length != info.Length)
            {
                Length = info.Length;
                HashCode = null;
                Crc32 = null;
            }

            string path = DirectoryPath = info.DirectoryName;
            if (Drive.ParseDriveLetter(ref path, out string driveLetter))
            {
                DirectoryPath = path;
                DriveLetter = driveLetter;
            }

            return this;
        }

        /// <summary>Compute <see cref="HashCode"/> and <see cref="Crc32"/> if they are null</summary>
        public void ComputeHash(bool force = false)
        {
            var info = new FileInfo(Path.Combine(DriveLetter, DirectoryPath, FileName));
            if (info.Exists)
            {
                using (var reader = info.OpenRead())
                {
                    reader.Position = 0;

                    HashCode =
                        force || string.IsNullOrEmpty(HashCode) ?
                        reader.ToHashSha1Base64() :
                        HashCode;

                    reader.Position = 0;

                    Crc32 =
                        force || Crc32 == null || Crc32 == uint.MinValue ?
                        reader.ToHashCrc32() :
                        Crc32;
                }
            }
        }

    }
}
