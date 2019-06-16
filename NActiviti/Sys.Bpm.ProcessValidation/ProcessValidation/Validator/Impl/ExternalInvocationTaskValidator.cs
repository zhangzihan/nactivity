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
namespace Sys.Workflow.validation.validator.impl
{

    using Sys.Workflow.bpmn.model;

    public abstract class ExternalInvocationTaskValidator : ProcessLevelValidator
    {

        protected internal virtual void ValidateFieldDeclarationsForEmail(Process process, TaskWithFieldExtensions task, IList<FieldExtension> fieldExtensions, IList<ValidationError> errors)
        {
            bool toDefined = false;
            bool textOrHtmlDefined = false;

            foreach (FieldExtension fieldExtension in fieldExtensions)
            {
                if (fieldExtension.FieldName.Equals("to"))
                {
                    toDefined = true;
                }
                if (fieldExtension.FieldName.Equals("html"))
                {
                    textOrHtmlDefined = true;
                }
                if (fieldExtension.FieldName.Equals("htmlVar"))
                {
                    textOrHtmlDefined = true;
                }
                if (fieldExtension.FieldName.Equals("text"))
                {
                    textOrHtmlDefined = true;
                }
                if (fieldExtension.FieldName.Equals("textVar"))
                {
                    textOrHtmlDefined = true;
                }
            }

            if (!toDefined)
            {
                AddError(errors, ProblemsConstants.MAIL_TASK_NO_RECIPIENT, process, task, ProcessValidatorResource.MAIL_TASK_NO_RECIPIENT);
            }
            if (!textOrHtmlDefined)
            {
                AddError(errors, ProblemsConstants.MAIL_TASK_NO_CONTENT, process, task, ProcessValidatorResource.MAIL_TASK_NO_CONTENT);
            }
        }

        protected internal virtual void ValidateFieldDeclarationsForShell(Process process, TaskWithFieldExtensions task, IList<FieldExtension> fieldExtensions, IList<ValidationError> errors)
        {
            bool shellCommandDefined = false;

            foreach (FieldExtension fieldExtension in fieldExtensions)
            {
                string fieldName = fieldExtension.FieldName;
                string fieldValue = fieldExtension.StringValue;

                if (fieldName.Equals("command"))
                {
                    shellCommandDefined = true;
                }

                if ((fieldName.Equals("wait") || fieldName.Equals("redirectError") || fieldName.Equals("cleanEnv")) && !fieldValue.ToLower().Equals("true") && !fieldValue.ToLower().Equals("false"))
                {
                    AddError(errors, ProblemsConstants.SHELL_TASK_INVALID_PARAM, process, task, ProcessValidatorResource.SHELL_TASK_INVALID_PARAM);
                }
            }

            if (!shellCommandDefined)
            {
                AddError(errors, ProblemsConstants.SHELL_TASK_NO_COMMAND, process, task, ProcessValidatorResource.SHELL_TASK_NO_COMMAND);
            }
        }

        protected internal virtual void ValidateFieldDeclarationsForDmn(Process process, TaskWithFieldExtensions task, IList<FieldExtension> fieldExtensions, IList<ValidationError> errors)
        {
            bool keyDefined = false;

            foreach (FieldExtension fieldExtension in fieldExtensions)
            {
                string fieldName = fieldExtension.FieldName;
                string fieldValue = fieldExtension.StringValue;

                if (fieldName.Equals("decisionTableReferenceKey") && !string.IsNullOrWhiteSpace(fieldValue))
                {
                    keyDefined = true;
                }
            }

            if (!keyDefined)
            {
                AddError(errors, ProblemsConstants.DMN_TASK_NO_KEY, process, task, ProcessValidatorResource.DMN_TASK_NO_KEY);
            }
        }
    }
}