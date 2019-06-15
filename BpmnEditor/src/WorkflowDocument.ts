import { inject, observable } from 'aurelia-framework';
import { EventBus } from 'EventBus';

@inject('eventBus')
export class WorkflowDocument {

  @observable workflow;

  constructor(private es: EventBus) {
  }

  openWorkflow(doc){
    this.workflow = doc;
    this.es.publish("openWorkflow", this.workflow);
  }
}
