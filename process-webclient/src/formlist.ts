import { PLATFORM } from 'aurelia-pal';
import { BaseInfo } from "forms/baseinfo";
import { Student } from "forms/student";
import { Techer } from "forms/techer";
import { EventAggregator } from 'aurelia-event-aggregator';
import { inject, observable } from 'aurelia-framework';

@inject(EventAggregator)
export class FormList {

    forms = [
        {
            id: 1,
            name: '基本信息',
            formType: 'register',
            type: PLATFORM.moduleName('./forms/baseInfo')
        },
        {
            id: 2,
            name: '学生注册',
            formType: 'register',
            type: PLATFORM.moduleName('./forms/student'),
            headerStyle: "background-color:blue;color:white;"
        },
        {
            id: 3,
            name: '教师注册',
            formType: 'register',
            type: PLATFORM.moduleName('./forms/techer'),
            headerStyle: "background-color:orange;color:white"
        },
        {
            id: 4,
            name: '支付',
            formType: 'register',
            type: PLATFORM.moduleName('./forms/payment'),
            headerStyle: "background-color:red;color:white"
        },
        {
            id: 5,
            name: '编辑论文',
            formType: 'essay',
            type: PLATFORM.moduleName('./essaies/essay')
        },
        {
            id: 6,
            name: '评审员审批',
            formType: 'essay',
            type: PLATFORM.moduleName('./essaies/first'),
            headerStyle: "background-color:red;color:white"
        },
        {
            id: 7,
            name: '主办方审批',
            formType: 'essay',
            type: PLATFORM.moduleName('./essaies/second'),
            headerStyle: "background-color:red;color:white"
        }
    ]

    @observable nextForm = null;

    nextFormChanged(n, o) {
    }
    
    workflow;
    
    constructor(private es: EventAggregator) {
    }

    activate(model, nctx) {
        this.es.subscribe("started", (task) => {
        });

        this.es.subscribe("registerUser", (user) => {
            this.nextForm = 1;
        });

        this.es.subscribe("openWorkflow", (wf) => {
            this.workflow = wf;
        })

        var me = this;
        this.es.subscribe("next", function (task) {
            if (task) {
                me.nextForm = task.formKey;
            } else {
                if (me.workflow) {
                    if (me.workflow.name == "征文评审") {
                        me.nextForm = 5;
                    } else {
                        me.nextForm = 1;
                    }
                } else {
                    me.nextForm = 1;
                }
            }
        });

        this.es.subscribe("completed", (task) => {
            this.nextForm = -1;
        });
    }
}