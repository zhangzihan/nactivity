using System;

namespace Sys.Workflow.Engine.Impl.Cmd
{

    using Sys.Workflow.Engine.Impl.Interceptor;

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
            if (entityClass is null)
            {
                throw new ActivitiIllegalArgumentException("entityClass is null");
            }
            return commandContext.TableDataManager.GetTableName(entityClass, true);
        }

    }

}