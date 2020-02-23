import { LoginUser } from '../model/loginuser';
import Axios, { AxiosRequestConfig, AxiosPromise } from 'axios';
import { EventBus } from '../model/EventBus';
import { inject, singleton } from 'aurelia-framework';

@inject('loginUser', 'eventBus')
@singleton()
export class HttpInvoker {

  constructor(private user: LoginUser, private es: EventBus) {

  }

  post(url, data?: any, config?: AxiosRequestConfig): AxiosPromise<any> {
    var cfg = Object.assign({ headers: {} }, config);
    cfg.headers = Object.assign(cfg.headers, this.tokenAccess());
    return Axios.post(url, data, cfg);
  }

  get(url, config?: AxiosRequestConfig): AxiosPromise<any> {
    var cfg = Object.assign({ headers: {} }, config);
    cfg.headers = Object.assign(cfg.headers, this.tokenAccess());

    return Axios.get(url, cfg);
  }

  tokenAccess() {
    return {
      "Authorization": "Bearer " + encodeURIComponent(JSON.stringify(this.user.current))
    }
  }

}
