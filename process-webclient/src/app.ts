import { WorkflowDocument } from './WorkflowDocument';
import { Aurelia, inject, noView } from 'aurelia-framework';
import { Router, RouterConfiguration, Redirect } from 'aurelia-router';
import { PLATFORM } from 'aurelia-pal';

@inject('document')
export class App {
  router: Router;

  constructor(private doc: WorkflowDocument) {

  }

  configureRouter(config: RouterConfiguration, router: Router) {
    config.title = 'Aurelia';
    
    config.addPreActivateStep({
      run: (nav, next) => {
        if (nav.fragment == "/proc-inst" && this.doc.workflow == null) {
          return next.cancel(new Redirect("proc-def"));
        }
        return next();
      }
    });

    config.map([
      {
        route: '',
        redirect: 'proc-def'
      },
      {
        route: 'proc-def',
        name: 'proc-def',
        moduleId: PLATFORM.moduleName('./components/procdefinition'),
        nav: true,
        title: '流程定义'
      },
      {
        route: 'proc-inst',
        name: 'proc-inst',
        moduleId: PLATFORM.moduleName('./components/procinstances'),
        nav: true,
        title: '流程实例'
      },
      {
        route: 'proc-inst/:id/:status',
        name: 'instance',
        moduleId: PLATFORM.moduleName('./components/procinstance'),
        title: '流程实例详情'
      },
      {
        route: 'tasks',
        name: 'tasks',
        moduleId: PLATFORM.moduleName('./components/tasks'),
        nav: true,
        title: '流程任务'
      }
    ]);

    this.router = router;
  }

  maximized = false;

  hideTop = "";

  maxBpmnIo = "";

  maximize() {
    this.maximized = !this.maximized;
    if (this.maximized) {
      this.hideTop = "hide-top";
      this.maxBpmnIo = "max-bpmn-io";
    } else {
      this.hideTop = '';
      this.maxBpmnIo = "";
    }
  }
}
