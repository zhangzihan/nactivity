import { PLATFORM } from 'aurelia-pal';

import $ from 'jquery';

import { RouterConfiguration, Router, Redirect } from 'aurelia-router';
import { LoginUser } from 'model/loginuser';
import { autoinject } from 'aurelia-framework';

export class App {

  private router;

  constructor() {
  }

  configureRouter(config: RouterConfiguration, router: Router): void {
    this.router = router;
    config.title = 'Workflow Studio';
    config.options.pushState = false;
    //config.options.root = '/';
    config.options.hashChange = true;

    config.addPipelineStep('authorize', AuthorizeStep);
    config.map([
      { route: '', redirect: 'studio' },
      { route: 'login', name: 'login', href:'#login', moduleId: PLATFORM.moduleName('./components/login'), title: 'Login' },
      { route: 'studio', name: 'studio', href:"#studio", moduleId: PLATFORM.moduleName('./components/studio'), nav: true, title: 'Workflow Studio' }
    ]);
  }
}

@autoinject()
class AuthorizeStep {

  constructor(private loginUser: LoginUser) {

  }

  run(navigationInstruction, next) {
    if (navigationInstruction.config.name == 'studio') {
      try {
        if (this.loginUser.isSignin() == false) {
          return next.cancel(new Redirect('login'));
        }
      } catch (e) {
        return next.cancel(new Redirect('login'));
      }
    }

    return next();
  }
}
