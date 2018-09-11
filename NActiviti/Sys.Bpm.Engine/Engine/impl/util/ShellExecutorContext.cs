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
namespace org.activiti.engine.impl.util
{

    using org.activiti.engine.cfg.security;

    /// 
    public class ShellExecutorContext : IExecutorContext
    {
        private bool? waitFlag;
        private readonly bool? cleanEnvBoolan;
        private readonly bool? redirectErrorFlag;
        private readonly string directoryStr;
        private readonly string resultVariableStr;
        private readonly string errorCodeVariableStr;
        private IList<string> argList;

        public ShellExecutorContext(bool? waitFlag, bool? cleanEnvBoolean, bool? redirectErrorFlag, string directoryStr, string resultVariableStr, string errorCodeVariableStr, IList<string> argList)
        {
            this.waitFlag = waitFlag;
            this.cleanEnvBoolan = cleanEnvBoolean;
            this.redirectErrorFlag = redirectErrorFlag;
            this.directoryStr = directoryStr;
            this.resultVariableStr = resultVariableStr;
            this.errorCodeVariableStr = errorCodeVariableStr;
            this.argList = argList;
        }

        public virtual bool? WaitFlag
        {
            get
            {
                return waitFlag;
            }
            set
            {
                this.waitFlag = value;
            }
        }


        public virtual bool? CleanEnvBoolan
        {
            get
            {
                return cleanEnvBoolan;
            }
        }

        public virtual bool? RedirectErrorFlag
        {
            get
            {
                return redirectErrorFlag;
            }
        }

        public virtual string DirectoryStr
        {
            get
            {
                return directoryStr;
            }
        }

        public virtual string ResultVariableStr
        {
            get
            {
                return resultVariableStr;
            }
        }

        public virtual string ErrorCodeVariableStr
        {
            get
            {
                return errorCodeVariableStr;
            }
        }

        public virtual IList<string> ArgList
        {
            get
            {
                return argList;
            }
            set
            {
                this.argList = value;
            }
        }

    }

}