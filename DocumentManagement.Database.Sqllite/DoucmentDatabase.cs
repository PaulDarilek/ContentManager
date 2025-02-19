using DocumentManagement;
using Microsoft.Data.Sqlite;
using System.Data;

namespace DocumentManagement.Database.Sqllite;

/// <summary>Constructor</summary>
/// <param name="connectionString">String like $"Data Source={dbFile.FullName}"</param>
public class DoucmentDatabase(string connectionString) : IDocumentDatabase
{

    public DocumentDbContext Context { get; } = new DocumentDbContext(connectionString);

    public IQueryable<Document> Documents => Context.Documents;
    public IQueryable<DocumentBlob> Blobs => Context.Blobs;


    public int SaveChanges() => Context.SaveChanges(acceptAllChangesOnSuccess: true);

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

