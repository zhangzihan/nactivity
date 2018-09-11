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

namespace org.activiti.engine.impl.variable
{
    /// <summary>
    /// Subclass of <seealso cref="JPAEntityListVariableType"/> which is cacheable, unlike the super-class. This is used when fetching historic variables
    /// 
    /// 
    /// </summary>
    public class HistoricJPAEntityListVariableType : JPAEntityListVariableType
    {

        private static readonly HistoricJPAEntityListVariableType INSTANCE = new HistoricJPAEntityListVariableType();

        public override bool Cachable
        {
            get
            {
                return true;
            }
        }

        public static HistoricJPAEntityListVariableType SharedInstance
        {
            get
            {
                return INSTANCE;
            }
        }

    }

}