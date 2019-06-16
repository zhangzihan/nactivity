using System;

namespace Sys.Workflow.engine.impl.cmd
{
    using Sys.Workflow.engine.impl.interceptor;

    /// 
    public class ExecuteCustomSqlCmd<Mapper, ResultType> : ICommand<ResultType>
    {

        protected internal Type mapperClass;
        protected internal ICustomSqlExecution<Mapper, ResultType> customSqlExecution;

        public ExecuteCustomSqlCmd(Type mapperClass, ICustomSqlExecution<Mapper, ResultType> customSqlExecution)
        {
            this.mapperClass = mapperClass;
            this.customSqlExecution = customSqlExecution;
        }

        public virtual ResultType Execute(ICommandContext commandContext)
        {
            Mapper mapper = (Mapper)commandContext.DbSqlSession.GetMapper(mapperClass);

            return customSqlExecution.execute(mapper);
        }

    }

}