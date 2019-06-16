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
namespace Sys.Workflow.Engine.Impl.Persistence.Entity
{
    /// <summary>
    /// A dummy implementation of <seealso cref="IVariableInstance"/>, used for storing transient variables
    /// on a <seealso cref="IVariableScope"/>, as the <seealso cref="IVariableScope"/> works with instances of <seealso cref="IVariableInstance"/>
    /// and not with raw key/values.
    /// 
    /// Nothing more than a thin wrapper around a name and value. All the other methods are not implemented.
    /// 
    /// 
    /// </summary>
    public class TransientVariableInstance : IVariableInstance
    {

        public static string TYPE_TRANSIENT = "transient";

        protected internal string variableName;
        protected internal object variableValue;

        public TransientVariableInstance(string variableName, object variableValue)
        {
            this.variableName = variableName;
            this.variableValue = variableValue;
        }

        public virtual string Name
        {
            get
            {
                return variableName;
            }
            set
            {

            }
        }

        public virtual string TextValue
        {
            get
            {
                return null;
            }
            set
            {

            }
        }


        public virtual string TextValue2
        {
            get
            {
                return null;
            }
            set
            {

            }
        }


        public virtual long? LongValue
        {
            get
            {
                return null;
            }
            set
            {

            }
        }


        public virtual double? DoubleValue
        {
            get
            {
                return null;
            }
            set
            {

            }
        }


        public virtual byte[] Bytes
        {
            get
            {
                return null;
            }
            set
            {

            }
        }


        public virtual object CachedValue
        {
            get
            {
                return null;
            }
            set
            {

            }
        }


        public virtual string Id
        {
            get
            {
                return null;
            }
            set
            {

            }
        }


        public virtual bool Inserted
        {
            get
            {
                return false;
            }
            set
            {

            }
        }


        public virtual bool Updated
        {
            get
            {
                return false;
            }
            set
            {

            }
        }


        public virtual bool Deleted
        {
            get
            {
                return false;
            }
            set
            {

            }
        }


        public virtual PersistentState PersistentState
        {
            get
            {
                return null;
            }
        }

        public virtual int Revision
        {
            set
            {

            }
            get
            {
                return 0;
            }
        }


        public virtual int RevisionNext
        {
            get
            {
                return 0;
            }
        }


        public virtual string ProcessInstanceId
        {
            set
            {

            }
            get
            {
                return null;
            }
        }

        public virtual string ExecutionId
        {
            set
            {

            }
            get
            {
                return null;
            }
        }

        public virtual object Value
        {
            get
            {
                return variableValue;
            }
            set
            {
                variableValue = value;
            }
        }


        public virtual string TypeName
        {
            get
            {
                return TYPE_TRANSIENT;
            }
            set
            {

            }
        }



        public virtual string TaskId
        {
            get
            {
                return null;
            }
            set
            {

            }
        }



    }

}