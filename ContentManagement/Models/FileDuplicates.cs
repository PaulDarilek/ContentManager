using ContentManagement.Interfaces;
using System.Collections.Generic;
using System.Diagnostics;

namespace ContentManagement.Models
{

    /// <summary>Connector Table for Files that are suspected as duplicates</summary>
    public class FileDuplicates: INotes, ITags, IProperties
    {
        /// <summary>Lower <see cref="File.FileId"/> of the pair</summary>
        public int FirstId { get; set;}

        /// <summary>Higher <see cref="File.FileId"/> of the pair</summary>
        public int SecondId { get; set; }

        /// <summary>Optional Note related to this match</summary>
        public string Notes { get; set; }

        public HashSet<string> Tags { get; set; }

        public Dictionary<string, string> Properties { get; set; }


        /// <summary>Whether this these files are known to be duplicates</summary>
        /// <remarks>
        /// Null=File has not been compared, 
        /// False=They are not equal, or should not be compared (such as zero length files or standard .gitignores)
        /// True=They are a duplicates, maybe for backup purposes?
        /// </remarks>
        public bool? AreDuplicates { get; set; }

        /// <summary>Is the <see cref="First"/> File a backup?</summary>
        public bool? FirstIsABackup { get; set; }

        /// <summary>Is the <see cref="Second"/> file a backup?</summary>
        public bool? SecondIsABackup { get; set; }

        /// <summary>File Referenced by<see cref="FirstId"/></summary>
        public virtual File First { get; set; }

        /// <summary>File Referenced by<see cref="SecondId"/></summary>
        public virtual File Second { get; set; }


        public FileDuplicates()
        {
        }

        /// <summary>First Id is expected to be lower than second Id (to avoid two records with different orders</summary>
        public FileDuplicates  OrderIds()
        {
            // First Number is expected to be a lower number so we don't have two records with swapped numbers.
            FirstId = (First?.FileId ?? 0) > 0 ? First.FileId : FirstId;
            SecondId = (Second?.FileId ?? 0) > 0 ? Second.FileId : SecondId;

            if (FirstId > SecondId && FirstId != 0 && SecondId != 0)
            {
                // Tuple to swap values...
                (Second, First) = (First, Second);
                (SecondId, FirstId) = (FirstId, SecondId);
                (SecondIsABackup, FirstIsABackup) = (FirstIsABackup, SecondIsABackup);

                Debug.Assert(First == null || First.FileId == 0 || First.FileId == FirstId);
                Debug.Assert(Second == null || Second.FileId == 0 || Second.FileId == SecondId);
            }

            return this;
        }
    }
}
