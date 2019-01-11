import { PLATFORM } from 'aurelia-pal';
import { BaseInfo } from "forms/baseinfo";
import { Student } from "forms/student";
import { Techer } from "forms/techer";
import { EventAggregator } from 'aurelia-event-aggregator';
import { inject } from 'aurelia-framework';

@inject(EventAggregator)
export class FormList {

    forms = [
        {
            id: "1",
            name: '基本信息',
            type: PLATFORM.moduleName('./forms/baseInfo')
        },
        {
            id: "2",
            name: '学生注册',
            type: PLATFORM.moduleName('./forms/student'),
            headerStyle:"background-color:blue;color:white;"
        },
        {
            id: "3",
            name: '教师注册',
            type: PLATFORM.moduleName('./forms/techer'),
            headerStyle:"background-color:orange;color:white"
        }
    ]

    nextForm = "1";

    constructor(private es: EventAggregator) {
        this.es.subscribe("started", (task) => {
        });
    }

    activate(model, nctx) {
        var me = this;
        this.es.subscribe("next", function (task) {
            me.nextForm = task.formKey;
        });

        this.es.subscribe("completed", (task) =>{
            this.nextForm = null;
        });
    }
}