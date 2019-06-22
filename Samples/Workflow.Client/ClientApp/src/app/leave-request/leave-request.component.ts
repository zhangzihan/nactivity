import { Component } from '@angular/core';
import { WorkflowService } from "../services/workflow.service";

import * as uuid from 'uuid/v4';

@Component({
  selector: 'app-leave-request',
  templateUrl: './leave-request.component.html'
})
export class LeaveRequestComponent {

  request: any = {};

  constructor(private workflowService: WorkflowService) {
    debugger;
    this.newRequest();
  }

  submit() {
    this.workflowService.submit(this.request).subscribe(r => alert(r));
  }

  newRequest() {
    this.request = {
      id: uuid(),
      start: new Date(),
      end: new Date(),
      reason: "事假"
    }
  }
}
