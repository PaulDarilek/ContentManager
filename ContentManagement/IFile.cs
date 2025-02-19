using System.IO;

namespace ContentManagement
{
    public interface IFile : IDateCreated, IDateUpdated, IDateRetention, IHashCode, INotes, ITags, IProperties
    {
        int FileId { get; set; }
        string FileName { get; set; }
        string DriveLetter { get; set; }
        int? DriveId { get; set; }
        string DirectoryPath { get; set; }
        FileAttributes Attributes { get; set; }
        bool? Exists { get; set; }
        bool IsReadOnly { get; set; }
        long? Length { get; set; }

        void ComputeHash(bool force = false);
    }
}