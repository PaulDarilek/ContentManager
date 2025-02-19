using ContentManagement;
using ContentManagement.Compression;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DocumentManagement
{
    public class DocumentBlob : IHashCode, IReadWriteData
    {
        public static short MinimumSizeToCompress { get; set; } = 1024;

        /// <summary></summary>
        [Required]
        public Guid? DocumentGuid { get; set; }

        /// <summary>Sequence Number</summary>
        [Required]
        public ushort BlobNumber { get; set; }

        /// <summary>Extension including leading period</summary>
        public string FileExtension { get; set; }

        /// <summary>MimeType</summary>
        /// <remarks>Optional (could be determined by FileExtension)</remarks>
        public string MimeType { get; set; }

        /// <summary>Hash Code Base64 encrypted</summary>
        public string HashCode { get; set; }

        /// <summary>Cyclic Redundancy Counter</summary>
        public uint? Crc32 { get; set; }

        /// <summary>Uncompressed Length</summary>
        public int? Length { get; set; }

        /// <summary>Table Storaage Bits</summary>
        /// <remarks>Use <see cref="ReadData"/> or <see cref="WriteData(byte[], out string)"/>, because <<see cref="Data"/> may contain compressed or uncompresed data.</remarks>
        [JsonIgnore]
        public byte[] Data { get; protected set; }

        // IReadWriteData, IReadData
        public bool? CanReadData { get; protected set; } = true;

        // IReadWriteData, IWriteData
        public bool? CanWriteData { get; protected set; } = true;

        public virtual Document Document { get; set; }


        public byte[] ReadData()
        {
            UncompressDataIfNeeded();
            return Data;
        }

        public bool WriteData(byte[] data, out string errorMessage)
        {
            errorMessage = string.Empty;
            try
            {
                Data = data;
                Length = Data?.Length;
                HashCode = Data?.ToHashSha1Base64();
                Crc32 = Data?.ToHashCrc32();

                CompressDataIfNeeded();
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = $"{ex.GetType().Name}: {ex.Message}";
                return false;
            }
        }

        public bool? IsCompressed => Length == null || Data == null ? (bool?)null : Length != Data.Length;

        /// <summary>Sqlite Archive uses ZlibDeflate</summary>
        /// <remarks>
        ///     From SQL Lite website:
        ///     Both directories and empty files have sqlar.sz==0. Directories can be distinguished from empty files because directories have sqlar.data IS NULL. 
        ///     The file is compressed if length(sqlar.blob) == sqlar.sz and is stored as plaintext if length(sqlar.blob)==sqlar.sz.
        ///     Symbolic links have sqlar.sz set to -1, and the link target stored as a text value in the sqlar.data field.
        ///     SQLAR uses the "zlib format" for compression.ZIP uses the raw deflate format. 
        ///     The difference is that the zlib format contains a two byte compression-type indentification header (0x78 0x9c) and a 4-byte checksum at the end.
        ///     Thus the "data" for SQLAR is always 6 bytes larger than the equivalent data for ZIP.
        ///     The SQLAR program uses the zlib format rather than the slightly smaller raw deflate format because that is what the zlib documentation recommends.
        ///     SQLAR might someday be extended to support additional compression formats other than deflate.
        ///     If so, the data field will contain new header values to identify entries compressed using the new formats.
        /// </remarks>
        public bool? IsZlibDeflate()
        {
            if (Length == null || Data == null)
                return null;

            return
                Length != Data.Length &&
                Data?.Length >= 2 &&
                Data[0] == ZLibCompression.Header_Default[0] &&
                Data[1] == ZLibCompression.Header_Default[1];
        }

        public void CompressDataIfNeeded()
        {
            if (Data == null || Length != Data.Length || Data.Length < MinimumSizeToCompress)
                return;

            byte[] compressed = Data.CompressWithDeflate();

            if (compressed.Length < Data.Length)
            {
                Data = compressed;
            }
        }

        public void UncompressDataIfNeeded()
        {
            if (Data != null && Length != null && Length != Data.Length)
            {
                byte[] uncompressed =
                       IsZlibDeflate() == true ?
                       Data.ZLibDeCompress() :
                       Data.UncompressWithDeflate();

                if (uncompressed.Length == Length)
                {
                    Data = uncompressed;
                    return;
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(Length), $"Decompressed {nameof(Data)} = {uncompressed.Length} bytes instead of {Length} bytes");
                }
            }
        }

    }
}
