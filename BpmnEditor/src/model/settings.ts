import { singleton } from "aurelia-framework";

import * as environment from '../../config/environment.json';

@singleton()
export class Settings {
  private serverUrl;

  constructor() {
    if (environment.debug == true) {
      this.serverUrl = "http://localhost:11015";
    } else {
      this.serverUrl = "http://rest.31huiyi.com";
    }
    var url = window.localStorage.getItem("workflow_serverurl");
    if (url == null) {
      url = this.serverUrl;
      this.save();
    }
    this.serverUrl = url;
  }

  save() {
    window.localStorage.setItem("workflow_serverurl", this.serverUrl);
  }

  getUrl(path) {
    return `${this.serverUrl}/api/v1/${path}`;
  }
}
