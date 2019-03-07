import { ISort } from "./ISort";

export interface IPageable {

  offset: number;

  pageSize: number;

  sort?: Array<ISort>
}
