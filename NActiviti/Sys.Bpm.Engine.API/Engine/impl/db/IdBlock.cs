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
namespace Sys.Workflow.Engine.Impl.DB
{
    /// 
    public class IdBlock
    {

        internal long nextId;
        internal long lastId;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nextId"></param>
        /// <param name="lastId"></param>
        public IdBlock(long nextId, long lastId)
        {
            this.nextId = nextId;
            this.lastId = lastId;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual long NextId
        {
            get
            {
                return nextId;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual long LastId
        {
            get
            {
                return lastId;
            }
        }
    }

}