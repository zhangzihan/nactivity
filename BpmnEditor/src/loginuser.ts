
export interface IUserInfo {
  id: string;
  name: string;
  tenantId?: string
}

export class LoginUser {
  current: IUserInfo = {
    id: "新用户1",
    name: "新用户1",
    tenantId: "cb79f3dd-e84e-49b0-95c2-0bdafc80f09d"
  }
}
