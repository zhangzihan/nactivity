import { autoinject } from "aurelia-framework";

export interface IUserInfo {
  id: string;
  name: string;
  tenantId?: string;
}

@autoinject()
export class LoginUser {
  current: IUserInfo = {
    id: null,
    name: null,
    tenantId: "3b450000-00f0-5254-168b-08d6f4673e73"
  }

  constructor() {
    var user: any = window.localStorage.getItem("workflow_studio");
    if (user != null) {
      user = JSON.parse(user);
      this.current.id = user.id;
      this.current.name = user.name;
      this.current.tenantId = user.tenantId;
    }
  }

  isSignin() {
    return this.current.name != null && this.current.name.replace(/^\s+|\+$/, "") != "";
  }

  signin(name, tenantId, remenberMe) {
    if (name == null) {
      throw new Error("name is null.");
    }

    return new Promise((res, rej) => {
      this.current.name = name.trim();
      this.current.id = this.current.name;
      this.current.tenantId = tenantId;

      if (remenberMe === true) {
        window.localStorage.setItem("workflow_studio", JSON.stringify(this.current));
      }

      res(true);
    });
  }

  signout() {

    return new Promise((res, rej) => {
      window.localStorage.removeItem("workflow_studio");

      res(true);
    });
  }
}
