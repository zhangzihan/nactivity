import { ProcessInstanceAdminService } from './services/ProcessInstanceAdminService';


/// <reference types="aurelia-loader-webpack/src/webpack-hot-interface"/>
// we want font-awesome to load as soon as possible to show the fa-spinner
import '../static/styles.css';
import 'font-awesome/css/font-awesome.css';
import 'bootstrap/dist/css/bootstrap.css';
import '../lib/vendor/diagram-js.css';
import '../lib/vendor/bpmn-font/css/bpmn-embedded.css';
import '../lib/css/app.css'
import { Aurelia } from 'aurelia-framework';
import { PLATFORM } from 'aurelia-pal';
import * as Bluebird from 'bluebird';
import { LoginUser } from 'loginuser';
import { ProcessDefineService } from 'services/processdefineservice';
import { EssayModel } from 'essaies/essaymodel';
import { ProcessDefinitionDeployerService } from './services/ProcessDefinitionDeployerService';
import { ProcessInstanceServie } from 'services/ProcessInstanceService';
import { WorkflowWorkspce } from 'WorkflowWorkspace';
import { EventBus } from 'EventBus';
import { WorkflowDocument } from 'WorkflowDocument';
import { HttpInvoker } from 'services/httpInvoker';
import { ProcessInstanceHistoriceService } from 'services/ProcessInstanceHistoricService';

// remove out if you don't want a Promise polyfill (remove also from webpack.config.js)
Bluebird.config({ warnings: { wForgottenReturn: false } });

export async function configure(aurelia: Aurelia) {
  aurelia.use
    .standardConfiguration()
    .developmentLogging()
    .plugin(PLATFORM.moduleName('aurelia-dialog'));


  aurelia.container.registerSingleton("loginUser", LoginUser);
  aurelia.container.registerSingleton('processDefineService', ProcessDefineService);
  aurelia.container.registerSingleton('essayModel', EssayModel);
  aurelia.container.registerSingleton('processDefinitionDeployer', ProcessDefinitionDeployerService);
  aurelia.container.registerSingleton('processInstanceService', ProcessInstanceServie);
  aurelia.container.registerSingleton('processInstanceAdminService', ProcessInstanceAdminService);
  aurelia.container.registerSingleton('historyService', ProcessInstanceHistoriceService);
  aurelia.container.registerSingleton('eventBus', EventBus);
  aurelia.container.registerSingleton('httpInvoker', HttpInvoker);
  aurelia.container.registerSingleton('document', WorkflowDocument);
  aurelia.container.registerSingleton('workspace', WorkflowWorkspce);

  // Uncomment the line below to enable animation.
  // aurelia.use.plugin(PLATFORM.moduleName('aurelia-animator-css'));
  // if the css animator is enabled, add swap-order="after" to all router-view elements

  // Anyone wanting to use HTMLImports to load views, will need to install the following plugin.
  // aurelia.use.plugin(PLATFORM.moduleName('aurelia-html-import-template-loader'));

  await aurelia.start();
  await aurelia.setRoot(PLATFORM.moduleName('app'));
}
