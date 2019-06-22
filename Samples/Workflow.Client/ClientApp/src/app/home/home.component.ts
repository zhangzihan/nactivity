import { Shared } from "./../shared";
import { WorkflowService } from "./../services/workflow.service";
import { Component } from '@angular/core';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
})
export class HomeComponent {

  users: Array<any>;

  constructor(private workflowService: WorkflowService) {
    this.workflowService.getUsers().subscribe(users => {
      this.users = users;
    });
  }

  login(user) {
    Shared.loginUser = user;
  }
}
