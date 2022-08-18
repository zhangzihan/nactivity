using System;
using System.Collections.Generic;

namespace Sys.Workflow.Engine.Impl.Events.Logger.Handlers
{
    using Microsoft.Extensions.Logging;
    using Sys.Workflow.Engine.Delegate.Events;
    using Sys.Workflow.Engine.Impl.Variable;
    using Sys.Workflow;

    /// 
    public abstract class VariableEventHandler : AbstractDatabaseEventLoggerEventHandler
    {
        public const string TYPE_BOOLEAN = "boolean";
        public const string TYPE_STRING = "string";
        public const string TYPE_SHORT = "short";
        public const string TYPE_INTEGER = "integer";
        public const string TYPE_DOUBLE = "double";
        public const string TYPE_LONG = "long";
        public const string TYPE_DATE = "date";
        public const string TYPE_UUID = "uuid";
        public const string TYPE_JSON = "json";

        private static readonly ILogger log = ProcessEngineServiceProvider.LoggerService<VariableEventHandler>();

        protected internal virtual IDictionary<string, object> createData(IActivitiVariableEvent variableEvent)
        {
            IDictionary<string, object> data = new Dictionary<string, object>();
            PutInMapIfNotNull(data, FieldsFields.NAME, variableEvent.VariableName);
            PutInMapIfNotNull(data, FieldsFields.PROCESS_DEFINITION_ID, variableEvent.ProcessDefinitionId);
            PutInMapIfNotNull(data, FieldsFields.PROCESS_INSTANCE_ID, variableEvent.ProcessInstanceId);
            PutInMapIfNotNull(data, FieldsFields.EXECUTION_ID, variableEvent.ExecutionId);
            PutInMapIfNotNull(data, FieldsFields.VALUE, variableEvent.VariableValue);

            IVariableType variableType = variableEvent.VariableType;
            if (variableType is BooleanType)
            {
                PutInMapIfNotNull(data, FieldsFields.VALUE_BOOLEAN, (bool?)variableEvent.VariableValue);
                PutInMapIfNotNull(data, FieldsFields.VALUE, variableEvent.VariableValue);
                PutInMapIfNotNull(data, FieldsFields.VARIABLE_TYPE, TYPE_BOOLEAN);

            }
            else if (variableType is StringType || variableType is LongStringType)
            {

                PutInMapIfNotNull(data, FieldsFields.VALUE_STRING, (string)variableEvent.VariableValue);
                PutInMapIfNotNull(data, FieldsFields.VARIABLE_TYPE, TYPE_STRING);

            }
            else if (variableType is ShortType)
            {

                short? value = (short?)variableEvent.VariableValue;
                PutInMapIfNotNull(data, FieldsFields.VALUE_SHORT, value);
                PutInMapIfNotNull(data, FieldsFields.VARIABLE_TYPE, TYPE_SHORT);

                if (value is not null)
                {
                    PutInMapIfNotNull(data, FieldsFields.VALUE_INTEGER, value.Value);
                    PutInMapIfNotNull(data, FieldsFields.VALUE_LONG, value.Value);
                    PutInMapIfNotNull(data, FieldsFields.VALUE_DOUBLE, value.Value);
                }

            }
            else if (variableType is IntegerType)
            {

                int? value = (int?)variableEvent.VariableValue;
                PutInMapIfNotNull(data, FieldsFields.VALUE_INTEGER, value);
                PutInMapIfNotNull(data, FieldsFields.VARIABLE_TYPE, TYPE_INTEGER);

                if (value is not null)
                {
                    PutInMapIfNotNull(data, FieldsFields.VALUE_LONG, value.Value);
                    PutInMapIfNotNull(data, FieldsFields.VALUE_DOUBLE, value.Value);
                }

            }
            else if (variableType is LongType)
            {

                long? value = (long?)variableEvent.VariableValue;
                PutInMapIfNotNull(data, FieldsFields.VALUE_LONG, value);
                PutInMapIfNotNull(data, FieldsFields.VARIABLE_TYPE, TYPE_LONG);

                if (value is not null)
                {
                    PutInMapIfNotNull(data, FieldsFields.VALUE_DOUBLE, value.Value);
                }

            }
            else if (variableType is DoubleType)
            {
                double? value = (double?)variableEvent.VariableValue;
                PutInMapIfNotNull(data, FieldsFields.VALUE_DOUBLE, value);
                PutInMapIfNotNull(data, FieldsFields.VARIABLE_TYPE, TYPE_DOUBLE);

                if (value is not null)
                {
                    PutInMapIfNotNull(data, FieldsFields.VALUE_INTEGER, value.Value);
                    PutInMapIfNotNull(data, FieldsFields.VALUE_LONG, value.Value);
                }

            }
            else if (variableType is DateType)
            {
                if (variableEvent.VariableValue is DateTime value)
                {
                    PutInMapIfNotNull(data, FieldsFields.VALUE_DATE, value.Ticks);
                }
                PutInMapIfNotNull(data, FieldsFields.VARIABLE_TYPE, TYPE_DATE);
            }
            else if (variableType is UUIDType)
            {

                string value = null;
                if (variableEvent.VariableValue is System.Guid)
                {
                    value = ((System.Guid)variableEvent.VariableValue).ToString();
                }
                else
                {
                    value = (string)variableEvent.VariableValue;
                }

                PutInMapIfNotNull(data, FieldsFields.VALUE_UUID, value);
                PutInMapIfNotNull(data, FieldsFields.VALUE_STRING, value);
                PutInMapIfNotNull(data, FieldsFields.VARIABLE_TYPE, TYPE_UUID);

            }
            else if (variableType is SerializableType || (variableEvent.VariableValue is not null && (variableEvent.VariableValue is not null)))
            {

                // Last try: serialize it to json
                ObjectMapper objectMapper = new ObjectMapper();
                try
                {
                    string value = objectMapper.WriteValueAsString(variableEvent.VariableValue);
                    PutInMapIfNotNull(data, FieldsFields.VALUE_JSON, value);
                    PutInMapIfNotNull(data, FieldsFields.VARIABLE_TYPE, TYPE_JSON);
                    PutInMapIfNotNull(data, FieldsFields.VALUE, value);
                }
                catch (Exception)
                {
                    // Nothing to do about it
                    log.LogDebug("Could not serialize variable value " + variableEvent.VariableValue);
                }
            }

            return data;
        }
    }
}