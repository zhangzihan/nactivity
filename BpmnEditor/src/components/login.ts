
import { Router } from 'aurelia-router';
import { autoinject } from 'aurelia-framework';
import { LoginUser } from 'model/loginuser';
import { Settings } from 'model/settings';

var uuid = require("uuid");

@autoinject()
export class Login {

  name;

  tenantId;

  serverUrl;

  remenberMe = true;

  constructor(private router: Router,
    private loginUser: LoginUser,
    private settings: Settings) {
    this.name = this.loginUser.current.name;
    this.tenantId = this.loginUser.current.tenantId;
  }

  attached() {
  }

  async signin() {

    var user = await this.loginUser.signin(this.name, this.tenantId, this.remenberMe);

    this.settings.save();

    this.router.navigate("studio");
  }

  genId() {
    this.tenantId = uuid();
  }
}
