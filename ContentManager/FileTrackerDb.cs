

namespace ContentManager;

public class FileTrackerDb(string connectionString)
{
    public string ConnectionString { get; init; } = connectionString;

    internal void Delete(string filespec)
    {
        throw new NotImplementedException();
    }

    internal void Hash(string filespec)
    {
        throw new NotImplementedException();
    }

    internal void Restore(string filespec)
    {
        throw new NotImplementedException();
    }

    internal void Scan(string filespec)
    {
        throw new NotImplementedException();
    }

    internal void Verify(string filespec)
    {
        throw new NotImplementedException();
    }



}
