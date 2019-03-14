import { LoginUser } from 'loginuser';
import Axios, { AxiosRequestConfig, AxiosPromise } from 'axios';
import { EventBus } from 'EventBus';
import { inject } from 'aurelia-framework';

@inject('loginUser', 'eventBus')
export class HttpInvoker {

  constructor(private user: LoginUser, private es: EventBus) {

  }

  post(url, data?: any, config?: AxiosRequestConfig): AxiosPromise<any> {
    var cfg = Object.assign(this.tokenAccess(), config);

    return Axios.post(url, data, cfg);
  }

  get(url, config?: AxiosRequestConfig): AxiosPromise<any> {
    var cfg = Object.assign(this.tokenAccess(), config);

    return Axios.get(url, cfg);
  }

  private tokenAccess() {
    return {
      headers: {
        "Authorization": "Bearer " + encodeURIComponent(JSON.stringify(this.user.current))
      }
    }
  }

}
