using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FileManagement
{
    public class FileScanner
    {
        public HashSet<DirectoryInfo> DirectoriesToSkip { get; } = new HashSet<DirectoryInfo>();

        /// <summary>Get a List of Files</summary>
        /// <param name="searchPattern">path including directory and a filename/filespec (such as *.txt)</param>
        /// <param name="recursive">true to search subdirectories also</param>
        /// <returns></returns>
        public IEnumerable<FileInfo> EnumerateFiles(DirectoryInfo dirInfo, string searchPattern = "*", bool recursive = true, FileAttributes attributesToSkip = FileAttributes.Hidden | FileAttributes.System | FileAttributes.ReparsePoint)
        {
            if (!dirInfo.Exists)
                return Array.Empty<FileInfo>();

            var options = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            //var gitIngoreDirs =
            //    dirInfo.EnumerateFiles(".gitignore", options)
            //    .Select(x => x.DirectoryName)
            //    .ToList();

            //var ignore = new GitignoreParserNet.GitignoreParser();


            searchPattern =
                string.IsNullOrEmpty(searchPattern) ? "*" :
                searchPattern == "*.*" ? "*" :
                searchPattern;


            var files = dirInfo.EnumerateFiles(searchPattern, options);

            bool filter = DirectoriesToSkip.Any(dir => dir.FullName.StartsWith(dirInfo.FullName));
            if (filter)
            {
                // add filter for Attributes.
                files = files.Where(f => f.Directory == null || !DirectoriesToSkip.Contains(f.Directory));
            }

            return files;
        }

        public void AllowDirectoryAndChildren(DirectoryInfo dirInfo)
                => DirectoriesToSkip.RemoveWhere(dir => dir.FullName.StartsWith(dirInfo.FullName));

        public void SkipDirectoriesWithAttributes(IEnumerable<DirectoryInfo> directories, bool skipChildrenToo = true, FileAttributes attributesToSkip = FileAttributes.System | FileAttributes.Hidden | FileAttributes.ReparsePoint)
        {
            if (directories == null)
                return;

            foreach (var dir in directories)
            {
                if ((dir.Attributes & attributesToSkip) != 0)
                {
                    DirectoriesToSkip.Add(dir);
                }
                else if (skipChildrenToo && dir.Parent != null && DirectoriesToSkip.Contains(dir.Parent))
                {
                    // skip because of their parents
                    DirectoriesToSkip.Add(dir);
                }
            }
        }

    }
}
