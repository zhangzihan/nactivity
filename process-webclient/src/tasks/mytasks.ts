import Axios from 'axios';
import { EventAggregator } from 'aurelia-event-aggregator';
import { inject } from "aurelia-framework";
import { LoginUser } from 'loginuser';
import contants from 'contants';


@inject('loginUser', EventAggregator)
export class MyTasks {

    tasks = [];

    constructor(private user: LoginUser, private es: EventAggregator) {
        this.es.subscribe("reloadMyTasks", () => {
            this.loadMyTasks();
        });

        this.loadMyTasks();
    }

    receivedMyTasks(tasks) {
        this.tasks = tasks;
    }

    next(task){
        this.es.publish("next", task);
    }

    loadMyTasks() {
        Axios.get(`${contants.serverUrl}/mytasks/${this.user.name}`)
            .then((tasks) => {
                this.tasks = (tasks.data || []).map(x => x.content);

                this.es.publish("next", this.tasks[0]);
            })
            .catch(() => {
                alert('获取任务列表失败');
            });
    }
}