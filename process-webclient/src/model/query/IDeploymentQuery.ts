import { IAbstractQuery } from "./IAbstractQuery";

export interface IDeploymentQuery extends IAbstractQuery {

  /**
   * @description 多个部署id
   */
  ids?: Array<string>;

  /**
   * @description 关联业务数据键值
   */
  key?: string;

  /**
   * @description 关联业务数据键值
   */
  keyLike?: string;

  /**
   * @description 多个关联业务数据键值
   */
  keys?: Array<string>;

  /**
   * @description 关联业务键值
   */
  businessKey?: string;

  /**
   * @description 不使用租户id
   */
  withoutTenantId?: boolean;

  /***
   * @description 仅查询最后部署的流程
   */
  latestDeployment?: boolean;
}
