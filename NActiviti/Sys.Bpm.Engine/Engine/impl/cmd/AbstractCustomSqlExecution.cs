using System;

namespace org.activiti.engine.impl.cmd
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