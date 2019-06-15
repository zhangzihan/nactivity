import { ProcessInstanceStatus } from "model/ProcessInstanceStatus";

export enum HistoricInstanceStatus {
  /// <summary>
  /// 已完成
  /// </summary>
  COMPLETED,

  /// <summary>
  /// 已终止
  /// </summary>
  DELETED
}

export interface IHistoricInstance {
  id: string;
  name: string;
  description: string;
  startDate: Date;
  endDate: Date;
  businessKey: string;
  status: string;
  processDefinitionId: string;
  processDefinitionKey: string;
  processDefinitionVersion: number;
  durationInMillis: number;
  startUserId: string;
  deleteReason: string;
  tenantId: string;
}
