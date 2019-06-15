using System.Collections.Generic;

/* Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *      http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace org.activiti.engine.impl.history
{

    /// <summary>
    /// Enum that contains all possible history-levels.
    /// 
    /// 
    /// </summary>
    public sealed class HistoryLevel : IComparer<HistoryLevel>
    {
        public static readonly HistoryLevel NONE = new HistoryLevel("NONE", InnerEnum.NONE, "none");
        public static readonly HistoryLevel ACTIVITY = new HistoryLevel("ACTIVITY", InnerEnum.ACTIVITY, "activity");
        public static readonly HistoryLevel AUDIT = new HistoryLevel("AUDIT", InnerEnum.AUDIT, "audit");
        public static readonly HistoryLevel FULL = new HistoryLevel("FULL", InnerEnum.FULL, "full");

        private static readonly IList<HistoryLevel> valueList = new List<HistoryLevel>();

        static HistoryLevel()
        {
            valueList.Add(NONE);
            valueList.Add(ACTIVITY);
            valueList.Add(AUDIT);
            valueList.Add(FULL);
        }

        public enum InnerEnum
        {
            NONE = 0x00,
            ACTIVITY = 0x01,
            AUDIT = 0x10,
            FULL = 0x11
        }

        public readonly InnerEnum innerEnumValue;
        private readonly string nameValue;
        private readonly int ordinalValue;
        private static int nextOrdinal = 0;

        private readonly string key;

        private HistoryLevel(string name, InnerEnum innerEnum, string key)
        {
            this.key = key;

            nameValue = name;
            ordinalValue = nextOrdinal++;
            innerEnumValue = innerEnum;
        }

        /// <param name="key">
        ///          string representation of level </param>
        /// <returns> <seealso cref="HistoryLevel"/> for the given key </returns>
        /// <exception cref="ActivitiException">
        ///           when passed in key doesn't correspond to existing level </exception>
        public static HistoryLevel GetHistoryLevelForKey(string key)
        {
            foreach (HistoryLevel level in Values())
            {
                if (level.key.Equals(key))
                {
                    return level;
                }
            }
            throw new ActivitiIllegalArgumentException("Illegal value for history-level: " + key);
        }

        /// <summary>
        /// String representation of this history-level.
        /// </summary>
        public string Key
        {
            get
            {
                return key;
            }
        }

        /// <summary>
        /// Checks if the given level is the same as, or higher in order than the level this method is executed on.
        /// </summary>
        public bool IsAtLeast(HistoryLevel level)
        {
            // Comparing enums actually compares the location of values declared in
            // the enum
            return Compare(this, level) >= 0;
        }

        public static IList<HistoryLevel> Values()
        {
            return valueList;
        }

        public int Ordinal()
        {
            return ordinalValue;
        }

        public override string ToString()
        {
            return nameValue;
        }

        public static HistoryLevel ValueOf(string name)
        {
            foreach (HistoryLevel enumInstance in valueList)
            {
                if (enumInstance.nameValue == name)
                {
                    return enumInstance;
                }
            }
            throw new System.ArgumentException(name);
        }

        public int Compare(HistoryLevel x, HistoryLevel y)
        {
            return x.innerEnumValue.CompareTo(y.innerEnumValue);
        }
    }
}