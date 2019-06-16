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
namespace Sys.Workflow.validation.validator
{
    /// 
    public interface IProblems
    {

    }

    public static class ProblemsConstants
    {
        public const string ALL_PROCESS_DEFINITIONS_NOT_EXECUTABLE = "activiti-process-definition-not-executable";
        public const string PROCESS_DEFINITION_NOT_EXECUTABLE = "activiti-specific-process-definition-not-executable";
        public const string ASSOCIATION_INVALID_SOURCE_REFERENCE = "activiti-asscociation-invalid-source-reference";
        public const string ASSOCIATION_INVALID_TARGET_REFERENCE = "activiti-asscociation-invalid-target-reference";
        public const string EXECUTION_LISTENER_IMPLEMENTATION_MISSING = "activiti-execution-listener-implementation-missing";
        public const string EXECUTION_LISTENER_INVALID_IMPLEMENTATION_TYPE = "activiti-execution-listener-invalid-implementation-type";
        public const string EVENT_LISTENER_IMPLEMENTATION_MISSING = "activiti-event-listener-implementation-missing";
        public const string EVENT_LISTENER_INVALID_IMPLEMENTATION = "activiti-event-listener-invalid-implementation";
        public const string EVENT_LISTENER_INVALID_THROW_EVENT_TYPE = "activiti-event-listener-invalid-throw-event-type";
        public const string START_EVENT_MULTIPLE_FOUND = "activiti-start-event-multiple-found";
        public const string START_EVENT_INVALID_EVENT_DEFINITION = "activiti-start-event-invalid-event-definition";
        public const string SEQ_FLOW_INVALID_SRC = "activiti-seq-flow-invalid-src";
        public const string SEQ_FLOW_INVALID_TARGET = "activiti-seq-flow-invalid-target";

        public const string USER_TASK_LISTENER_IMPLEMENTATION_MISSING = "activiti-usertask-listener-implementation-missing";
        public const string USER_TASK_NAME_TOO_LONG = "activiti-usertask-name-too-long";

