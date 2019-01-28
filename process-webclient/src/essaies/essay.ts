import { BaseForm } from './../forms/baseform';
import { EssayModel } from './essaymodel';
import { inject } from 'aurelia-framework';
import contants from 'contants';
import Axios from 'axios';

@inject('essayModel')
export class Essay extends BaseForm {

    constructor(public model: EssayModel, ...args) {
        super(args[0], args[1])
    }

    submit() {
        if (this.workflow == null) {
            alert('先选择流程');
            return;
        }

        Axios.post(`${contants.serverUrl}/workflow/process-instances/start`, {
            processDefinitionKey: this.workflow.key,
            businessKey: this.workflow.businessKey,
            variables: {
                "name": this.user.name
            }
        }).then((res) => {
            alert('征文已提交');
            this.model.submitted = true;
            this.es.publish("reloadMyTasks");
            this.es.publish("started", res.data);
        }).catch((res) => {
            alert("是不是没有启动服务或是没有选择流程!那么就是未知错误喽.");
        });

    }
}