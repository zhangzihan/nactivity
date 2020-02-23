import { LoginUser } from "./model/loginuser";
import 'jquery';
import 'jquery-ui';

import {Aurelia} from 'aurelia-framework'
import * as environment from '../config/environment.json';
import {PLATFORM} from 'aurelia-pal';



import { enableRipple } from '@syncfusion/ej2-base';
enableRipple(true);

import { EventBus } from "model/EventBus";

export function configure(aurelia: Aurelia) {
  aurelia.use
    .standardConfiguration()
    .feature(PLATFORM.moduleName('resources/index'))
    .plugin(PLATFORM.moduleName('aurelia-dialog'));

  aurelia.use.developmentLogging(environment.debug ? 'debug' : 'warn');

  if (environment.testing) {
    aurelia.use.plugin(PLATFORM.moduleName('aurelia-testing'));
  }
  
  aurelia.container.registerSingleton('eventBus', EventBus);
  aurelia.container.registerSingleton("loginUser", LoginUser);

  aurelia.start().then(() => aurelia.setRoot(PLATFORM.moduleName('app')));
}
