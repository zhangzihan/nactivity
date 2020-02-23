import { IPageable } from './IPageable';

export enum TaskStatus {

  /// <summary>
  /// 已创建
  /// </summary>
  CREATED,

  /// <summary>
  /// 已分配
  /// </summary>
  ASSIGNED,

  /// <summary>
  /// 已挂起
  /// </summary>
  SUSPENDED,

  /// <summary>
  /// 已取消
  /// </summary>
  CANCELLED
}

export interface ITaskQuery {
  tenantId?: any;
  id?: any;
  owner?: any
  assignee?: any;
  name?: any;
  description?: any;
  createdDate?: Date;
  endDate?: Date;
  claimedDate?: Date;
  dueDate?: Date;
  processDefinitionId?: any;
  processInstanceId?: any;
  parentTaskId?: any;
  status?: any;
  formKey?: any;
  pageable?: IPageable
}
