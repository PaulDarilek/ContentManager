using ContentManagement;
using System;
using System.Collections.Generic;
using System.IO;

namespace FileManagement
{
    public class Directory : INotes, ITags, IProperties
    {

        /// <summary>Database Row Id</summary>
        public int DirectoryId { get; set; }

        /// <summary>Navigation to Drive</summary>
        public int DriveId { get; set; }

        /// <summary>Drive Letter without trailing slash</summary>
        public string DriveLetter { get; set; }

        /// <summary>Full path of the directory with leading slash (but no filename)</summary>
        public string DirectoryPath { get; set; }

        public bool? Exists { get; set; }

        /// <summary>Should This folder be purposely scanned?</summary>
        /// <remarks>Null=No Preference, True=Automatically scan this folder, False=Explicitly Ignore files in this folder.</remarks>
        public bool? ShouldMonitorFiles { get; set; }

        /// <summary>Is this folder intended for backups?</summary>
        /// <remarks>Null=Unknown, True=Automatically mark duplicates as the backup</remarks>
        public bool? IsBackupLocation { get; set; }


        /// <summary></summary>
        public string Notes { get; set; }

        /// <summary></summary>
        public HashSet<string> Tags { get; set; }

        /// <summary></summary>
        public Dictionary<string, string> Properties { get; set; }


        /// <summary>Drive (Because DriveLetters may change)</summary>
        public virtual Drive Drive { get; set; }

        public Directory()
        {
        }

        public Directory(DirectoryInfo info) : this()
        {
            CopyFrom(info);
        }

        public void CopyFrom(DirectoryInfo info)
        {
            info.ThrowIfArgumentNull(nameof(info));

            Exists = info.Exists;
            DriveLetter = info.GetDriveLetter();

            string path = info.FullName;
            if (Drive.ParseDriveLetter(ref path, out string driveLetter))
            {
                DirectoryPath = path;
                DriveLetter = driveLetter;
            }

            if (Drive != null && Drive.DriveLetter != DriveLetter)
            {
                throw new ArgumentOutOfRangeException(nameof(Drive), $"{nameof(Drive.DriveLetter)} does not match path {info.FullName}");
            }

        }

    }
}
