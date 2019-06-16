using System.Collections.Generic;
using System.Linq;

namespace Sys.Workflow.Engine.Impl.DB
{

    /// <summary>
    /// This class is used for auto-upgrade purposes.
    /// 
    /// The idea is that instances of this class are put in a sequential order, and that the current version is determined from the ACT_GE_PROPERTY table.
    /// 
    /// Since sometimes in the past, a version is ambiguous (eg. 5.12 => 5.12, 5.12.1, 5.12T) this class act as a wrapper with a smarter matches() method.
    /// 
    /// 
    /// </summary>
    public class ActivitiVersion
    {
        /// <summary>
        /// 
        /// </summary>
        protected internal string mainVersion;

        /// <summary>
        /// 
        /// </summary>
        protected internal IList<string> alternativeVersionStrings;

        /// <summary>
        /// 
        /// </summary>
        public ActivitiVersion(string mainVersion)
        {
            this.mainVersion = mainVersion;
            this.alternativeVersionStrings = new List<string>(new string[] { mainVersion });
        }

        /// <summary>
        /// 
        /// </summary>
        public ActivitiVersion(string mainVersion, IList<string> alternativeVersionStrings)
        {
            this.mainVersion = mainVersion;
            this.alternativeVersionStrings = alternativeVersionStrings;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual string MainVersion
        {
            get
            {
                return mainVersion;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual bool Matches(string version)
        {
            if (version.Equals(mainVersion))
            {
                return true;
            }
            else if (alternativeVersionStrings.Count > 0)
            {
                return alternativeVersionStrings.Contains(version);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override int GetHashCode()
        {
            return alternativeVersionStrings.Sum(x => x.GetHashCode()) >> 2;
        }

        /// <summary>
        /// 
        /// </summary>
        public override bool Equals(object obj)
        {
            if (!(obj is ActivitiVersion))
            {
                return false;
            }
            ActivitiVersion other = (ActivitiVersion)obj;
            bool mainVersionEqual = mainVersion.Equals(other.mainVersion);
            if (!mainVersionEqual)
            {
                return false;
            }
            else
            {
                if (alternativeVersionStrings != null)
                {
                    return alternativeVersionStrings == other.alternativeVersionStrings;
                }
                else
                {
                    return other.alternativeVersionStrings == null;
                }
            }
        }
    }
}