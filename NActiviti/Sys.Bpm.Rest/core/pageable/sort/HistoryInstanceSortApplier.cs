using Sys.Workflow.Api.Runtime.Shared.Query;
using Sys.Workflow.Engine.History;
using Sys.Workflow.Engine.Impl;
using Sys.Workflow.Engine.Query;
using Sys.Workflow.Engine.Tasks;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sys.Workflow.Cloud.Services.Core.Pageables.Sorts
{
    /// <summary>
    /// 
    /// </summary>
    public class HistoryInstanceSortApplier : BaseSortApplier<IHistoricProcessInstanceQuery, IHistoricProcessInstance>
    {
        private IDictionary<string, IQueryProperty> orderByProperties = new Dictionary<string, IQueryProperty>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 
        /// </summary>
        public HistoryInstanceSortApplier()
        {
            orderByProperties["id"] = HistoricProcessInstanceQueryProperty.PROCESS_INSTANCE_ID_;
            orderByProperties["name"] = HistoricProcessInstanceQueryProperty.BUSINESS_KEY;
            orderByProperties["startDate"] = HistoricProcessInstanceQueryProperty.START_TIME;
        }

        /// <inheritdoc />
        protected internal override void ApplyDefaultSort(IHistoricProcessInstanceQuery query)
        {
            query.OrderByProcessInstanceStartTime().Asc();
        }

        /// <inheritdoc />
        protected internal override IQueryProperty GetOrderByProperty(Sort.Order order)
        {
            orderByProperties.TryGetValue(order.Property, out IQueryProperty qp);

            return qp;
        }
    }
}
