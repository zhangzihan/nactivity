using System;

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
namespace org.activiti.engine.impl.cmd
{
    using org.activiti.engine.impl.db;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;
    using System.Collections.Generic;

    /// 
    public class GetNextIdBlockCmd : ICommand<IdBlock>
    {

        private const long serialVersionUID = 1L;
        protected internal int idBlockSize;

        public GetNextIdBlockCmd(int idBlockSize)
        {
            this.idBlockSize = idBlockSize;
        }

        public  virtual IdBlock  Execute(ICommandContext  commandContext)
        {
            IPropertyEntity property = commandContext.PropertyEntityManager.FindById<IPropertyEntity>("next.dbid");
            long oldValue = long.Parse(property.Value);
            long newValue = oldValue + idBlockSize;
            property.Value = Convert.ToString(newValue);
            return new IdBlock(oldValue, newValue - 1);
        }
    }

}