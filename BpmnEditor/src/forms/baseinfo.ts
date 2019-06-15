import Axios from 'axios';
import { EventAggregator } from "aurelia-event-aggregator";
import { inject } from 'aurelia-framework';
import { BaseForm } from './baseform';
import contants from '../contants';
import { IProcessInstanceService } from 'services/IProcessInstanceService';

@inject('processInstanceService')
export class BaseInfo extends BaseForm {

  isTecher = false;

  constructor(private processInstanceSvc: IProcessInstanceService, ...args) {
    super(args[0], args[1], args[2]);
  }

  submit() {

    if (this.workflow == null) {
      alert('先选择流程');
      return;
    }

    this.es.publish("registeredUser", this.user);

    this.processInstanceSvc.start([{
      processDefinitionKey: this.workflow.key,
      businessKey: this.workflow.businessKey,
      variables: {
        // "url": "http://event.31huiyi.com/manage/test3",
        // "postdata": {
        //   name: this.user.current.name,
        //   isTecher: this.isTecher
        // },
        "user": [this.user.current.name],
        //"users": ['新用户1','评审员'],//this.user.current.name],//'新用户1','评审员'
        // "isTecher": this.isTecher,
        // "formData": {
        //   "id": "123423",
        //   "items": [
        //     { "id": "3434343", "name": "test_product" }
        //   ]
        // }
      }
    }]).then((res) => {
      this.es.publish("reloadMyTasks");
      this.es.publish("started", res);
    }).catch((res) => {
      alert("是不是没有启动服务或是没有选择流程!那么就是未知错误喽.");
    });
  }
}
