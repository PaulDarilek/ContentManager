using System;

namespace ContentManagement
{

    public interface IDateRetention
    {
        /// <summary>Retention Date</summary>
        /// <remarks>if set, the content may be deleted after this date.</remarks>
        DateTime? DateRetention { get; set; }
    }


}
