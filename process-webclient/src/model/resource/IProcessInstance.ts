import { ProcessInstanceStatus } from "model/ProcessInstanceStatus";

export interface IProcessInstance {
  id: string;
  name: string;
  description: string;
  startDate?: Date;
  initiator: string;
  businessKey: string;
  status?: ProcessInstanceStatus;
  processDefinitionId?: string;
  processDefinitionKey?: string;
}
