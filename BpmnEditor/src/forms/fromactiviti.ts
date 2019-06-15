import { BaseForm } from './baseform';
import Axios from 'axios';
import contants from 'contants';

export class FromActiviti extends BaseForm {

  activityId;

  constructor(...args) {
    super(args[0], args[1], args[2]);
  }

  attached() {
  }

  start() {
    this.httpInvoker.post(`${contants.serverUrl}/process-instances/startbyactiviti`, {
      processId: this.workflow.id,
      activitiId: this.activityId,
      variables: ["新用户1", "评审员"]
    }).then(res => {
      debugger;
    }).catch(res => {

    });
  }
}
