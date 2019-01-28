import Axios from 'axios';
import { BaseForm } from './baseform';
import { EventAggregator } from "aurelia-event-aggregator";
import { inject } from "aurelia-framework";
import contants from 'contants';

export class Student extends BaseForm {

    className;

    workflow;

    task;

    constructor(...args) {
        super(args[0], args[1]);
    }

    activate(model, nctx) {
        super.activate(model, nctx);

        this.es.subscribe("next", (task) => {
            this.task = task;
        });
    }

    submit() {
        Axios.post(`${contants.serverUrl}/${this.task.id}/complete`, {
            taskId: this.task.id,
            outputVariables: {
                className: this.className
            }
        }).then((res) => {
            this.es.publish("reloadMyTasks");
            this.es.publish("completed");
        }).catch((res) => {

        });
    }
}