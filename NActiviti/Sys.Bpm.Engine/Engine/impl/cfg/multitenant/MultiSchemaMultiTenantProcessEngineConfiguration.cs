/* Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *      http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
namespace Sys.Workflow.Engine.Impl.Cfg.Multitenants
{
    using Sys.Workflow.Engine.Impl.Asyncexecutor.Multitenants;
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence;
    using Sys.Data;

    /// <summary>
    /// A <seealso cref="ProcessEngineConfiguration"/> that builds a multi tenant <seealso cref="IProcessEngine"/> where 
    /// each tenant has its own database schema.
    /// 
    /// If multitenancy is needed and no data isolation is needed: the default <seealso cref="ProcessEngineConfigurationImpl"/> 
    /// of Activiti is multitenant enabled out of the box by setting a tenant identifier on a <seealso cref="IDeploymentBuilder"/>. 
    /// 
    /// This configuration has following characteristics:
    /// 
    /// - It needs a <seealso cref="ITenantInfoHolder"/> to determine which tenant is currently 'active'. Ie for which 
    ///   tenant a certain API call is executed.
    /// 
    /// - The <seealso cref="StrongUuidGenerator"/> is used by default. The 'regular' <seealso cref="DbIdGenerator"/> cannot be used with this config.
    /// 
    /// - Adding tenants (also after boot!) is done using the <seealso cref="#registerTenant(String, DataSource)"/> operations.
    /// 
    /// - Currently, this config does not work with the 'old' <seealso cref="JobExecutor"/>, but only with the newer <seealso cref="IAsyncExecutor"/>.
    ///   There are two different implementations: 
    ///     - The <seealso cref="ExecutorPerTenantAsyncExecutor"/>: creates one full <seealso cref="IAsyncExecutor"/> for each tenant.
    ///     - The <seealso cref="SharedExecutorServiceAsyncExecutor"/>: created acquisition threads for each tenant, but the 
    ///       job execution is done using a process engine shared <seealso cref="ExecutorService"/>.
    ///   The <seealso cref="IAsyncExecutor"/> needs to be injected using the <seealso cref="#setAsyncExecutor(AsyncExecutor)"/> method on this class.    
    /// 
    /// databasetype
    /// 
    /// 
    /// </summary>
    public class MultiSchemaMultiTenantProcessEngineConfiguration : ProcessEngineConfigurationImpl
    {
        protected internal ITenantInfoHolder tenantInfoHolder;
        protected internal bool booted;

        public MultiSchemaMultiTenantProcessEngineConfiguration(ITenantInfoHolder tenantInfoHolder)
        {

            this.tenantInfoHolder = tenantInfoHolder;

            // Using the UUID generator, as otherwise the ids are pulled from a global pool of ids, backed by
            // a database table. Which is impossible with a mult-database-schema setup.

            // Also: it avoids the need for having a process definition cache for each tenant

            this.idGenerator = new StrongUuidGenerator();

            this.dataSource = new TenantAwareDataSource(tenantInfoHolder);
        }

        /// <summary>
        /// Add a new <seealso cref="DataSource"/> for a tenant, identified by the provided tenantId, to the engine.
        /// This can be done after the engine has booted up.
        /// 
        /// Note that the tenant identifier must have been added to the <seealso cref="ITenantInfoHolder"/> *prior*
        /// to calling this method.
        /// </summary>
        public virtual void registerTenant(string tenantId, IDataSource dataSource)
        {
            ((TenantAwareDataSource)base.DataSource).addDataSource(tenantId, dataSource);

            if (booted)
            {
                createTenantSchema(tenantId);

                createTenantAsyncJobExecutor(tenantId);

                tenantInfoHolder.CurrentTenantId = tenantId;
                base.postProcessEngineInitialisation();
                tenantInfoHolder.clearCurrentTenantId();
            }
        }

        public override void initAsyncExecutor()
        {

            if (asyncExecutor == null)
            {
                asyncExecutor = new ExecutorPerTenantAsyncExecutor(tenantInfoHolder);
            }

            base.initAsyncExecutor();

            if (asyncExecutor is ITenantAwareAsyncExecutor)
            {
                foreach (string tenantId in tenantInfoHolder.AllTenants)
                {
                    ((ITenantAwareAsyncExecutor)asyncExecutor).addTenantAsyncExecutor(tenantId, false); // false -> will be started later with all the other executors
                }
            }
        }

        public override IProcessEngine buildProcessEngine()
        {

            // Disable schema creation/validation by setting it to null.
            // We'll do it manually, see buildProcessEngine() method (hence why it's copied first)
            string originalDatabaseSchemaUpdate = this.databaseSchemaUpdate;
            this.databaseSchemaUpdate = null;

            // Also, we shouldn't start the async executor until *after* the schema's have been created
            bool originalIsAutoActivateAsyncExecutor = this.asyncExecutorActivate;
            this.asyncExecutorActivate = false;

            IProcessEngine processEngine = base.buildProcessEngine();

            // Reset to original values
            this.databaseSchemaUpdate = originalDatabaseSchemaUpdate;
            this.asyncExecutorActivate = originalIsAutoActivateAsyncExecutor;

            // Create tenant schema
            foreach (string tenantId in tenantInfoHolder.AllTenants)
            {
                createTenantSchema(tenantId);
            }

            // Start async executor
            if (asyncExecutor != null && originalIsAutoActivateAsyncExecutor)
            {
                asyncExecutor.start();
            }

            booted = true;
            return processEngine;
        }

        protected internal virtual void createTenantSchema(string tenantId)
        {
            logger.LogInformation("creating/validating database schema for tenant " + tenantId);
            tenantInfoHolder.CurrentTenantId = tenantId;
            CommandExecutor.execute(SchemaCommandConfig, new ExecuteSchemaOperationCommand(databaseSchemaUpdate));
            tenantInfoHolder.clearCurrentTenantId();
        }

        protected internal virtual void createTenantAsyncJobExecutor(string tenantId)
        {
            ((ITenantAwareAsyncExecutor)asyncExecutor).addTenantAsyncExecutor(tenantId, AsyncExecutorActivate && booted);
        }

        public override ICommandInterceptor createTransactionInterceptor()
        {
            return null;
        }

        protected internal override void postProcessEngineInitialisation()
        {
            // empty here. will be done in registerTenant
        }

        public override ProcessEngineConfiguration getProcessEngineConfiguration()
        {
            return this;
        }

        public override IUserGroupLookupProxy UserGroupLookupProxy
        {
            get
            {
                return null; //no external identity provider supplied
            }
        }
    }

}