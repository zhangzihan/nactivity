import { IAbstractQuery } from "./IAbstractQuery";

export interface IProcessInstanceQuery extends IAbstractQuery {
  /// <summary>
  /// 业务键
  /// </summary>
  businessKey?: string;

  /// <summary>
  /// 流程部署id
  /// </summary>
  deploymentId?: string;

  /// <summary>
  /// 查询多个流程部署id
  /// </summary>
  deploymentIds?: Array<string>;

  /// <summary>
  /// 是否排除子流程
  /// </summary>
  excludeSubprocesses?: boolean;

  /// <summary>
  /// 流程实例id
  /// </summary>
  executionId?: string;

  /// <summary>
  /// 包含指定业务键的子流程
  /// </summary>
  includeChildExecutionsWithBusinessKeyQuery?: boolean;

  /// <summary>
  /// 是否包含流程变量
  /// </summary>
  includeProcessVariables?: boolean;

  /// <summary>
  /// 查询分配人
  /// </summary>
  involvedUser?: string;

  /// <summary>
  /// 是否查询包含异常的作业任务
  /// </summary>
  isWithException?: boolean;

  /// <summary>
  /// 仅查询子流程
  /// </summary>
  onlyChildExecutions?: boolean;

  /// <summary>
  /// 
  /// </summary>
  onlyProcessInstanceExecutions?: boolean;

  /// <summary>
  /// 
  /// </summary>
  onlyProcessInstances?: boolean;

  /// <summary>
  /// 
  /// </summary>
  onlySubProcessExecutions?: boolean;

  /// <summary>
  /// 上级流程实例id
  /// </summary>
  parentId?: string

  /// <summary>
  /// 流程定义目录
  /// </summary>
  processDefinitionCategory?: string;

  /// <summary>
  /// 流程定义id
  /// </summary>
  processDefinitionId?: string;

  /// <summary>
  /// 多个流程定义id
  /// </summary>
  processDefinitionIds?: Array<string>;

  /// <summary>
  /// 流程定义业务键
  /// </summary>
  processDefinitionKey?: string;

  /// <summary>
  /// 流程定义业务键列表
  /// </summary>
  processDefinitionKeys?: Array<string>;

  /// <summary>
  /// 流程定义名称
  /// </summary>
  processDefinitionName?: string;

  /// <summary>
  /// 流程定义版本号
  /// </summary>
  processDefinitionVersion?: number;

  /// <summary>
  /// 流程实例id
  /// </summary>
  processInstanceId?: string;

  /// <summary>
  /// 流程实例id列表
  /// </summary>
  processInstanceIds?: Array<string>;

  /// <summary>
  /// 
  /// </summary>
  processInstanceVariablesLimit?: number;

  /// <summary>
  /// 在某个时间之后开始
  /// </summary>
  startedAfter?: Date;

  /// <summary>
  /// 在某个时间之前开始
  /// </summary>
  startedBefore?: Date;

  /// <summary>
  /// 启动用户
  /// </summary>
  startedBy?: string;

  /// <summary>
  /// 子流程id
  /// </summary>
  subProcessInstanceId?: string;

  /// <summary>
  /// 
  /// </summary>
  superProcessInstanceId?: string;

  /// <summary>
  /// 流程挂起状态
  /// </summary>
  suspensionState?: string;

  /// <summary>
  /// 是否不使用租户id
  /// </summary>
  sithoutTenantId?: boolean
}
