import { singleton } from "aurelia-framework";


@singleton()
export class LocalParameterStore {

  getParameters(processKey) {
    let params = localStorage.getItem(processKey);
    if (params == null) {
      return null;
    }
    return JSON.parse(params);
  }

  save(processKey, params) {
    localStorage.setItem(processKey, JSON.stringify(params));
  }

  remove(pid) {
    localStorage.removeItem(pid);
  }

  find(processKey, activityId): any {
    let param = this.getParameters(processKey);
    if (param == null) {
      return null;
    }

    let value = param[activityId];
    if (value != null) {
      return JSON.parse(value.parameterValue);
    }

    return null;
  }
}
