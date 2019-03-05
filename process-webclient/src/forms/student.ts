import Axios from 'axios';
import { BaseForm } from './baseform';
import { EventAggregator } from "aurelia-event-aggregator";
import { inject } from "aurelia-framework";
import contants from 'contants';

export class Student extends BaseForm {

  className;

  workflow;

  task;

  approvaled = false;

  toUser;

  constructor(...args) {
    super(args[0], args[1]);
  }

  activate(model, nctx) {
    super.activate(model, nctx);

    this.es.subscribe("next", (task) => {
      this.task = task;
    });
  }

  toUsers() {
    Axios.post(`${contants.serverUrl}/workflow/tasks/${this.task.id}/subtask`, {
      taskId: this.task.id,
      name: this.task.name,
      assignee: this.toUser,
      parentTaskId: this.task.id
    }).then((res) => {
      this.es.publish("reloadMyTasks");
      this.es.publish("completed");
    }).catch((res) => {

    });
  }

  submit() {
    Axios.post(`${contants.serverUrl}/workflow/tasks/${this.task.id}/complete`, {
      taskId: this.task.id,
      outputVariables: {
        approvaled: this.approvaled,
        className: this.className
      }
    }).then((res) => {
      this.es.publish("reloadMyTasks");
      this.es.publish("completed");
    }).catch((res) => {

    });
  }

  terminate() {
    Axios.post(`${contants.serverUrl}/workflow/process-instances/${this.task.processInstanceId}/terminate`, {
      reason: "终止流程" 
    }).then((res) => {
      this.es.publish("reloadMyTasks");
      this.es.publish("completed");
    }).catch((res) => {

    });
  }
}
