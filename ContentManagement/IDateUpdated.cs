using System;

namespace ContentManagement
{
    public interface IDateUpdated
    {
        /// <summary>Date Document was Last Updated</summary>
        /// <remarks>When last Updated (from <see cref="IDateUpdated"/>) Example Source is <see cref="DateTime.UtcNow"/> or <see cref="System.IO.FileSystemInfo.LastWriteTimeUtc"/></remarks>
        DateTime? DateUpdatedUtc { get; set; }

    }


}
