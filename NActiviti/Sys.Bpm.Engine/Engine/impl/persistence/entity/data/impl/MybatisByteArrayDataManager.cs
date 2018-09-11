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
namespace org.activiti.engine.impl.persistence.entity.data.impl
{

    using org.activiti.engine.impl.cfg;
    using Sys.Bpm.Engine;

    /// 
    public class MybatisByteArrayDataManager : AbstractDataManager<IByteArrayEntity>, IByteArrayDataManager
    {

        public MybatisByteArrayDataManager(ProcessEngineConfigurationImpl processEngineConfiguration) : base(processEngineConfiguration)
        {
        }

        public override IByteArrayEntity create()
        {
            return new ByteArrayEntityImpl();
        }

        public override Type ManagedEntityClass
        {
            get
            {
                return typeof(ByteArrayEntityImpl);
            }
        }

        public virtual IList<IByteArrayEntity> findAll()
        {
            return DbSqlSession.selectList<ByteArrayEntityImpl, IByteArrayEntity>("selectByteArrays");
        }

        public virtual void deleteByteArrayNoRevisionCheck(string byteArrayEntityId)
        {
            DbSqlSession.delete("deleteByteArrayNoRevisionCheck", new KeyValuePair<string, object>("id", byteArrayEntityId), typeof(ByteArrayEntityImpl));
        }

    }

}