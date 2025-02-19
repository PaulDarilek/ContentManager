using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ContentManagement.Compression
{
    public static class Crc32
    {
        public const uint MinBufferSize = 512;
        public const uint MaxBufferSize = 65536;
        public const uint DefaultBufferSize = 4096;
        public static uint BufferSize { get; set; } = DefaultBufferSize;

        private static uint[] Crc32Table { get; }

        static Crc32()
        {
            // Initialize the CRC32 table
            uint dwPolynomial = 0xEDB88320;
            var crc = new uint[256];
            for (uint i = 0; i < 256; i++)
            {
                uint dwCrc = i;
                for (int j = 8; j > 0; j--)
                {
                    if ((dwCrc & 1) == 1)
                    {
                        dwCrc = dwCrc >> 1 ^ dwPolynomial;
                    }
                    else
                    {
                        dwCrc >>= 1;
                    }
                }
                crc[i] = dwCrc;
            }
            Crc32Table = crc;
        }

        public static uint ToHashCrc32(this byte[] buffer)
        {
            buffer.ThrowIfArgumentNull(nameof(buffer));
            uint crc32 = buffer.GetCrc32Async().GetAwaiter().GetResult();
            return crc32;
        }

        public static uint ToHashCrc32(this Stream input)
        {
            input.ThrowIfArgumentNull(nameof(input));
            uint crc32 = input.GetCrc32Async().GetAwaiter().GetResult();
            return crc32;
        }

        public static Task<uint> GetCrc32Async(this byte[] buffer)
            => new MemoryStream(buffer).GetCrc32Async();

        public static async Task<uint> GetCrc32Async(this Stream stream, CancellationToken cancellationToken = default)
        {
            unchecked
            {
                uint crc32Result = 0xFFFFFFFF;

                uint bufferSize = Math.Min(MaxBufferSize, Math.Max(BufferSize, MinBufferSize));
                byte[] buffer = new byte[bufferSize];

                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);

                while (bytesRead > 0)
                {
                    for (int i = 0; i < bytesRead; i++)
                    {
                        crc32Result = crc32Result >> 8 ^ Crc32Table[buffer[i] ^ crc32Result & 0x000000FF];
                    }
                    bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                }

                return ~crc32Result;
            }
        }
    }
}
