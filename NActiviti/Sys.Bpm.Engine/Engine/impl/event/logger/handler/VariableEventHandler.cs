using System;
using System.Collections.Generic;

namespace org.activiti.engine.impl.@event.logger.handler
{
    using Microsoft.Extensions.Logging;
    using org.activiti.engine.@delegate.@event;
    using org.activiti.engine.impl.variable;
    using Sys;
    using Sys.Bpm;

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
            putInMapIfNotNull(data, Fields_Fields.NAME, variableEvent.VariableName);
            putInMapIfNotNull(data, Fields_Fields.PROCESS_DEFINITION_ID, variableEvent.ProcessDefinitionId);
            putInMapIfNotNull(data, Fields_Fields.PROCESS_INSTANCE_ID, variableEvent.ProcessInstanceId);
            putInMapIfNotNull(data, Fields_Fields.EXECUTION_ID, variableEvent.ExecutionId);
            putInMapIfNotNull(data, Fields_Fields.VALUE, variableEvent.VariableValue);

            IVariableType variableType = variableEvent.VariableType;
            if (variableType is BooleanType)
            {

                putInMapIfNotNull(data, Fields_Fields.VALUE_BOOLEAN, (bool?)variableEvent.VariableValue);
                putInMapIfNotNull(data, Fields_Fields.VALUE, variableEvent.VariableValue);
                putInMapIfNotNull(data, Fields_Fields.VARIABLE_TYPE, TYPE_BOOLEAN);

            }
            else if (variableType is StringType || variableType is LongStringType)
            {

                putInMapIfNotNull(data, Fields_Fields.VALUE_STRING, (string)variableEvent.VariableValue);
                putInMapIfNotNull(data, Fields_Fields.VARIABLE_TYPE, TYPE_STRING);

            }
            else if (variableType is ShortType)
            {

                short? value = (short?)variableEvent.VariableValue;
                putInMapIfNotNull(data, Fields_Fields.VALUE_SHORT, value);
                putInMapIfNotNull(data, Fields_Fields.VARIABLE_TYPE, TYPE_SHORT);

                if (value != null)
                {
                    putInMapIfNotNull(data, Fields_Fields.VALUE_INTEGER, value.Value);
                    putInMapIfNotNull(data, Fields_Fields.VALUE_LONG, value.Value);
                    putInMapIfNotNull(data, Fields_Fields.VALUE_DOUBLE, value.Value);
                }

            }
            else if (variableType is IntegerType)
            {

                int? value = (int?)variableEvent.VariableValue;
                putInMapIfNotNull(data, Fields_Fields.VALUE_INTEGER, value);
                putInMapIfNotNull(data, Fields_Fields.VARIABLE_TYPE, TYPE_INTEGER);

                if (value != null)
                {
                    putInMapIfNotNull(data, Fields_Fields.VALUE_LONG, value.Value);
                    putInMapIfNotNull(data, Fields_Fields.VALUE_DOUBLE, value.Value);
                }

            }
            else if (variableType is LongType)
            {

                long? value = (long?)variableEvent.VariableValue;
                putInMapIfNotNull(data, Fields_Fields.VALUE_LONG, value);
                putInMapIfNotNull(data, Fields_Fields.VARIABLE_TYPE, TYPE_LONG);

                if (value != null)
                {
                    putInMapIfNotNull(data, Fields_Fields.VALUE_DOUBLE, value.Value);
                }

            }
            else if (variableType is DoubleType)
            {

                double? value = (double?)variableEvent.VariableValue;
                putInMapIfNotNull(data, Fields_Fields.VALUE_DOUBLE, value);
                putInMapIfNotNull(data, Fields_Fields.VARIABLE_TYPE, TYPE_DOUBLE);

                if (value != null)
                {
                    putInMapIfNotNull(data, Fields_Fields.VALUE_INTEGER, value.Value);
                    putInMapIfNotNull(data, Fields_Fields.VALUE_LONG, value.Value);
                }

            }
            else if (variableType is DateType)
            {

                DateTime value = (DateTime)variableEvent.VariableValue;
                putInMapIfNotNull(data, Fields_Fields.VALUE_DATE, value != null ? (long?)value.Ticks : null);
                putInMapIfNotNull(data, Fields_Fields.VARIABLE_TYPE, TYPE_DATE);

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

                putInMapIfNotNull(data, Fields_Fields.VALUE_UUID, value);
                putInMapIfNotNull(data, Fields_Fields.VALUE_STRING, value);
                putInMapIfNotNull(data, Fields_Fields.VARIABLE_TYPE, TYPE_UUID);

            }
            else if (variableType is SerializableType || (variableEvent.VariableValue != null && (variableEvent.VariableValue is object)))
            {

                // Last try: serialize it to json
                ObjectMapper objectMapper = new ObjectMapper();
                try
                {
                    string value = objectMapper.writeValueAsString(variableEvent.VariableValue);
                    putInMapIfNotNull(data, Fields_Fields.VALUE_JSON, value);
                    putInMapIfNotNull(data, Fields_Fields.VARIABLE_TYPE, TYPE_JSON);
                    putInMapIfNotNull(data, Fields_Fields.VALUE, value);
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