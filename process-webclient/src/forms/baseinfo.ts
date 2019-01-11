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
            debugger;
        });
    }
}