import { inject } from "aurelia-framework";
import { LoginUser } from "loginuser";
import { EventAggregator } from "aurelia-event-aggregator";


@inject('loginUser', EventAggregator)
export class BaseForm {

    form;

    constructor(protected user?: LoginUser, protected es?: EventAggregator) {

    }

    activate(model, nctx){
        this.form = model;
    }
}