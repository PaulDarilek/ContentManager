using System.Collections.Generic;

namespace ContentManagement.Interfaces
{
    public interface ITags
    {

        /// <summary>Unique Tags</summary>
        /// <remarks>stored as comma separated text on database</remarks>
        HashSet<string> Tags { get; set; }

    }


}
