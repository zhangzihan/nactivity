import Axios from 'axios';
import { EventAggregator } from 'aurelia-event-aggregator';
import { inject, observable } from "aurelia-framework";
import { LoginUser } from 'loginuser';
import contants from 'contants';
import { EventBus } from 'EventBus';
import { HttpInvoker } from 'services/httpInvoker';


@inject('loginUser', 'eventBus', 'httpInvoker')
export class MyTasks {

  tasks = [];

  constructor(private user: LoginUser, private es: EventBus, private httpInvoker: HttpInvoker) {
  }

  activate(model, nctx) {
    this.es.subscribe("reloadMyTasks", () => {
      this.loadMyTasks();
    });

    this.es.subscribe('userLogined', (u) => {
      this.loadMyTasks();
    })
  }

  receivedMyTasks(tasks) {
    this.tasks = tasks;
  }

  @observable select;

  next(task) {
    this.select = task;

    this.es.publish("next", task);
  }

  removeTask() {
    this.httpInvoker.post(`${contants.serverUrl}/workflow/tasks/${this.select.id}/terminate`)
      .then(() => {
        this.es.publish("reloadMyTasks");
      })
      .catch(() => {
        alert('获取任务列表失败');
      });
  }

  loadMyTasks() {
    this.httpInvoker.get(`${contants.serverUrl}/workflow/tasks/${this.user.current.id}/mytasks`)
      .then((tasks) => {
        this.tasks = tasks.data.list;

        this.es.publish("next", this.tasks[0]);
      })
      .catch(() => {
        alert('获取任务列表失败');
      });
  }
}
