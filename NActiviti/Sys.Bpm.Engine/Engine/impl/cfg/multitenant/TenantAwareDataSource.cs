using Sys.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

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

namespace Sys.Workflow.engine.impl.cfg.multitenant
{


    /// <summary>
    /// A <seealso cref="DataSource"/> implementation that switches the currently used datasource based on the
    /// current values of the <seealso cref="ITenantInfoHolder"/>.
    /// 
    /// When a <seealso cref="Connection"/> is requested from this <seealso cref="DataSource"/>, the correct <seealso cref="DataSource"/>
    /// for the current tenant will be determined and used.
    /// 
    /// Heavily influenced and inspired by Spring's AbstractRoutingDataSource.
    /// 
    /// 
    /// </summary>
    public class TenantAwareDataSource : IDataSource
    {

        protected internal ITenantInfoHolder tenantInfoHolder;
        protected internal IDictionary<object, IDataSource> dataSources = new Dictionary<object, IDataSource>();

        public TenantAwareDataSource(ITenantInfoHolder tenantInfoHolder)
        {
            this.tenantInfoHolder = tenantInfoHolder;
        }

        public virtual void addDataSource(object key, IDataSource dataSource)
        {
            dataSources[key] = dataSource;
        }

        public virtual void removeDataSource(object key)
        {
            dataSources.Remove(key);
        }

        public virtual IDbConnection Connection
        {
            get
            {
                return CurrentDataSource.Connection;
            }
        }

        public virtual IDbConnection getConnection(string provider, string connectionString)
        {
            return CurrentDataSource.getConnection(provider, connectionString);
        }

        protected internal virtual IDataSource CurrentDataSource
        {
            get
            {
                string tenantId = tenantInfoHolder.CurrentTenantId;
                IDataSource dataSource = dataSources[tenantId];
                if (dataSource == null)
                {
                    throw new ActivitiException("Could not find a dataSource for tenant " + tenantId);
                }
                return dataSource;
            }
        }

        public virtual int LoginTimeout
        {
            get
            {
                return 0; // Default
            }
            set
            {
                throw new System.NotSupportedException();
            }
        }

        //public virtual Logger ParentLogger
        //{
        //    get
        //    {
        //        return null;
        //        //return Logger.getLogger(Logger.GLOBAL_LOGGER_NAME);
        //    }
        //}

        public virtual object unwrap<T>()
        {
            Type iface = typeof(T);
            if (iface.IsInstanceOfType(this))
            {
                return this;
            }

            throw new Exception("Cannot unwrap " + this.GetType().FullName + " as an instance of " + iface.FullName);
        }

        public virtual bool isWrapperFor(Type iface)
        {
            return iface.IsInstanceOfType(this);
        }

        public void forceCloseAll()
        {
            this.CurrentDataSource.forceCloseAll();
        }

        public virtual IDictionary<object, IDataSource> DataSources
        {
            get
            {
                return dataSources;
            }
            set
            {
                this.dataSources = value;
            }
        }

        public DbProviderFactory DbProviderFactory => CurrentDataSource.DbProviderFactory;

        public string ConnectionString => CurrentDataSource.ConnectionString;


        // Unsupported //////////////////////////////////////////////////////////
        //public virtual PrintWriter LogWriter
        //{
        //    get
        //    {
        //        throw new System.NotSupportedException();
        //    }
        //    set
        //    {
        //        throw new System.NotSupportedException();
        //    }
        //}
    }

}