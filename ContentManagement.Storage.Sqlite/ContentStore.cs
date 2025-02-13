using ContentManagement.Models;
using Microsoft.Data.Sqlite;
using System.Data;

namespace ContentManagement.Storage.Sqlite;

/// <summary>Constructor</summary>
/// <param name="connectionString">String like $"Data Source={dbFile.FullName}"</param>
public class ContentStore(string connectionString) : IContentStore
{

    public ContentStoreContext Context { get; } = new ContentStoreContext(connectionString);

    public IQueryable<Document> Documents => Context.Documents;
    public IQueryable<DocumentBlob> Blobs => Context.Blobs;

    public IQueryable<Drive> Drives => Context.Drives;
    public IQueryable<Models.Directory> Directories => Context.Directories;
    public IQueryable<Models.File> Files => Context.Files;
    public IQueryable<FileDuplicates> FileDuplicates => Context.FileDuplicates;

    public int SaveChanges() => Context.SaveChanges(acceptAllChangesOnSuccess: true);

    public void Add(Drive drive) {
        Context.Drives.Add(drive);
        //SaveChanges();
    }
    public void Add(Models.Directory directory)
    {
        Context.Directories.Add(directory);
        //SaveChanges();
    }

    public void Add(Models.File file)
    {
        Context.Files.Add(file);
        //SaveChanges();
    }

    public void Add(FileDuplicates item)
    {
        Context.FileDuplicates.Add(item);
        //SaveChanges();
    }

    public void Add(Document document)
    {
        Context.Documents.Add(document);
        //SaveChanges();
    }

    public Document? GetDocument(Guid documentGuid)
    {
        var doc = Documents.FirstOrDefault(x => x.DocumentGuid == documentGuid);
        return doc;
    }

    public Drive? FindOrAdd(DriveInfo drive) => FindOrAdd(new Drive(drive));

    public Drive? FindOrAdd(Drive? drive)
    {
        if (drive == null)
            return drive;

        var found =
            drive.DriveId != 0 ? Drives.FirstOrDefault(db => db.DriveId == drive.DriveId) :
            drive.VolumeSerialNumber != null ? Drives.FirstOrDefault(db => db.VolumeSerialNumber == drive.VolumeSerialNumber) :
            null;

        if (found != null)
            return found;

        var list = 
            Drives
            .Where(db => db.DriveFormat == drive.DriveFormat && db.TotalSize == drive.TotalSize && db.DriveType == drive.DriveType)
            .ToList();

        found =
            list.FirstOrDefault(db => db.DriveLetter == drive.DriveLetter && db.VolumeLabel == drive.VolumeLabel && db.HardwareSerialNumber == drive.HardwareSerialNumber) ??
            list.FirstOrDefault(db => db.DriveLetter == drive.DriveLetter && db.HardwareSerialNumber == drive.HardwareSerialNumber) ??
            list.FirstOrDefault(db => db.DriveLetter == drive.DriveLetter && db.VolumeLabel == drive.VolumeLabel) ??
            list.FirstOrDefault(db => db.VolumeLabel == drive.VolumeLabel && db.HardwareSerialNumber == drive.HardwareSerialNumber);

        if (found != null)
            return found;

        Add(drive);
        return drive;
    }

    public IEnumerable<Drive> FindDrives(string? driveLetter = null, string? volumeSerialNumber = null, string? volumeLabel = null, long? totalSize = null, string? machineName = null)
    {
        var query = Drives;

        if (!string.IsNullOrWhiteSpace(machineName))
            query = query.Where(x => x.MachineNames.Contains(machineName, StringComparer.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(driveLetter))
            query = query.Where(x => driveLetter.Equals(x.DriveLetter, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(volumeSerialNumber))
            query = query.Where(x => volumeSerialNumber.Equals(x.VolumeSerialNumber, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrEmpty(volumeLabel))
            query = query.Where(x => volumeLabel.Equals(x.VolumeLabel, StringComparison.OrdinalIgnoreCase));

        if (totalSize != null)
            query = query.Where(x => x.TotalSize == totalSize);

        return query;
    }

    public IEnumerable<Models.File> Find(FileInfo file, Func<DirectoryInfo?,Drive> driveLookup)
    {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentNullException.ThrowIfNull(file.DirectoryName);

        var query = Context.Files.Where(x => file.Name.Equals(x.FileName) && file.DirectoryName.Equals(x.DirectoryPath));

        var drive = driveLookup?.Invoke(file.Directory);

        if (drive != null)
        {
            query = query.Where(x => x.DriveId == drive.DriveId);
        }
        else
        {
            var driveLetter = file.GetDriveLetter();
            query = query.Where(x => x.DriveLetter == driveLetter);
        }

        var found = query.ToArray();
        return found;
    }


    /// <summary>Search for Documents</summary>
    public IEnumerable<Document> SearchDocuments(Func<Document, bool> predicate)
    {
        return Context.Documents.Where(x => predicate(x));
    }


    public void Backup(string destinationDbConnectionString, string? sourceConnectionString = null)
    {
        using var source = new SqliteConnection(sourceConnectionString ?? Context.ConnectionString);
        using var destination = new SqliteConnection(destinationDbConnectionString);
        source.BackupDatabase(destination);
    }

    public int ExecSqlReader(string sql, IEnumerable<KeyValuePair<string, object>> parameters, Action<IDataRecord> action)
    {
        using var connection = new SqliteConnection(Context.ConnectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = sql;

        if (parameters != null)
        {
            foreach (var parm in parameters)
            {
                command.Parameters.AddWithValue(parm.Key, parm.Value);
            }
        }

        using var reader = command.ExecuteReader();

        int count = 0;
        while (reader.Read())
        {
            count++;
            action?.Invoke(reader);
        }
        return count;
    }

}

