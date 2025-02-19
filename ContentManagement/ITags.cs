using System.Collections.Generic;

namespace ContentManagement
{
    public interface ITags
    {

        /// <summary>Unique Tags</summary>
        /// <remarks>stored as comma separated text on database</remarks>
        HashSet<string> Tags { get; set; }

    }


}
