import { ISort } from "./ISort";

export interface IPageable {

  pageNo: number;

  pageSize: number;

  sort?: Array<ISort>
}
