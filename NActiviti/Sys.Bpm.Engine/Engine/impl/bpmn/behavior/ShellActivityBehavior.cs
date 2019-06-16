using System;
using System.Collections.Generic;

namespace Sys.Workflow.Engine.Impl.Bpmn.Behavior
{

    using Sys.Workflow.Engine.Cfg.Securities;
    using Sys.Workflow.Engine.Delegate;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow.Engine.Impl.Util;

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

        private void ReadFields(IExecutionEntity execution)
        {
            commandStr = GetStringFromField(command, execution);
            arg1Str = GetStringFromField(arg1, execution);
            arg2Str = GetStringFromField(arg2, execution);
            arg3Str = GetStringFromField(arg3, execution);
            arg4Str = GetStringFromField(arg4, execution);
            arg5Str = GetStringFromField(arg5, execution);
            waitStr = GetStringFromField(wait, execution);
            resultVariableStr = GetStringFromField(outputVariable, execution);
            errorCodeVariableStr = GetStringFromField(errorCodeVariable, execution);

            string redirectErrorStr = GetStringFromField(redirectError, execution);
            string cleanEnvStr = GetStringFromField(cleanEnv, execution);

            waitFlag = waitStr is null || waitStr.Equals("true");
            redirectErrorFlag = "true".Equals(redirectErrorStr);
            cleanEnvBoolean = "true".Equals(cleanEnvStr);
            directoryStr = GetStringFromField(directory, execution);

        }

        public override void Execute(IExecutionEntity execution)
        {

            ReadFields(execution);

            IList<string> argList = new List<string>
            {
                commandStr
            };

            if (!(arg1Str is null))
            {
                argList.Add(arg1Str);
            }
            if (!(arg2Str is null))
            {
                argList.Add(arg2Str);
            }
            if (!(arg3Str is null))
            {
                argList.Add(arg3Str);
            }
            if (!(arg4Str is null))
            {
                argList.Add(arg4Str);
            }
            if (!(arg5Str is null))
            {
                argList.Add(arg5Str);
            }

            ShellExecutorContext executorContext = new ShellExecutorContext(waitFlag, cleanEnvBoolean, redirectErrorFlag, directoryStr, resultVariableStr, errorCodeVariableStr, argList);
            ICommandExecutorFactory shellCommandExecutorFactory = CommandExecutorContext.ShellCommandExecutorFactory;


            ICommandExecutor commandExecutor;
            if (shellCommandExecutorFactory != null)
            {
                // if there is a ShellExecutorFactoryProvided
                // then it will be used to create a desired shell command executor.
                commandExecutor = shellCommandExecutorFactory.CreateExecutor(executorContext);
            }
            else
            {
                // default Shell executor (if the shell security is OFF)
                commandExecutor = new ShellCommandExecutor(executorContext);
            }

            try
            {
                commandExecutor.ExecuteCommand(execution);
            }
            catch (Exception e)
            {
                throw new ActivitiException("Could not execute shell command ", e);
            }

            Leave(execution);
        }

        protected internal virtual string GetStringFromField(IExpression expression, IExecutionEntity execution)
        {
            if (expression != null)
            {
                object value = expression.GetValue(execution);
                if (value != null)
                {
                    return value.ToString();
                }
            }
            return null;
        }

    }

}