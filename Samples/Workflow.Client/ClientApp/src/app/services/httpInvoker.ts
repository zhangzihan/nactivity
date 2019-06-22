import { HttpClient } from "@angular/common/http";
import { Injectable } from '@angular/core';
import { Observable } from "rxjs";

@Injectable({
  providedIn: "root"
})
export class HttpInvoker {

  constructor(private httpClient: HttpClient) {

  }

  baseUrl = "http://localhost:8009"

  post(user: any, url, data?: any, options?: any): Observable<any> {
    var cfg = Object.assign({ headers: {} }, options);
    cfg.headers = Object.assign(cfg.headers, this.tokenAccess(user));
    return this.httpClient.post(`${this.baseUrl}${url}`, data, cfg);
  }

  get(user: any, url, options?: any): Observable<any> {
    var cfg = Object.assign({ headers: {} }, options);
    cfg.headers = Object.assign(cfg.headers, this.tokenAccess(user));

    return this.httpClient.get(`${this.baseUrl}${url}`, cfg);
  }

  private tokenAccess(user) {
    return {
      "Authorization": "Bearer " + encodeURIComponent(JSON.stringify(user))
    }
  }
}
