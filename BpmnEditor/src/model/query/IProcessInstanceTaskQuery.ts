import { IAbstractQuery } from "./IAbstractQuery";

export interface IProcessInstanceTaskQuery extends IAbstractQuery {

  /// <summary>
  /// 流程实例id
  /// </summary>
  processInstanceId?: string;

  /// <summary>
  /// 包含已完成的实例
  /// </summary>
  includeCompleted?: boolean;
}
