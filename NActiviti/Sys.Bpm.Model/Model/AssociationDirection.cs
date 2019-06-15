using System;
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
namespace org.activiti.bpmn.model
{
    public sealed class AssociationDirection
    {
        public static readonly AssociationDirection NONE = new AssociationDirection("NONE", InnerEnum.NONE, "None");
        public static readonly AssociationDirection ONE = new AssociationDirection("ONE", InnerEnum.ONE, "One");
        public static readonly AssociationDirection BOTH = new AssociationDirection("BOTH", InnerEnum.BOTH, "Both");

        private static readonly IList<AssociationDirection> valueList = new List<AssociationDirection>();

        static AssociationDirection()
        {
            valueList.Add(NONE);
            valueList.Add(ONE);
            valueList.Add(BOTH);
        }

        public enum InnerEnum
        {
            NONE,
            ONE,
            BOTH
        }

        public readonly InnerEnum innerEnumValue;
        private readonly string nameValue;
        private readonly int ordinalValue;
        private static int nextOrdinal = 0;
        internal string value;

        internal AssociationDirection(string name, InnerEnum innerEnum, string value)
        {
            this.value = value;

            nameValue = name;
            ordinalValue = nextOrdinal++;
            innerEnumValue = innerEnum;
        }

        public string Value
        {
            get
            {
                return value;
            }
        }

        public static IList<AssociationDirection> Values()
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

        public static AssociationDirection ValueOf(string name)
        {
            foreach (AssociationDirection enumInstance in valueList)
            {
                if (enumInstance.nameValue == name)
                {
                    return enumInstance;
                }
            }
            throw new ArgumentException(name);
        }
    }

}