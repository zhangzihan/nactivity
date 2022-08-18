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
namespace Sys.Workflow.Engine.Impl.Util
{
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using System.Diagnostics;
    using System.IO;

    /// 
    public class ShellCommandExecutor : ICommandExecutor
    {
        private bool? waitFlag;
        private readonly bool? cleanEnvBoolean;
        private readonly bool? redirectErrorFlag;
        private readonly string directoryStr;
        private readonly string resultVariableStr;
        private readonly string errorCodeVariableStr;
        private readonly IList<string> argList;

        public ShellCommandExecutor(bool? waitFlag, bool? cleanEnvBoolean, bool? redirectErrorFlag, string directoryStr, string resultVariableStr, string errorCodeVariableStr, IList<string> argList)
        {
            this.waitFlag = waitFlag;
            this.cleanEnvBoolean = cleanEnvBoolean;
            this.redirectErrorFlag = redirectErrorFlag;
            this.directoryStr = directoryStr;
            this.resultVariableStr = resultVariableStr;
            this.errorCodeVariableStr = errorCodeVariableStr;
            this.argList = argList;
        }

        public ShellCommandExecutor(ShellExecutorContext context) : this(context.WaitFlag, context.CleanEnvBoolan, context.RedirectErrorFlag, context.DirectoryStr, context.ResultVariableStr, context.ErrorCodeVariableStr, context.ArgList)
        {
        }

        public virtual void ExecuteCommand(IExecutionEntity execution)
        {
            if (argList is object && argList.Count > 0)
            {
                //ProcessBuilder processBuilder = new ProcessBuilder(argList);
                //processBuilder.redirectErrorStream(RedirectErrorFlag);
                //if (CleanEnvBoolean.Value)
                //{
                //    IDictionary<string, string> env = processBuilder.environment();
                //    env.Clear();
                //}
                //if (!string.ReferenceEquals(DirectoryStr, null) && DirectoryStr.Length > 0)
                //{
                //    processBuilder.directory(new File(DirectoryStr));
                //}
                var psi = new ProcessStartInfo();
                Process process = new Process()
                {
                    StartInfo = psi
                };//processBuilder.start();

                if (WaitFlag.Value)
                {
                    int errorCode = 0;

                    process.WaitForExit();

                    if (ResultVariableStr is not null)
                    {
                        string result = process.StandardOutput.ReadToEnd(); //convertStreamToStr(process.StandardOutput);
                        execution.SetVariable(ResultVariableStr, result);
                    }

                    if (ErrorCodeVariableStr is not null)
                    {
                        execution.SetVariable(ErrorCodeVariableStr, Convert.ToString(errorCode));

                    }

                }
            }
        }

        private string ConvertStreamToStr(System.IO.Stream @is)
        {

            if (@is is not null)
            {
                return new StreamReader(@is).ReadToEnd();
                //StringWriter writer = new StringWriter();

                //char[] buffer = new char[1024];
                //try
                //{
                //    StringReader reader = new StringReader(@is);
                //    int n;
                //    while ((n = reader.read(buffer)) != -1)
                //    {
                //        writer.write(buffer, 0, n);
                //    }
                //}
                //finally
                //{
                //    @is.Close();
                //}
                //return writer.ToString();
            }
            else
            {
                return "";
            }
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


        public virtual bool? CleanEnvBoolean
        {
            get
            {
                return cleanEnvBoolean;
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
        }

    }

}