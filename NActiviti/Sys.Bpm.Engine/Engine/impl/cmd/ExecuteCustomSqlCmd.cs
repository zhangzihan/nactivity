using System;

namespace org.activiti.engine.impl.cmd
{
    using org.activiti.engine.impl.interceptor;

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

        public virtual ResultType execute(ICommandContext commandContext)
        {
            Mapper mapper = (Mapper)commandContext.DbSqlSession.getMapper(mapperClass);

            return customSqlExecution.execute(mapper);
        }

    }

}