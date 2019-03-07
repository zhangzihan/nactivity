import { IPageable } from "./IPageable";

export interface IAbstractQuery {

  /**
   * @description 分页和排序设置
   */
  pageable: IPageable;

  /**
   * @description 实体id
   */
  id?: string;


  /**
   * @description 租户id
   */
  tenantId?: string;

  /**
   * @description 实例对象名称
   */
  name?: string;

  /**
   * @description 包含实例对象名称
   */
  nameLike?: string;
}