        public const string SERVICE_TASK_INVALID_TYPE = "activiti-servicetask-invalid-type";
        public const string SERVICE_TASK_NAME_TOO_LONG = "activiti-servicetask-name-too-long";
        public const string SERVICE_TASK_RESULT_VAR_NAME_WITH_DELEGATE = "activiti-servicetask-result-var-name-with-delegate";
        public const string SERVICE_TASK_MISSING_IMPLEMENTATION = "activiti-servicetask-missing-implementation";
        public const string SERVICE_TASK_WEBSERVICE_INVALID_OPERATION_REF = "activiti-servicetask-webservice-invalid-operation-ref";
        public const string SERVICE_TASK_WEBSERVICE_INVALID_URL = "activiti-servicetask-webservice-invalid-url";
        public const string SEND_TASK_INVALID_IMPLEMENTATION = "activiti-sendtask-invalid-implementation";
        public const string SEND_TASK_INVALID_TYPE = "activiti-sendtask-invalid-type";
        public const string SEND_TASK_NAME_TOO_LONG = "activiti-sendtask-name-too-long";
        public const string SEND_TASK_WEBSERVICE_INVALID_OPERATION_REF = "activiti-send-webservice-invalid-operation-ref";
        public const string SCRIPT_TASK_MISSING_SCRIPT = "activiti-scripttask-missing-script";
        public const string MAIL_TASK_NO_RECIPIENT = "activiti-mailtask-no-recipient";
        public const string MAIL_TASK_NO_CONTENT = "activiti-mailtask-no-content";
        public const string SHELL_TASK_NO_COMMAND = "activiti-shelltask-no-command";
        public const string SHELL_TASK_INVALID_PARAM = "activiti-shelltask-invalid-param";
        public const string DMN_TASK_NO_KEY = "activiti-dmntask-no-decision-table-key";
        public const string EXCLUSIVE_GATEWAY_NO_OUTGOING_SEQ_FLOW = "activiti-exclusive-gateway-no-outgoing-seq-flow";
        public const string EXCLUSIVE_GATEWAY_CONDITION_NOT_ALLOWED_ON_SINGLE_SEQ_FLOW = "activiti-exclusive-gateway-condition-not-allowed-on-single-seq-flow";
        public const string EXCLUSIVE_GATEWAY_CONDITION_ON_DEFAULT_SEQ_FLOW = "activiti-exclusive-gateway-condition-on-seq-flow";
        public const string EXCLUSIVE_GATEWAY_SEQ_FLOW_WITHOUT_CONDITIONS = "activiti-exclusive-gateway-seq-flow-without-conditions";
        public const string EVENT_GATEWAY_ONLY_CONNECTED_TO_INTERMEDIATE_EVENTS = "activiti-event-gateway-only-connected-to-intermediate-events";
        public const string BPMN_MODEL_TARGET_NAMESPACE_TOO_LONG = "activiti-bpmn-model-target-namespace-too-long";
        public const string PROCESS_DEFINITION_ID_TOO_LONG = "activiti-process-definition-id-too-long";
        public const string PROCESS_DEFINITION_NAME_TOO_LONG = "activiti-process-definition-name-too-long";
        public const string PROCESS_DEFINITION_DOCUMENTATION_TOO_LONG = "activiti-process-definition-documentation-too-long";
        public const string FLOW_ELEMENT_ID_TOO_LONG = "activiti-flow-element-id-too-long";
        public const string SUBPROCESS_MULTIPLE_START_EVENTS = "activiti-subprocess-multiple-start-event";
        public const string SUBPROCESS_START_EVENT_EVENT_DEFINITION_NOT_ALLOWED = "activiti-subprocess-start-event-event-definition-not-allowed";
        public const string EVENT_SUBPROCESS_INVALID_START_EVENT_DEFINITION = "activiti-event-subprocess-invalid-start-event-definition";
        public const string BOUNDARY_EVENT_NO_EVENT_DEFINITION = "activiti-boundary-event-no-event-definition";
        public const string BOUNDARY_EVENT_INVALID_EVENT_DEFINITION = "activiti-boundary-event-invalid-event-definition";
        public const string BOUNDARY_EVENT_CANCEL_ONLY_ON_TRANSACTION = "activiti-boundary-event-cancel-only-on-transaction";
        public const string BOUNDARY_EVENT_MULTIPLE_CANCEL_ON_TRANSACTION = "activiti-boundary-event-multiple-cancel-on-transaction";
        public const string INTERMEDIATE_CATCH_EVENT_NO_EVENTDEFINITION = "activiti-intermediate-catch-event-no-eventdefinition";
        public const string INTERMEDIATE_CATCH_EVENT_INVALID_EVENTDEFINITION = "activiti-intermediate-catch-event-invalid-eventdefinition";
        public const string ERROR_MISSING_ERROR_CODE = "activiti-error-missing-error-code";
        public const string EVENT_MISSING_ERROR_CODE = "activiti-event-missing-error-code";
        public const string EVENT_TIMER_MISSING_CONFIGURATION = "activiti-event-timer-missing-configuration";
        public const string THROW_EVENT_INVALID_EVENTDEFINITION = "activiti-throw-event-invalid-eventdefinition";
        public const string MULTI_INSTANCE_MISSING_COLLECTION = "activiti-multi-instance-missing-collection";
        public const string MESSAGE_MISSING_NAME = "activiti-message-missing-name";
        public const string MESSAGE_INVALID_ITEM_REF = "activiti-message-invalid-item-ref";
        public const string MESSAGE_EVENT_MISSING_MESSAGE_REF = "activiti-message-event-missing-message-ref";
        public const string MESSAGE_EVENT_INVALID_MESSAGE_REF = "activiti-message-event-invalid-message-ref";
        public const string MESSAGE_EVENT_MULTIPLE_ON_BOUNDARY_SAME_MESSAGE_ID = "activiti-message-event-multiple-on-boundary-same-message-id";
        public const string OPERATION_INVALID_IN_MESSAGE_REFERENCE = "activiti-operation-invalid-in-message-reference";
        public const string SIGNAL_EVENT_MISSING_SIGNAL_REF = "activiti-signal-event-missing-signal-ref";
        public const string SIGNAL_EVENT_INVALID_SIGNAL_REF = "activiti-signal-event-invalid-signal-ref";
        public const string COMPENSATE_EVENT_INVALID_ACTIVITY_REF = "activiti-compensate-event-invalid-activity-ref";
        public const string COMPENSATE_EVENT_MULTIPLE_ON_BOUNDARY = "activiti-compensate-event-multiple-on-boundary";
        public const string SIGNAL_MISSING_ID = "activiti-signal-missing-id";
        public const string SIGNAL_MISSING_NAME = "activiti-signal-missing-name";
        public const string SIGNAL_DUPLICATE_NAME = "activiti-signal-duplicate-name";
        public const string SIGNAL_INVALID_SCOPE = "activiti-signal-invalid-scope";
        public const string DATA_ASSOCIATION_MISSING_TARGETREF = "activiti-data-association-missing-targetref";
        public const string DATA_OBJECT_MISSING_NAME = "activiti-data-object-missing-name";
        public const string END_EVENT_CANCEL_ONLY_INSIDE_TRANSACTION = "activiti-end-event-cancel-only-inside-transaction";
        public const string DI_INVALID_REFERENCE = "activiti-di-invalid-reference";
        public const string DI_DOES_NOT_REFERENCE_FLOWNODE = "activiti-di-does-not-reference-flownode";
        public const string DI_DOES_NOT_REFERENCE_SEQ_FLOW = "activiti-di-does-not-reference-seq-flow";
    }

}