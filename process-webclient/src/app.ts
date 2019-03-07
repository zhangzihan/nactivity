import {Aurelia} from 'aurelia-framework';
import {Router, RouterConfiguration} from 'aurelia-router';
import {PLATFORM} from 'aurelia-pal';

export class App {
  router: Router;

  processes = PLATFORM.moduleName('./processes');

  deployment = PLATFORM.moduleName('./DeploymentViewModel');

  tasks = PLATFORM.moduleName('./tasks/mytasks');
  
  configureRouter(config: RouterConfiguration, router: Router) {
    config.title = 'Aurelia';
    config.map([
      { route: ['', 'welcome'], name: 'welcome',      moduleId: PLATFORM.moduleName('./welcome'),      nav: true, title: 'Welcome' },
      { route: 'users',         name: 'users',        moduleId: PLATFORM.moduleName('./users'),        nav: true, title: 'Github Users' },
      { route: 'child-router',  name: 'child-router', moduleId: PLATFORM.moduleName('./child-router'), nav: true, title: 'Child Router' },
    ]);

    this.router = router;
  }

  maximized = false;

  hideTop = "";

  maxBpmnIo = "";

  maximize(){
    this.maximized = !this.maximized;  
    if (this.maximized){
      this.hideTop = "hide-top";
      this.maxBpmnIo = "max-bpmn-io";
    }else{
      this.hideTop = '';
      this.maxBpmnIo = "";
    }
  }
}
