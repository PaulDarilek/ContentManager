namespace ContentManagement.Interfaces
{
    public interface IReadWriteData : IReadData, IWriteData
    {

    }

    public interface IReadData
    {
        /// <summary>Is it available for read? (Could be offline)</summary>
        bool? CanReadData { get; }

        /// <summary>Method May need to read the filesystem, or may come from a database, or cloud blob</summary>
        byte[] ReadData();
    }

    public interface IWriteData
    {
        /// <summary>Can Data be updated (false for Blue-Ray or read-only system)</summary>
        bool? CanWriteData { get; }

        /// <summary>Tries to Store the Data.</summary>
        /// <remarks>Errors returned in <paramref name="errorMessage"/>.</remarks>
        bool WriteData(byte[] data, out string errorMessage);
    }


}
