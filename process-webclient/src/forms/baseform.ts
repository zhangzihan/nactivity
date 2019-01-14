import { inject } from "aurelia-framework";
import { LoginUser } from "loginuser";
import { EventAggregator } from "aurelia-event-aggregator";


@inject('loginUser', EventAggregator)
export class BaseForm {

    form;

    workflow;

    constructor(protected user?: LoginUser, protected es?: EventAggregator) {

    }

    activate(model, nctx){
        this.es.subscribe("openWorkflow", (wf) => {
            this.workflow = wf;
        });
        this.form = model;
    }
}