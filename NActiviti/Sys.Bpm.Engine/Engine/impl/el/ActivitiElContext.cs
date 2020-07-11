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

namespace Sys.Workflow.Engine.Impl.EL
{
    /// <summary>
    /// 
    /// </summary>
    public class ActivitiElContext : ELContext
    {
        /// <summary>
        /// 
        /// </summary>
        protected internal ELResolver elResolver;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="elResolver"></param>
        public ActivitiElContext(ELResolver elResolver)
        {
            this.elResolver = elResolver;
        }
        /// <summary>
        /// 
        /// </summary>
        public override ELResolver ELResolver
        {
            get
            {
                return elResolver;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public override FunctionMapper FunctionMapper
        {
            get
            {
                return null;// new ActivitiFunctionMapper();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public override VariableMapper VariableMapper
        {
            get
            {
                return null;
            }
        }
    }
}