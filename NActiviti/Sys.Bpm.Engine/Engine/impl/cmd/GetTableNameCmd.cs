using System;

namespace org.activiti.engine.impl.cmd
{

    using org.activiti.engine.impl.interceptor;

    [Serializable]
    public class GetTableNameCmd : ICommand<string>
    {

        private const long serialVersionUID = 1L;

        private Type entityClass;

        public GetTableNameCmd(Type entityClass)
        {
            this.entityClass = entityClass;
        }

        public  virtual string  Execute(ICommandContext  commandContext)
        {
            if (entityClass == null)
            {
                throw new ActivitiIllegalArgumentException("entityClass is null");
            }
            return commandContext.TableDataManager.GetTableName(entityClass, true);
        }

    }

}