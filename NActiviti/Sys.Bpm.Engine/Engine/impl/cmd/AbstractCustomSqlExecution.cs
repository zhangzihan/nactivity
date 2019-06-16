using System;

namespace Sys.Workflow.Engine.Impl.Cmd
{
    /// 
    public abstract class AbstractCustomSqlExecution<Mapper, ResultType> : ICustomSqlExecution<Mapper, ResultType>
    {
        public abstract ResultType execute(Mapper mapper);

        protected internal Type mapperClass;

        public AbstractCustomSqlExecution(Type mapperClass)
        {
            this.mapperClass = mapperClass;
        }

        public virtual Type MapperClass
        {
            get
            {
                return mapperClass;
            }
        }

    }

}