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

namespace org.activiti.engine.impl.persistence.entity
{

    using org.activiti.engine.impl.cfg;
    using org.activiti.engine.impl.persistence.entity.data;


    /// 
    /// 
    public class ByteArrayEntityManagerImpl : AbstractEntityManager<IByteArrayEntity>, IByteArrayEntityManager
	{

	  protected internal IByteArrayDataManager byteArrayDataManager;

	  public ByteArrayEntityManagerImpl(ProcessEngineConfigurationImpl processEngineConfiguration, IByteArrayDataManager byteArrayDataManager) : base(processEngineConfiguration)
	  {
		this.byteArrayDataManager = byteArrayDataManager;
	  }

	  protected internal override IDataManager<IByteArrayEntity> DataManager
	  {
		  get
		  {
			return byteArrayDataManager;
		  }
	  }

	  public virtual IList<IByteArrayEntity> findAll()
	  {
		return byteArrayDataManager.findAll();
	  }

	  public virtual void deleteByteArrayById(string byteArrayEntityId)
	  {
		byteArrayDataManager.deleteByteArrayNoRevisionCheck(byteArrayEntityId);
	  }

	  public virtual IByteArrayDataManager ByteArrayDataManager
	  {
		  get
		  {
			return byteArrayDataManager;
		  }
		  set
		  {
			this.byteArrayDataManager = value;
		  }
	  }


	}

}