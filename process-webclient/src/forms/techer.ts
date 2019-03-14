import Axios from 'axios';
import { BaseForm } from './baseform';
import { EventAggregator } from "aurelia-event-aggregator";
import { inject } from "aurelia-framework";
import contants from 'contants';

export class Techer extends BaseForm {

  cityName;

  workflow;

  task;

  constructor(...args) {
    super(args[0], args[1], args[2]);
  }

  activate(model, nctx) {
    super.activate(model, nctx);

    this.es.subscribe("next", (task) => {
      this.task = task;
    });
  }

  submit() {
    this.httpInvoker.post(`${contants.serverUrl}/workflow/tasks/${this.task.id}/complete`, {
      taskId: this.task.id,
      outputVariables: {
        cityName: this.cityName
      }
    }).then((res) => {
      this.es.publish("reloadMyTasks");
      this.es.publish("completed");
    }).catch((res) => {

    });
  }
}
