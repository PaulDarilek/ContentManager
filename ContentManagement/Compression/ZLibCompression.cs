using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;

namespace ContentManagement.Compression
{

    /// <summary>
    /// </summary>
    /// <remarks>
    /// The ZLIB trailer is a four-byte sequence that verifies the integrity of uncompressed data after decoding. 
    /// The sequence is 00 00 FF FF. 
    /// The ZLIB format also has a two-byte header that identifies the stream and provides decoding information.
    /// The first byte of the header, CMF, defines the compression method and flags. 
    /// The second byte, FLG, contains a checksum of the first two bytes, a bit for using a preset dictionary, and two bits for the compression level.
    /// Level | ZLIB  | GZIP 
    ///   0   | 78 01 | 1F 8B   NoCompression 
    ///   1   | 78 01 | 1F 8B   NoCompression / Low Compression
    ///   2   | 78 5E | 1F 8B   FastCompression
    ///   3   | 78 5E | 1F 8B   FastCompression
    ///   4   | 78 5E | 1F 8B   FastCompression
    ///   5   | 78 5E | 1F 8B   FastCompression
    ///   6   | 78 9C | 1F 8B   Deflate (Default)
    ///   7   | 78 DA | 1F 8B   BestCompression
    ///   8   | 78 DA | 1F 8B   BestCompression
    ///   9   | 78 DA | 1F 8B   BestCompression
    /// </remarks>
    public static class ZLibCompression
    {
        // write header:
        const int SizeOf_Header = 2;
        const int SizeOf_Trailer = 4;

        /// <summary>78 01 - No Compression/low</summary>
        public static byte[] Header_NoCompression { get; } = { 0x78, 0x01 };

        /// <summary>78 5E - Fast Compression</summary>
        public static byte[] Header_FastCompression { get; } = { 0x78, 0x5E };

        /// <summary>78 9C - Default Compression</summary>
        public static byte[] Header_Default { get; } = { 0x78, 0x9C };

        /// <summary>78 DA - Best Compression</summary>
        public static byte[] Header_BestCompression { get; } = { 0x78, 0xDA };


        /// <summary>Append Checksum of Compressed Data</summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static byte[] Adler32_Trailer(byte[] buffer, int offset, long count)
        {
            buffer.ThrowIfArgumentNull(nameof(buffer));
            if (offset < 0 || offset > buffer.Length) throw new ArgumentOutOfRangeException(nameof(offset));
            if (count < 0 || offset + count > buffer.Length) throw new ArgumentOutOfRangeException(nameof(count));

            uint a1 = 1, a2 = 0;
            for (int i = 0; i < count; i++)
            {
                byte b = buffer[offset + i];
                a1 = (a1 + b) % 65521;
                a2 = (a2 + a1) % 65521;
            }

            return new byte[]
            {
                (byte)(a2 >> 8),
                (byte)a2,
                (byte)(a1 >> 8),
                (byte)a1,
            };
        }

        public static byte[] ZLibCompress(this byte[] input, CompressionLevel level = CompressionLevel.Optimal)
        {
            input.ThrowIfArgumentNull(nameof(input));
            byte[] compressed;
            using (var memStream = new MemoryStream())
            {
                // write header:
                switch (level)
                {
                    case CompressionLevel.NoCompression:
                        memStream.Write(Header_NoCompression, 0, 2);
                        break;

                    case CompressionLevel.Fastest:
                        memStream.Write(Header_FastCompression, 0, 2);
                        break;

                    //case CompressionLevel.SmallestSize:
                    //    memStream.Write(Header_BestCompression, 0, 2);
                    //    break;

                    case CompressionLevel.Optimal:
                        memStream.Write(Header_Default, 0, 2);
                        break;

                    default: //Default Deflate
                        memStream.Write(Header_Default, 0, 2);
                        break;
                }

                using (var compressionStream = new DeflateStream(memStream, level, leaveOpen: true))
                {
                    compressionStream.Write(input, 0, input.Length);
                    compressionStream.Close();
                }

                // write trailer
                var prevPosition = memStream.Position;
                var length = memStream.Length;
                var buffer = memStream.GetBuffer();
                var trailer = Adler32_Trailer(buffer, 0, length);

                memStream.Position = prevPosition;
                memStream.Write(trailer, 0, 4);

                compressed = memStream.ToArray();
                memStream.Close();
            }
            return compressed;
        }


        public static byte[] ZLibDeCompress(this byte[] input)
        {

            input.ThrowIfArgumentNull(nameof(input));

            // calculate amd verify trailer checksum
            var trailer = Adler32_Trailer(input, 0, input.Length - SizeOf_Trailer);
            for (int i = 0; i < trailer.Length; i++)
            {
                int offSet = input.Length - SizeOf_Trailer + i;
                byte inputByte = input[offSet];
                byte trailerByte = trailer[i];
                Debug.Assert(inputByte == trailerByte);
            }

            byte[] decompressed = input;
            using (var memOutput = new MemoryStream())
            {
                using (var memInput = new MemoryStream(input, SizeOf_Header, input.Length - SizeOf_Header - SizeOf_Trailer))
                {
                    using (var compressionStream = new DeflateStream(memInput, CompressionMode.Decompress))
                    {
                        compressionStream.CopyTo(memOutput);
                        compressionStream.Close();
                    }
                    memInput.Close();
                }
                decompressed = memOutput.ToArray();
                memOutput.Close();
            }

            return decompressed;
        }
    }

}