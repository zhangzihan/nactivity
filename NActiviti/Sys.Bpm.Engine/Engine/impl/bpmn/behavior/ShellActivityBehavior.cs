using System;
using System.Collections.Generic;

namespace org.activiti.engine.impl.bpmn.behavior
{

    using org.activiti.engine.cfg.security;
    using org.activiti.engine.@delegate;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.impl.util;

    [Serializable]
    public class ShellActivityBehavior : AbstractBpmnActivityBehavior
    {

        private const long serialVersionUID = 1L;

        protected internal IExpression command;
        protected internal IExpression wait;
        protected internal IExpression arg1;
        protected internal IExpression arg2;
        protected internal IExpression arg3;
        protected internal IExpression arg4;
        protected internal IExpression arg5;
        protected internal IExpression outputVariable;
        protected internal IExpression errorCodeVariable;
        protected internal IExpression redirectError;
        protected internal IExpression cleanEnv;
        protected internal IExpression directory;

        internal string commandStr;
        internal string arg1Str;
        internal string arg2Str;
        internal string arg3Str;
        internal string arg4Str;
        internal string arg5Str;
        internal string waitStr;
        internal string resultVariableStr;
        internal string errorCodeVariableStr;
        internal bool? waitFlag;
        internal bool? redirectErrorFlag;
        internal bool? cleanEnvBoolean;
        internal string directoryStr;

        private void readFields(IExecutionEntity execution)
        {
            commandStr = getStringFromField(command, execution);
            arg1Str = getStringFromField(arg1, execution);
            arg2Str = getStringFromField(arg2, execution);
            arg3Str = getStringFromField(arg3, execution);
            arg4Str = getStringFromField(arg4, execution);
            arg5Str = getStringFromField(arg5, execution);
            waitStr = getStringFromField(wait, execution);
            resultVariableStr = getStringFromField(outputVariable, execution);
            errorCodeVariableStr = getStringFromField(errorCodeVariable, execution);

            string redirectErrorStr = getStringFromField(redirectError, execution);
            string cleanEnvStr = getStringFromField(cleanEnv, execution);

            waitFlag = string.ReferenceEquals(waitStr, null) || waitStr.Equals("true");
            redirectErrorFlag = "true".Equals(redirectErrorStr);
            cleanEnvBoolean = "true".Equals(cleanEnvStr);
            directoryStr = getStringFromField(directory, execution);

        }

        public override void execute(IExecutionEntity execution)
        {

            readFields(execution);

            IList<string> argList = new List<string>();
            argList.Add(commandStr);

            if (!string.ReferenceEquals(arg1Str, null))
            {
                argList.Add(arg1Str);
            }
            if (!string.ReferenceEquals(arg2Str, null))
            {
                argList.Add(arg2Str);
            }
            if (!string.ReferenceEquals(arg3Str, null))
            {
                argList.Add(arg3Str);
            }
            if (!string.ReferenceEquals(arg4Str, null))
            {
                argList.Add(arg4Str);
            }
            if (!string.ReferenceEquals(arg5Str, null))
            {
                argList.Add(arg5Str);
            }

            ShellExecutorContext executorContext = new ShellExecutorContext(waitFlag, cleanEnvBoolean, redirectErrorFlag, directoryStr, resultVariableStr, errorCodeVariableStr, argList);

            ICommandExecutor commandExecutor = null;

            ICommandExecutorFactory shellCommandExecutorFactory = CommandExecutorContext.ShellCommandExecutorFactory;

            if (shellCommandExecutorFactory != null)
            {
                // if there is a ShellExecutorFactoryProvided
                // then it will be used to create a desired shell command executor.
                commandExecutor = shellCommandExecutorFactory.createExecutor(executorContext);
            }
            else
            {
                // default Shell executor (if the shell security is OFF)
                commandExecutor = new ShellCommandExecutor(executorContext);
            }

            try
            {
                commandExecutor.executeCommand(execution);
            }
            catch (Exception e)
            {
                throw new ActivitiException("Could not execute shell command ", e);
            }

            leave(execution);
        }

        protected internal virtual string getStringFromField(IExpression expression, IExecutionEntity execution)
        {
            if (expression != null)
            {
                object value = expression.getValue(execution);
                if (value != null)
                {
                    return value.ToString();
                }
            }
            return null;
        }

    }

}