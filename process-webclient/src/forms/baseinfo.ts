import Axios from 'axios';
import { EventAggregator } from "aurelia-event-aggregator";
import { inject } from 'aurelia-framework';
import constants from '../contants';

@inject(EventAggregator)
export class BaseInfo {

    name;

    isTecher;

    workflow;

    constructor(private es: EventAggregator) {
        this.es.subscribe("openWorkflow", (wf) => {
            this.workflow = wf;
        });
    }

    submit() {
        Axios.post(`${constants.serverUrl}/workflow/process-instances/start`, {
            processDefinitionKey:this.workflow.key,
            variables: {
                "name": this.name,
                "isTecher": this.isTecher
            }
        }).then(() => {
            debugger;
        }).catch(() => {
            debugger;
        });
    }
}