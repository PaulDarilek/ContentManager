namespace ContentManagement
{
    public interface IHashCode
    {
        /// <summary>Sha1 Hash Base64 Encoded</summary>
        string HashCode { get; set; }

        /// <summary>Zip compatible CRC32</summary>
        uint? Crc32 { get; set; }
    }


}