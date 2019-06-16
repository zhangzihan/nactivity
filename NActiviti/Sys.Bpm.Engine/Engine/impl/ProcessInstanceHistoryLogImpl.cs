using System;
using System.Collections.Generic;

namespace Sys.Workflow.engine.impl
{

    using Sys.Workflow.engine.history;
    using System.Linq;

    /// 
    public class ProcessInstanceHistoryLogImpl : IProcessInstanceHistoryLog
    {

        protected internal IHistoricProcessInstance historicProcessInstance;

        protected internal IList<IHistoricData> historicData = new List<IHistoricData>();

        public ProcessInstanceHistoryLogImpl(IHistoricProcessInstance historicProcessInstance)
        {
            this.historicProcessInstance = historicProcessInstance;
        }

        public virtual string Id
        {
            get
            {
                return historicProcessInstance.Id;
            }
        }

        public virtual string BusinessKey
        {
            get
            {
                return historicProcessInstance.BusinessKey;
            }
        }

        public virtual string ProcessDefinitionId
        {
            get
            {
                return historicProcessInstance.ProcessDefinitionId;
            }
        }

        public virtual DateTime? StartTime
        {
            get
            {
                return historicProcessInstance.StartTime;
            }
        }

        public virtual DateTime? EndTime
        {
            get
            {
                return historicProcessInstance.EndTime;
            }
        }

        public virtual long? DurationInMillis
        {
            get
            {
                return historicProcessInstance.DurationInMillis;
            }
        }

        public virtual string StartUserId
        {
            get
            {
                return historicProcessInstance.StartUserId;
            }
        }

        public virtual string StartActivityId
        {
            get
            {
                return historicProcessInstance.StartActivityId;
            }
        }

        public virtual string DeleteReason
        {
            get
            {
                return historicProcessInstance.DeleteReason;
            }
        }

        public virtual string SuperProcessInstanceId
        {
            get
            {
                return historicProcessInstance.SuperProcessInstanceId;
            }
        }

        public virtual string TenantId
        {
            get
            {
                return historicProcessInstance.TenantId;
            }
        }

        public virtual IList<IHistoricData> HistoricData
        {
            get
            {
                return historicData;
            }
        }

        public virtual void addHistoricData(IHistoricData historicEvent)
        {
            historicData.Add(historicEvent);
        }

        public virtual void AddHistoricData<T1>(ICollection<T1> historicEvents) where T1 : IHistoricData
        {
            ((List<IHistoricData>)historicData).AddRange(historicEvents as IEnumerable<IHistoricData>);
        }

        public virtual void orderHistoricData()
        {
            historicData.OrderBy(x => x, new ComparatorAnonymousInnerClass(this));
        }

        private class ComparatorAnonymousInnerClass : IComparer<IHistoricData>
        {
            private readonly ProcessInstanceHistoryLogImpl outerInstance;

            public ComparatorAnonymousInnerClass(ProcessInstanceHistoryLogImpl outerInstance)
            {
                this.outerInstance = outerInstance;
            }

            public int Compare(IHistoricData data1, IHistoricData data2)
            {
                if (!data1.Time.HasValue && data2.Time.HasValue)
                {
                    return 1;
                }

                if (data2.Time.HasValue && !data2.Time.HasValue)
                {
                    return -1;
                }

                return data1.Time.Value.CompareTo(data2.Time.Value);
            }
        }

    }

}