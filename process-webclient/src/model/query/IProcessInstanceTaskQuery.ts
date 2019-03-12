import { IAbstractQuery } from "./IAbstractQuery";

export interface IProcessInstanceTaskQuery extends IAbstractQuery {

  /// <summary>
  /// 流程实例id
  /// </summary>
  processInstanceId?: string;
}
