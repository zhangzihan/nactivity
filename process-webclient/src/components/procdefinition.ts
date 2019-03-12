
import { PLATFORM } from 'aurelia-framework'

export class ProcDefinitionViewModel {

  processes = PLATFORM.moduleName('../processes');

  deployment = PLATFORM.moduleName('../deployment');

  tasks = PLATFORM.moduleName('../tasks/mytasks');

}
