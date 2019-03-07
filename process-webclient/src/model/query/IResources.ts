
export interface IResources {

  /**
   * @description 当前页记录数
   */
  recordCount: number;

  /**
   * @description 当前页码
   */
  pageNo: number;

  /***
   * @description 每页大小
   */
  pageSize: number;

  /***
   * @description 总页数
   */
  totalCount: number;

  /***
   * @description 返回的数据
   */
  list: Array<any>;
}
