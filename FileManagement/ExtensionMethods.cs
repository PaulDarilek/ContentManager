using System;
using System.Collections.Generic;
using System.Text;

namespace FileManagement
{
    internal static class ExtensionMethods
    {
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
