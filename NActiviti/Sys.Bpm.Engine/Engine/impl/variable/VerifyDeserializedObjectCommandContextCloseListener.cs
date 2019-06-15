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
    using org.activiti.engine.impl.interceptor;

    /// <summary>
    /// A <seealso cref="ICommandContextCloseListener"/> that holds one <seealso cref="DeserializedObject"/> instance that
    /// is added by the <seealso cref="SerializableType"/>.
    /// 
    /// On the <seealso cref="#closing(CommandContext)"/> of the <seealso cref="CommandContext"/>, the <seealso cref="DeserializedObject"/>
    /// will be verified if it is dirty. If so, it will update the right entities such that changes will be flushed.
    /// 
    /// It's important that this happens in the <seealso cref="#closing(CommandContext)"/>, as this happens before
    /// the <seealso cref="CommandContext#close()"/> is called and when all the sessions are flushed 
    /// (including the <seealso cref="DbSqlSession"/> in the relational DB case (the data needs to be ready then). 
    /// 
    /// 
    /// </summary>
    public class VerifyDeserializedObjectCommandContextCloseListener : ICommandContextCloseListener
    {

        protected internal DeserializedObject deserializedObject;

        public VerifyDeserializedObjectCommandContextCloseListener(DeserializedObject deserializedObject)
        {
            this.deserializedObject = deserializedObject;
        }

        public virtual void Closing(ICommandContext commandContext)
        {
            deserializedObject.VerifyIfBytesOfSerializedObjectChanged();
        }

        public virtual void Closed(ICommandContext commandContext)
        {

        }

        public virtual void AfterSessionsFlush(ICommandContext commandContext)
        {

        }

        public virtual void CloseFailure(ICommandContext commandContext)
        {

        }
    }
}