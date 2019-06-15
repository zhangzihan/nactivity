import { HttpInvoker } from 'services/httpInvoker';
import { inject } from "aurelia-framework";
import { LoginUser } from "loginuser";
import { EventAggregator } from "aurelia-event-aggregator";
import { EventBus } from "EventBus";


@inject('loginUser', 'eventBus', 'httpInvoker')
export class BaseForm {

  form;

  workflow;

  constructor(protected user?: LoginUser, protected es?: EventBus, protected httpInvoker?: HttpInvoker) {

  }

  activate(model, nctx) {
    this.es.subscribe("openWorkflow", (wf) => {
      this.workflow = wf;
    });
    this.form = model;
  }
}
