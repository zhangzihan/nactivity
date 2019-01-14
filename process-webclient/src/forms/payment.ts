import { BaseForm } from './baseform';
import contants from 'contants';
import Axios from 'axios';

export class Payment extends BaseForm {

    money;

    task;

    constructor(...args) {
        super(args[0], args[1])
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
                money: this.money
            }
        }).then((res) => {
            this.es.publish("reloadMyTasks");
            this.es.publish("completed");
        }).catch((res) => {

        });
    }
}