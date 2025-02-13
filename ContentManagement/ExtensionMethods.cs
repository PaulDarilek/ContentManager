using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ContentManagement
{
    public static class ExtensionMethods
    {
        /// <summary>Parse (CR|LF) separated lines of text</summary>
        public static string[] SplitToLines(this string multiLineText, string delimitors = "\r\n", bool removeEmptyLines = true)
        {
            if(string.IsNullOrEmpty(multiLineText))
                return Array.Empty<string>();

            if (string.IsNullOrEmpty(delimitors))
                delimitors = Environment.NewLine;

            char[] separators = delimitors.ToCharArray();
            var options = removeEmptyLines ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None;

            return multiLineText.Split(separators, options);
        }

        /// <summary>Join a set of string into a single newline (\n) separated text</summary>
        public static string Join(this IEnumerable<string> lines, string separator = "\n") => string.Join(separator ?? string.Empty, lines);

        /// <summary>Return Key Value pairs for any strings like Key=Value</summary>
        /// <remarks>Must have an non-empty key before the equals sign '=', and the value afterwards can be empty string</remarks>
        /// <returns>Valid Key Value Pairs where Key is not null or empty.</returns>
        public static IEnumerable<KeyValuePair<string, string>> ParseKeyValuePairs(this IEnumerable<string> lines, char keyValueSeparator = '=')
        {
            foreach (var line in lines)
            {
                int indexOf = line?.IndexOf(keyValueSeparator) ?? -1;
                if (indexOf > 0) // ignore if equals starts the line.
                {
                    var key = line.Substring(0, indexOf).Trim();
                    var value = 
                        line.Length > (indexOf + 1) ? 
                        line.Substring(indexOf + 1).Trim() : 
                        string.Empty;
                    
                    yield return new KeyValuePair<string, string>(key, value);
                }
            }
        }

        public static Dictionary<string, string> ToDictionary(this string TextWithNewLinesAndEquals, string lineSeparators = "\r\n", char keyValueSeparator = '=', bool igoreCase = true)
            => TextWithNewLinesAndEquals.SplitToLines(lineSeparators).ParseKeyValuePairs(keyValueSeparator).ToDictionary(igoreCase);

        public static Dictionary<string, string> ToDictionary(this IEnumerable<KeyValuePair<string, string>> values, bool ingoreCase = true)
        {
            var comparer = ingoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
            var dict = new Dictionary<string, string>(comparer);
            if (values != null)
            {
                foreach (var pair in values)
                {
                    if (!string.IsNullOrWhiteSpace(pair.Key))
                    {
                        dict[pair.Key] = pair.Value;
                    }
                        
                }
            }

            return dict;
        }


        /// <summary>Combines dictionary values into a multiline string of key=value lines.</summary>
        /// <param name="values"></param>
        /// <param name="kvPairLineSeparator"></param>
        /// <param name="keyValueSeparator"></param>
        /// <returns></returns>
        public static string AsText(this IEnumerable<KeyValuePair<string, string>> values, string kvPairLineSeparator = "\n", char keyValueSeparator = '=')
            => string.Join(kvPairLineSeparator, values.Select(pair => $"{pair.Key}{keyValueSeparator}{pair.Value}"));


        /// <summary>Split a <paramref name="delimitors"/> separated string into distinct values eliminating duplicates (case insensitive)</summary>
        /// <remarks><paramref name="delimitors"/>defaults to "\r\n" to allow splitting text files</remarks>
        /// <param name="delimitedString">string with values separated by <paramref name="delimitors"/>.</param>
        /// <param name="delimitors">defaults to a comma</param>
        /// <returns></returns>

        public static HashSet<string> ToHashSet(this string delimitedString, string delimitors = "\r\n")
            => new HashSet<string>(delimitedString.SplitToLines(delimitors, removeEmptyLines: true), StringComparer.OrdinalIgnoreCase);


        /// <summary>Modify <paramref name="properties"/> to include elemens in itself, <paramref name="keyValuePairs"/> or both</summary>
        /// <param name="keyValuePairs">Updated key/value pairs</param>
        /// <param name="properties">Dictionary to Update</param>
        /// <param name="keyValuePairs">New (and optionally replaced) values</param>
        /// <param name="replace">true to update values for existing keys</param>
        [DebuggerStepThrough]
        public static void UnionWith<TKey, TValue>(this IDictionary<TKey, TValue> properties, IEnumerable<KeyValuePair<TKey, TValue>> keyValuePairs, bool replace = false)
        {
            if (properties == null || keyValuePairs == null) return;

            foreach (var item in keyValuePairs)
            {
                if(!properties.ContainsKey(item.Key))
                {
                    properties.Add(item.Key, item.Value);
                }
                else if(replace)
                {
                    properties[item.Key] = item.Value;
                }
            }
        }

        [DebuggerStepThrough]
        public static string FirstNotNullOrWhiteSpace(this string original, string otherValue) 
            => !string.IsNullOrWhiteSpace(original) ? original : !string.IsNullOrWhiteSpace(otherValue) ? original : string.Empty;

        [DebuggerStepThrough]
        public static string FirstNotNullOrWhiteSpace(this string original, params string[] otherValues)
        {
            if(!string.IsNullOrWhiteSpace(original))
                return original;
            return otherValues.FirstOrDefault(x=> !string.IsNullOrWhiteSpace(x)) ?? string.Empty;
        }

    }
}
