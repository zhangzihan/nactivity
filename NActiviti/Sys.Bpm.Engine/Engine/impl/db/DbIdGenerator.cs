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

namespace Sys.Workflow.engine.impl.db
{
    using Sys.Workflow.engine.impl.cfg;
    using Sys.Workflow.engine.impl.cmd;
    using Sys.Workflow.engine.impl.interceptor;
    using Sys.Workflow.engine.impl.persistence.entity;

    /// 
    public class DbIdGenerator : IIdGenerator
    {
        /// <summary>
        /// 
        /// </summary>
        protected internal int idBlockSize;
        /// <summary>
        /// 
        /// </summary>
        protected internal long nextId;
        /// <summary>
        /// 
        /// </summary>
        protected internal long lastId = -1;

        /// <summary>
        /// 
        /// </summary>
        protected internal ICommandExecutor commandExecutor;
        /// <summary>
        /// 
        /// </summary>
        protected internal CommandConfig commandConfig;

        private readonly object syncRoot = new object();

        /// <summary>
        /// 
        /// </summary>
        public virtual string GetNextId()
        {
            lock (syncRoot)
            {
                if (lastId < nextId)
                {
                    GetNewBlock();
                }
                long _nextId = nextId++;
                return Convert.ToString(_nextId);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected internal virtual void GetNewBlock()
        {
            lock (syncRoot)
            {
                IdBlock idBlock = commandExecutor.Execute(commandConfig, new GetNextIdBlockCmd(idBlockSize));
                this.nextId = idBlock.NextId;
                this.lastId = idBlock.LastId;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int IdBlockSize
        {
            get
            {
                return idBlockSize;
            }
            set
            {
                this.idBlockSize = value;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public virtual ICommandExecutor CommandExecutor
        {
            get
            {
                return commandExecutor;
            }
            set
            {
                this.commandExecutor = value;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public virtual CommandConfig CommandConfig
        {
            get
            {
                return commandConfig;
            }
            set
            {
                this.commandConfig = value;
            }
        }
    }
}