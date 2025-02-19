using ContentManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace FileManagement
{

    public class Drive : IDateCreated, IDateUpdated, INotes, ITags, IProperties
    {
        /// <summary>Identity Column</summary>
        public int DriveId { get; set; }

        /// <summary>Fixed,Removable,Network,CDRom, etc</summary>
        /// <remarks>From DriveInfo</remarks>
        [Required]
        public string DriveType { get; set; }

        /// <summary>Drive Letter (C:) or Drive Path (/root/hda/) or UNC Share path (\\MachineName\Share\)</summary>
        /// <remarks>From DriveInfo. Can be changed in DiskMgmt.msc</remarks>
        [Required]
        public string DriveLetter { get; set; }

        /// <summary>Machines that the drive has been attached to (Comma separated)</summary>
        /// <remarks>Defaults to <see cref="Environment.MachineName"/></remarks>
        public HashSet<string> MachineNames { get; set; }

        /// <summary>Size of Drive</summary>
        /// <remarks>From DriveInto (Static size from Partitioning)</remarks>
        public long TotalSize { get; set; }

        /// <summary>Free Space on Drive</summary>
        /// <remarks>DriveInfo property only available if IsReady</remarks>
        public long? TotalFreeSpace { get; set; }

        /// <summary>NTFS, FAT32, EXT4, etc</summary>
        /// <remarks>DriveInfo property only available if IsReady</remarks>
        public string DriveFormat { get; set; }

        /// <summary>Volume Serial Number (Created during format of drive)</summary>
        /// <remarks>Set by Formatting the drive, Read by VOL command  (Vol C:)</remarks>
        public string VolumeSerialNumber { get; set; }

        /// <summary>Volume Label in OS</summary>
        /// <remarks>Label can be set during format, but changed at any time (Read by VOL command)</remarks>
        public string VolumeLabel { get; set; }

        /// <summary>When First Scanned</summary>
        /// <remarks>Should be first time added to database</remarks>
        public DateTime? DateCreatedUtc { get; set; } = DateTime.UtcNow;

        /// <summary>Last Time Drive was Scanned and Ready</summary>
        /// <remarks>Updated every time Drive is Scanned</remarks>
        public DateTime? DateUpdatedUtc { get; set; }

        ///// <summary>Drive Guid or Unique Identifier</summary>
        ///// <remarks>(Requires WMI) Useful for External/USB Drives that may change letters or mounting points</remarks>
        public string HardwareSerialNumber { get; set; }

        ///// <summary>Drive Guid or Unique Identifier</summary>
        ///// <remarks>(Requires WMI) Useful for External/USB Drives that may change letters or mounting points</remarks>
        public string Model { get; set; }

        /// <summary>Unique Tags</summary>
        public HashSet<string> Tags { get; set; }

        /// <summary></summary>
        public Dictionary<string, string> Properties { get; set; }

        ///// <summary>Free Form Notes</summary>
        public string Notes { get; set; }

        /// <summary>Fixed Drives are ususally Ready</summary>
        [NotMapped]
        public bool? IsReady { get; set; }


        /// <summary>List of Directories on the drive</summary>
        public virtual List<Directory> Directories { get; set; }

        public Drive() : base()
        {
            EnsureNotNull();
        }


        public Drive(DriveInfo drive)
        {
            EnsureNotNull();
            AddCurrentMachineName();
            DriveLetter = drive.Name?.ToUpper();
            DriveType = drive.DriveType.ToString();
            IsReady = drive.IsReady;
            Debug.Assert(drive.IsReady);
            if (drive.IsReady)
            {
                DriveFormat = drive.DriveFormat;
                TotalSize = drive.TotalSize;
                VolumeLabel = drive.VolumeLabel;
                TotalFreeSpace = drive.TotalFreeSpace;
                VolumeSerialNumber = drive.GetVolSerialNumber();
            }

            //Were Reading it right now from the DriveInfo
            DateUpdatedUtc = DateTime.UtcNow;
        }

        public void EnsureNotNull()
        {
            MachineNames = MachineNames ?? new HashSet<string>();
            Tags = Tags ?? new HashSet<string>();
        }

        public bool AddCurrentMachineName() => MachineNames.Add(Environment.MachineName);

        public void UnionWith(Drive other)
        {
            if (other == null) return;

            other.EnsureNotNull();

            // Merge the machines this drive has been attached to.
            MachineNames.UnionWith(other.MachineNames);

            // Things that can change (prefer the other)
            IsReady = other.IsReady ?? IsReady;
            DateUpdatedUtc = other.DateUpdatedUtc ?? DateUpdatedUtc ?? DateTime.UtcNow;
            DriveLetter = other.DriveLetter.FirstNotNullOrWhiteSpace(DriveLetter);
            DriveFormat = other.DriveFormat.FirstNotNullOrWhiteSpace(DriveFormat);
            TotalFreeSpace = other.TotalFreeSpace ?? TotalFreeSpace;
            VolumeLabel = other.VolumeLabel.FirstNotNullOrWhiteSpace(VolumeLabel);
            Notes = other.Notes.FirstNotNullOrWhiteSpace(Notes);

            // Things that should not change often, so only update if missing.
            DriveId = DriveId != default ? DriveId : other.DriveId;
            DateCreatedUtc = DateCreatedUtc ?? other.DateCreatedUtc;
            DriveType = DriveType.FirstNotNullOrWhiteSpace(other.DriveType);
            VolumeSerialNumber = VolumeSerialNumber.FirstNotNullOrWhiteSpace(other.VolumeSerialNumber);
            HardwareSerialNumber = HardwareSerialNumber.FirstNotNullOrWhiteSpace(other.HardwareSerialNumber);
            Model = Model.FirstNotNullOrWhiteSpace(other.Model);
            TotalSize = TotalSize > 0 ? TotalSize : other.TotalSize;
        }

        /// <summary>Returns the Drive Letter and removes it from a fully qualified path</summary>
        /// <remarks>A path like C:\Temp\ should return C: as the drive letter and modify path to \Temp\</remarks>
        /// <returns>whether a drive letter was detected. driveLetter will be string.empty false is returned</returns>
        public static bool ParseDriveLetter(ref string path, out string driveLetter)
        {
            driveLetter = string.Empty;

            if (string.IsNullOrEmpty(path)) return false;

            path = Path.GetFullPath(path);

            if (path.Length > 2 && path[1] == ':' && (path[2] == Path.DirectorySeparatorChar || path[2] == Path.AltDirectorySeparatorChar))
            {
                driveLetter = path.Substring(0, 3).ToUpper();
                path = path.Substring(3);
            }

            return driveLetter.Length > 0;
        }



    } // class Drive

    public static class DriveExtensions
    {

        /// <summary>Returns the upper case drive letter and colon (C: not c: or C:\)</summary>
        /// <remarks>returns null if <paramref name="info"/> is null or doesn't have a <see cref="FileSystemInfo.FullName"/> that starts with a Drive Letter</remarks>
        public static string GetDriveLetter(this DirectoryInfo info)
        {
            if (info == null) return null;
            var root = Path.GetPathRoot(info.FullName);
            return
                string.IsNullOrEmpty(root) ?
                null :
                char.IsUpper(root[0]) ?
                root :
                root.ToUpper();
        }

        /// <summary>Returns the upper case drive letter and colon (C: not c: or C:\)</summary>
        /// <remarks>returns null if <paramref name="info"/> is null or doesn't have a <see cref="FileSystemInfo.FullName"/> that starts with a Drive Letter</remarks>
        public static string GetDriveLetter(this FileInfo info) => info.Directory.GetDriveLetter();

        /// <summary>Returns the Drive's Serial Number using the VOL command</summary>
        /// <param name="driveLetter"></param>
        /// <returns>Serial Number if it exits</returns>
        /// <remarks>
        ///  Volume in drive C is OSDISK
        ///  Volume Serial Number is 1234-5678
        /// </remarks>
        public static string GetVolSerialNumber(this DriveInfo drive)
        {
            bool isWindows =
                Environment.OSVersion.Platform == PlatformID.Win32NT ||
                Environment.OSVersion.Platform == PlatformID.Win32Windows ||
                Environment.OSVersion.Platform == PlatformID.Win32S ||
                Environment.OSVersion.Platform == PlatformID.WinCE;

            if (!isWindows)
                return null;

            char[] splitChars = { '\n' };

            string driveLetter =
                drive.Name?.Length == 1 ? drive.Name + ':' :
                drive.Name?.Length == 2 ? drive.Name :
                drive.Name?.Length > 2 ? drive.Name.Substring(0, 2) :
                null;

            if (driveLetter == null) return null;

            var stdOut = ExecCmdReturnStdOut($"VOL {driveLetter}");
            var lines = stdOut.Split(splitChars, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();

            const string search = "Volume Serial Number is ";
            var serial = lines.FirstOrDefault(x => x.StartsWith(search))?.Substring(search.Length);

            return serial?.Replace("-", "");
        }

        private static string ExecCmdReturnStdOut(string command)
        {

            try
            {
                // create the ProcessStartInfo using "cmd" as the program to be run,
                // and "/c " as the parameters.
                // Incidentally, /c tells cmd that we want it to execute the command that follows,
                // and then exit.
                string cmdPath = Path.Combine(Environment.ExpandEnvironmentVariables("%windir%"), "System32", "cmd.exe");
                var procStartInfo = new ProcessStartInfo(cmdPath, "/c " + command)
                {
                    // The following commands are needed to redirect the standard output.
                    // This means that it will be redirected to the Process.StandardOutput StreamReader.
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    // Do not create the black window.
                    CreateNoWindow = true
                };
                // Now we create a process, assign its ProcessStartInfo and start it
                var proc = new Process
                {
                    StartInfo = procStartInfo
                };

                proc.Start();
                // Get the output into a string
                string stdOut = proc.StandardOutput.ReadToEnd();
                // Display the command output.
                return stdOut;
            }
            catch (Exception ex)
            {
                // Log the exception
                Trace.WriteLine(ex);
                throw;
            }
        }

    }
}