import Axios from 'axios';
import { EventAggregator } from "aurelia-event-aggregator";
import { inject } from 'aurelia-framework';
import constants from '../contants';
import { BaseForm } from './baseform';

export class BaseInfo extends BaseForm {

    name;

    isTecher = false;

    workflow;

    constructor(...args) {
        super(args[0], args[1]);
        this.es.subscribe("openWorkflow", (wf) => {
            this.workflow = wf;
        });
        this.name = this.user.name;
    }

    submit() {
        this.user.name = this.name;

        Axios.post(`${constants.serverUrl}/workflow/process-instances/start`, {
            processDefinitionKey: this.workflow.key,
            businessKey: this.workflow.businessKey,
            variables: {
                "name": this.name,
                "initiator": this.name,
                "isTecher": this.isTecher
            }
        }).then((res) => {
            this.es.publish("reloadMyTasks");
            this.es.publish("started", res.data);
        }).catch((res) => {
            alert("是不是没有启动服务或是没有选择流程!那么就是未知错误喽.");
        });
    }
}