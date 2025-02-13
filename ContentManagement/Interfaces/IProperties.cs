using System.Collections.Generic;

namespace ContentManagement.Interfaces
{


    public interface IProperties 
    {
        /// <summary>Key/Value pairs such as Indexes or Properties</summary>
        /// <remarks>Each Key-Value pair is a line (\n separator) in form key=value.  Much like an INI file without Sections</remarks>
        Dictionary<string, string> Properties { get; set; }
    }


}
