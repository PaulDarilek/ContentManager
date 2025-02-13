using System;
using System.IO;
using System.IO.Compression;

namespace ContentManagement.Compression
{
    public static class CompressionExtensions
    {
        private static byte[] Empty { get; } = new byte[0];

        public static byte[] CompressWithDeflate(this Stream input)
        {
            input.ThrowIfArgumentNull(nameof(input));

            using (var memStream = new MemoryStream())
            {
                using (var compressionStream = new DeflateStream(memStream, CompressionMode.Compress))
                {
                    input.CopyTo(compressionStream);
                    compressionStream.Close();
                }
                return memStream.ToArray();
            }
        }

        public static byte[] CompressWithDeflate(this byte[] uncompressedBytes)
        {
            if (uncompressedBytes == null || uncompressedBytes.Length == 0)
                return Empty;

            using (var memStream = new MemoryStream())
            {
                using (var compressionStream = new DeflateStream(memStream, CompressionMode.Compress, leaveOpen: true))
                {
                    compressionStream.Write(uncompressedBytes, 0, uncompressedBytes.Length);
                    compressionStream.Close();
                }

                byte[] compressed = memStream.ToArray();
                memStream.Close();
                return compressed;
            }
        }

        public static byte[] UncompressWithDeflate(this Stream input)
        {
            input.ThrowIfArgumentNull(nameof(input));

            using (var memStream = new MemoryStream())
            {
                using (var compressionStream = new DeflateStream(input, CompressionMode.Decompress))
                {
                    compressionStream.CopyTo(memStream);
                    compressionStream.Close();
                }
                byte[] uncompressed = memStream.ToArray();
                memStream.Close();
                return uncompressed;
            }
        }


        public static byte[] UncompressWithDeflate(this byte[] compressedBytes)
        {
            if (compressedBytes == null || compressedBytes.Length == 0)
                return Empty;

            using (var memOutput = new MemoryStream())
            {

                using (var memInput = new MemoryStream(compressedBytes))
                {
                    using (var compressionStream = new DeflateStream(memInput, CompressionMode.Decompress))
                    {
                        compressionStream.CopyTo(memOutput);
                        compressionStream.Close();
                    }
                    memInput.Close();
                }

                byte[] decompressed = memOutput.ToArray();
                memOutput.Close();
                return decompressed;
            }

        }

        /// <summary>Thow <see cref="ArgumentNullException"/> if value is null</summary>
        /// <exception cref="ArgumentNullException"></exception>
        internal static T ThrowIfArgumentNull<T>(this T value, string paramName) where T : class
        {
            return value ?? throw new ArgumentNullException(paramName);
        }

        internal static string ThrowIfArgumentNullOrEmpty(this string value, string paramName)
        {
            return !string.IsNullOrEmpty(value) ? value : throw new ArgumentNullException(paramName);
        }
    }
}
