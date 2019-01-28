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
namespace org.activiti.validation.validator.impl
{

    using org.activiti.bpmn.model;

    public abstract class ExternalInvocationTaskValidator : ProcessLevelValidator
    {

        protected internal virtual void validateFieldDeclarationsForEmail(org.activiti.bpmn.model.Process process, TaskWithFieldExtensions task, IList<FieldExtension> fieldExtensions, IList<ValidationError> errors)
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
                addError(errors, Problems_Fields.MAIL_TASK_NO_RECIPIENT, process, task, "No recipient is defined on the mail activity");
            }
            if (!textOrHtmlDefined)
            {
                addError(errors, Problems_Fields.MAIL_TASK_NO_CONTENT, process, task, "Text, html, textVar or htmlVar field should be provided");
            }
        }

        protected internal virtual void validateFieldDeclarationsForShell(org.activiti.bpmn.model.Process process, TaskWithFieldExtensions task, IList<FieldExtension> fieldExtensions, IList<ValidationError> errors)
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
                    addError(errors, Problems_Fields.SHELL_TASK_INVALID_PARAM, process, task, "Undefined parameter value for shell field");
                }

            }

            if (!shellCommandDefined)
            {
                addError(errors, Problems_Fields.SHELL_TASK_NO_COMMAND, process, task, "No shell command is defined on the shell activity");
            }
        }

        protected internal virtual void validateFieldDeclarationsForDmn(org.activiti.bpmn.model.Process process, TaskWithFieldExtensions task, IList<FieldExtension> fieldExtensions, IList<ValidationError> errors)
        {
            bool keyDefined = false;

            foreach (FieldExtension fieldExtension in fieldExtensions)
            {
                string fieldName = fieldExtension.FieldName;
                string fieldValue = fieldExtension.StringValue;

                if (fieldName.Equals("decisionTableReferenceKey") && !ReferenceEquals(fieldValue, null) && fieldValue.Length > 0)
                {
                    keyDefined = true;
                }
            }

            if (!keyDefined)
            {
                addError(errors, Problems_Fields.DMN_TASK_NO_KEY, process, task, "No decision table reference key is defined on the dmn activity");
            }
        }

    }

}