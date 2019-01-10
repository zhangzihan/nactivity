import { EventAggregator } from "aurelia-event-aggregator";
import { inject } from "aurelia-framework";

@inject(EventAggregator)
export class Techer {

    cityName;

    workflow;

    constructor(private es: EventAggregator) {
        this.es.subscribe("openWorkflow", (wf) => {
            this.workflow = wf;
        });
    }

}