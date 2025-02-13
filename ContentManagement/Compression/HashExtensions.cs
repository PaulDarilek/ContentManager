using System;
using System.IO;

namespace ContentManagement.Compression
{

    /// <summary>Helper Methods for Computing a Hash</summary>
    public static class HashExtensions
    {

        public static string ToHashSha1Base64(this Stream input) => input.ToHashSha1().ToBase64();

        public static string ToHashSha1Base64(this byte[] buffer) => buffer.ToHashSha1().ToBase64();

        public static string ToHashSha1Hex(this Stream input) => input.ToHashSha1().ToHexString();
        public static string ToHashSha1Hex(this byte[] buffer) => buffer.ToHashSha1().ToHexString();


        private static byte[] ToHashSha1(this byte[] buffer)
        {
            buffer.ThrowIfArgumentNull(nameof(buffer));
            using (var hasher = System.Security.Cryptography.SHA1.Create())
            {
                byte[] hashBytes = hasher.ComputeHash(buffer);
                return hashBytes;
            }
        }

        private static byte[] ToHashSha1(this Stream input)
        {
            input.ThrowIfArgumentNull(nameof(input));
            using (var hasher = System.Security.Cryptography.SHA1.Create())
            {
                byte[] hashBytes = hasher.ComputeHash(input);
                return hashBytes;
            }
        }



        public static string ToBase64(this byte[] bytes) => Convert.ToBase64String(bytes);


        public static string ToHexString(this byte[] bytes, bool lowercase = false)
        {
            string hex =
                lowercase ?
                "0123456789abcdef" :
                "0123456789ABCDEF";

            char[] charArr = new char[bytes.Length * 2];
            byte b;
            int nibble;
            for (int i = 0; i < bytes.Length; i++)
            {
                b = bytes[i];
                nibble = b >> 4;
                charArr[i * 2] = hex[nibble];

                nibble = b & 0xF;
                charArr[i * 2 + 1] = hex[nibble];
            }
            return new string(charArr);
        }

    }
}