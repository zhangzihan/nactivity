import { inject } from "aurelia-framework";
import { EventBus } from 'EventBus';

@inject('eventBus')
export class WorkflowWorkspce {

  constructor(private es: EventBus) {

  }
}
