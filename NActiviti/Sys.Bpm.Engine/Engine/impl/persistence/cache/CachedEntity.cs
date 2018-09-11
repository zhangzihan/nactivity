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
namespace org.activiti.engine.impl.persistence.cache
{
    using org.activiti.engine.impl.persistence.entity;

    /// 
    public class CachedEntity
    {

        /// <summary>
        /// The actual <seealso cref="Entity"/> instance. 
        /// </summary>
        protected internal IEntity entity;

        /// <summary>
        /// Represents the 'persistence state' at the moment this <seealso cref="CachedEntity"/> instance was created.
        /// It is used later on to determine if a <seealso cref="Entity"/> has been updated, by comparing
        /// the 'persistent state' at that moment with this instance here.
        /// </summary>
        protected internal PersistentState originalPersistentState;

        public CachedEntity(IEntity entity, bool storeState)
        {
            this.entity = entity;
            if (storeState)
            {
                this.originalPersistentState = entity.PersistentState.Clone();
            }
        }

        public virtual IEntity Entity
        {
            get
            {
                return entity;
            }
            set
            {
                this.entity = value;
            }
        }


        public virtual PersistentState OriginalPersistentState
        {
            get
            {
                return originalPersistentState;
            }
            set
            {
                this.originalPersistentState = value;
            }
        }


        public virtual bool hasChanged()
        {
            return entity.PersistentState != null && !entity.PersistentState.Equals(originalPersistentState);
        }

    }
}