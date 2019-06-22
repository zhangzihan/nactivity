import { WorkflowService } from "./../services/workflow.service";
import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Shared } from "../shared";

@Component({
  selector: 'app-my-tasks',
  templateUrl: './my-tasks.component.html'
})
export class MyTasksComponent {

  tasks;

  loginUser = Shared.loginUser;

  constructor(private workflowService: WorkflowService) {
    this.workflowService.myTasks().subscribe(tasks => {
      debugger
      this.tasks = tasks.list;
    });
  }

  passed(task) {
    this.workflowService.passed(task.businessKey).subscribe(r => alert(r));
  }

  reject(task) {
    this.workflowService.reject(task.businessKey).subscribe(r => alert(r));
  }
}
