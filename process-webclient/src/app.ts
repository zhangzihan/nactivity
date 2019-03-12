import { Aurelia } from 'aurelia-framework';
import { Router, RouterConfiguration } from 'aurelia-router';
import { PLATFORM } from 'aurelia-pal';

export class App {
  router: Router;

  configureRouter(config: RouterConfiguration, router: Router) {
    config.title = 'Aurelia';
    config.map([
      // { route: ['', 'welcome'], name: 'welcome', moduleId: PLATFORM.moduleName('./welcome'), nav: true, title: 'Welcome' },
      // { route: 'users', name: 'users', moduleId: PLATFORM.moduleName('./users'), nav: true, title: 'Github Users' },
      // { route: 'child-router', name: 'child-router', moduleId: PLATFORM.moduleName('./child-router'), nav: true, title: 'Child Router' },
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
        route: 'proc-inst/:id',
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
