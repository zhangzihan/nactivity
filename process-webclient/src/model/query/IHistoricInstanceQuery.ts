import { IAbstractQuery } from "./IAbstractQuery";

export interface IHistoricInstanceQuery extends IAbstractQuery {

  /// <summary>
  /// 业务键
  /// </summary>        
  businessKey?: string;

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
  /// 是否不使用租户id
  /// </summary>
  withoutTenantId?: boolean;
}
