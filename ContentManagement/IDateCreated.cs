using System;

namespace ContentManagement
{
    public interface IDateCreated
    {
        /// <summary>Date Document was created</summary>
        /// <remarks>When Created (from <see cref="IDateCreated"/>) Example Source is <see cref="DateTime.UtcNow"/> or <see cref="FileSystemInfo.CreationTimeUtc"/></remarks>
        DateTime? DateCreatedUtc { get; set; }

    }


}
