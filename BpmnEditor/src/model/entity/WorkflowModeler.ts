import { FormField } from 'model/form/formfield';
import { WorkflowParameter } from 'model/entity/WorkflowParameter';
import { LocalParameterStore } from 'model/storage/localparameterstore';
import { BpmnModeler } from 'bpmn-js/lib/Modeler';
import { singleton, autoinject } from 'aurelia-framework';

@singleton()
@autoinject()
export class WorkflowModeler {

  bpmnModeler: BpmnModeler;

  private _processDefinition;

  get processDefinition() {
    return this._processDefinition;
  }

  set processDefinition(processDefinition) {
    this._processDefinition = processDefinition;
  }

  get flowElements() {
    return this.process == null ? null : this.process.flowElements;
  }

  get definitions() {
    return this.bpmnModeler == null ? null : this.bpmnModeler.getDefinitions();
  }

  get process() {
    if (this.bpmnModeler == null) {
      return null;
    }
    var defs = this.bpmnModeler.getDefinitions();
    if (defs == null || defs.rootElements == null) {
      return null;
    }
    return defs.rootElements[0];
  }

  get startEvent() {
    if (this.flowElements == null) {
      return null;
    }

    return this.flowElements.find(x => x.$type == "bpmn:StartEvent");
  }

  constructor(private localParamStore: LocalParameterStore) {

  }

  isError(): boolean {
    if (this.bpmnModeler == null) {
      return false;
    }

    return this.process == null;
  }

  fields(activity): Array<FormField> {
    var formData = (activity.extensionElements || { values: [] }).values.find(x => x.$type == "camunda:FormData");
    let fields = null;
    if (formData) {
      fields = formData.fields.map(f => {
        return {
          id: f.id,
          name: f.label || f.id,
          type: f.type
        }
      });
    } else {
      let params = this.localParamStore.getParameters(this.process.id);
      if (params != null) {
        formData = params[activity.id];
        if (formData) {

          if (formData.parameterValue != null) {
            let param = WorkflowParameter.fromJSON(JSON.parse(formData.parameterValue));
            fields = FormField.fromWorkflowParameter(param);
          }
        }
      }
    }

    return fields == null ? [] : fields;
  }

  find(id): any {
    if (this.flowElements == null) {
      return null;
    }

    function recursiveFind(nodes, id) {
      for (var idx = 0; idx < nodes.length; idx++) {
        let node = nodes[idx];
        if (node.id == id) {
          return node;
        }

        if (node.flowElements) {
          return recursiveFind(node.flowElements, id);
        }
      }

      return null;
    }
    
    return recursiveFind(this.flowElements, id);
  }

  setServiceTaskDefaultClass(id) {
    var task = this.find(id);
    if (task && task.hasOwnProperty("class")) {
      var className = (task.class || "").trim();
      if (className == "") {
        task.class = "Sys.Workflow.Engine.Impl.Bpmn.Behavior.ServiceTaskWebApiActivityBehavior,Sys.Bpm.Engine";

        if (task.hasOwnProperty("extensionElements") == false) {
          var moddle = this.bpmnModeler.injector.get("moddle");
          //创建默认扩展属性:url, method, taskRequest, dataObj, mockData
          var extElem = moddle.create("bpmn:ExtensionElements", {
            values: []
          });
          extElem.$parent = task;

          //创建属性数据源
          var extProperties = moddle.create("camunda:Properties", {
            values: []
          });
          extProperties.$parent = extElem;

          ["url", "method", "taskRequest", "dataObj", "mockData"].forEach(prop => {
            var extProperty = moddle.create("bpmn:Property", {
              name: prop,
              value: ''
            });
            extProperty.value = '';
            extProperty.$parent = extProperties
            extProperties.values.push(extProperty);
          });
          extElem.values.push(extProperties);

          //modeling
          var modeling = this.bpmnModeler.injector.get("modeling");
          task.extensionElements = extElem;
        }

        return true;
      }
    }

    return false;
  }

  clear() {
    this.bpmnModeler.clear();
    this.bpmnModeler._setDefinitions(null);
  }
}
