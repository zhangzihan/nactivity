
export interface IUserInfo {
  id: string;
  name: string;
  tenantId?: string
}

export class LoginUser {
  current: IUserInfo = {
    id: "新用户1",
    name: "新用户1",
    tenantId: ""
  }
}
