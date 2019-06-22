import { HttpInvoker } from "./httpInvoker";
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Shared } from "../shared";

let serviceUrl = "/api/workflow";

@Injectable({
  providedIn: "root"
})
export class WorkflowService {

  constructor(private httpClient: HttpInvoker) {
  }

  //用户列表
  getUsers(): Observable<any> {
    return this.httpClient.get(Shared.admin, `${serviceUrl}/users`);
  }

  submit(request: any): Observable<any> {
    return this.httpClient.post(Shared.loginUser, `${serviceUrl}/submit`, request);
  }

  passed(id: any): Observable<any> {
    return this.httpClient.get(Shared.loginUser, `${serviceUrl}/passed/${id}`);
  }

  reject(id: any): Observable<any> {
    return this.httpClient.get(Shared.loginUser, `${serviceUrl}/reject/${id}`);
  }

  myTasks(): Observable<any> {
    return this.httpClient.get(Shared.loginUser, `${serviceUrl}/mytasks`);
  }
}
