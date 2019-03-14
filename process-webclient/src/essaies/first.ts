import { BaseForm } from './../forms/baseform';
import { inject } from 'aurelia-framework';
import { EssayModel } from './essaymodel';
import Axios from 'axios';
import contants from 'contants';

@inject('essayModel')
export class First extends BaseForm {

    constructor(public model: EssayModel, ...args) {
        super(args[0], args[1], args[2]);
    }

    task;

    activate(m, nctx) {
        this.es.subscribe("next", (task) => {
            this.task = task;
        });
    }

    submit() {
        this.httpInvoker.post(`${contants.serverUrl}/${this.task.id}/complete`, {
            taskId: this.task.id,
            outputVariables: {
                approvaled: this.model.approvaled
            }
        }).then((res) => {
            this.es.publish("reloadMyTasks");
            this.es.publish("completed");
        }).catch((res) => {

        });
    }
}
