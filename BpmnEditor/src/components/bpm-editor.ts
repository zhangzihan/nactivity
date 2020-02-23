import { ProcessDefineService } from "services/processdefineservice";
import { EventAggregator, Subscription } from "aurelia-event-aggregator";

import { autoinject, Aurelia } from "aurelia-framework";

import Axios from 'axios'
import $ from 'jquery';
import BpmnModeler from 'bpmn-js/lib/Modeler';
import BpmnModdle from 'bpmn-moddle';

import propertiesPanelModule from 'bpmn-js-properties-panel';
import propertiesProviderModule from 'bpmn-js-properties-panel/lib/provider/camunda';
import camundaModdleDescriptor from 'camunda-bpmn-moddle/resources/camunda.json';

import {
  debounce
} from 'lodash';
import contants from "contants";
import { ProcessDefinitionDeployerService } from "../services/ProcessDefinitionDeployerService";
import { tsImportEqualsDeclaration } from "@babel/types";

import minimapModule from 'diagram-js-minimap';

const shortid = require('shortid');
const uuid = require('uuid');
const format = require('string-format');

declare var ace: any;

import { Tab, HeaderPosition } from '@syncfusion/ej2-navigations'
import { WorkflowParameterComponent } from "./workflow-parameter";
import { LocalParameterStore } from "model/storage/localparameterstore";
import { FormTemplate } from "model/form/formtemplate";
import { compile } from "@syncfusion/ej2-base";
import { WorkflowParameter } from "model/entity/WorkflowParameter";
import { FormField } from "model/form/formfield";
import { WorkflowModeler } from './../model/entity/WorkflowModeler';
import { LoginUser } from "model/loginuser";

@autoinject()
export class BpmEditor {

  container;

  canvas;

  //事件BUS
  eventBus;

  //选定的节点
  node;

  element;

  gfx;

  definitions;

  moddle;

  modeling;

  registry;

  tabBar: Tab;

  private watch: Array<Subscription>;

  private editor;

  constructor(private ea: EventAggregator,
    private processDefineService: ProcessDefineService,
    private processDeployerSvc: ProcessDefinitionDeployerService,
    private aurelia: Aurelia,
    private localParamStore: LocalParameterStore,
    private formTemplate: FormTemplate,
    private workflowModeler: WorkflowModeler,
    private loginUser: LoginUser) {
  }

  attached() {

    this.subscribeEvent();

    this.createTabbar();

    this.createXmlEditor();

    this.createBpmEditor();
  }

  private createTabbar() {

    var me = this;
    this.tabBar = new Tab({
      headerPlacement: "Bottom",
      heightAdjustMode: "Fill",

      selecting: (e) => {
        if (e.selectingIndex >= 1) {
          me.saveDiagram(() => {
          });
        }
      }
    });
    this.tabBar.appendTo("#tab-content");
  }

  private createBpmEditor() {
    window["bpmnViewModel"] = this;

    window["bpmContainer"] = this.container = $('#js-drop-zone');

    window["bpmCanvas"] = this.canvas = $('#js-canvas');

    window["bpmModeler"] = this.workflowModeler.bpmnModeler = new BpmnModeler({
      container: this.canvas,
      keyboard: { bindTo: document },
      propertiesPanel: {
        parent: '#js-properties-panel'
      },
      additionalModules: [
        propertiesPanelModule,
        propertiesProviderModule,
        minimapModule
      ],
      moddleExtensions: {
        camunda: camundaModdleDescriptor
      }
    });

    this.workflowModeler.bpmnModeler.get('minimap').close();

    //取到画布事件总线
    window["eventBus"] = this.eventBus = this.workflowModeler.bpmnModeler.get('eventBus');

    this.eventBus.on("element.changed", (sender, args) => {
      var element = args.element;
      if (element.type == "bpmn:ServiceTask" && this.workflowModeler.setServiceTaskDefaultClass(element.id)) {

      }
    });
  }

  private subscribeEvent() {
    this.ea.subscribe('selectedProcessDefinitionVersion', (processDefinitino) => {
      this.openWorkflow(processDefinitino);
    });

    this.ea.subscribe("updateBpmn", () => {
      this.updateBpmnXml();
    });

    this.ea.subscribe("removeWorkflow", () => {
      this.remove();
    });

    this.ea.subscribe("saveWorkflow", () => {
      this.save();
    });

    this.ea.subscribe("deployWorkflow", () => {
      this.deploy();
    });

    this.ea.subscribe("copyWorkflow", () => {
      this.copy();
    });

    this.ea.subscribe("showBpmXml", () => {
      this.showBpmnXml();
    });

    this.ea.subscribe("saveAsImage", () => {
      this.saveAsImage();
    })

    this.ea.subscribe("downloadBpmXml", () => {
      this.downloadBpmXml();
    });

    this.ea.subscribe("setupWorkflowParameter", () => {
      this.setupWorkflowParameter();
    });

    this.ea.subscribe("selectedElement", (element) => {
      this.onSelectedElement(element);
    });

    this.ea.subscribe("runWorkflow", () => {
      this.onRunWorkflow();
    });
  }

  private onRunWorkflow() {
    var startEvent = this.workflowModeler.flowElements.find(x => x.$type == "bpmn:StartEvent");
    var formData = (startEvent.extensionElements || { values: [] }).values.find(x => x.$type == "camunda:FormData");
    let fields = null;
    if (formData) {
      fields = formData.fields.map(f => {
        return {
          id: f.id,
          name: f.label || f.id,
          type: f.type
        }
      });

      this.formTemplate.openTemplateDialog(fields);
    } else {
      let params = this.localParamStore.getParameters(this.workflowModeler.process.id);
      formData = params[startEvent.id];
      if (formData) {
        let param = WorkflowParameter.fromJSON(JSON.parse(formData.parameterValue));
        fields = FormField.fromWorkflowParameter(param);

        this.formTemplate.openTemplateDialog(fields);
      }
    }
  }

  private onSelectedElement(element) {
    var elementRegistry = this.workflowModeler.bpmnModeler.get("elementRegistry");
    var shape = elementRegistry.get(element.id);
    var g = document.querySelector(`g[data-element-id=${element.id}]`);
    this.eventBus.fire("element.click", { element: shape, gfx: g });
  }

  private createXmlEditor() {
    this.editor = ace.edit("code-xml");
    this.editor.setTheme("ace/theme/textmate");
    this.editor.setShowPrintMargin(false);
    this.editor.getSession().setMode("ace/mode/xml");
    this.editor.getSession().setUseWrapMode(true);
  }

  private async openWorkflow(processDefinition) {
    this.tabBar.select(0);
    if (processDefinition != null) {
      if (processDefinition.id == "-1") {
        var dep = await this.processDeployerSvc.draft(processDefinition.name);
        if (dep != null) {
          var xml = await this.processDeployerSvc.getProcessModel(dep.id);
          processDefinition.xml = xml;
        } else {
          processDefinition.xml = null;
        }
      } else if (processDefinition.version > 0) {
        await this.processDefineService.getProcessModel(processDefinition.id).then(data => {
          if (data != null && data != "") {
            processDefinition.xml = data;
          } else if (processDefinition.xml == null) {
            processDefinition.xml = contants.defaultBpmXml("新的流程");
          }
        });
      }

      this.openDiagram(processDefinition.xml);
    } else {
      this.workflowModeler.clear();
      this.editor.setValue("");
    }
    this.workflowModeler.processDefinition = processDefinition;
  }

  deactivate() {
  }

  openDiagram(xml) {
    this.workflowModeler.bpmnModeler.importXML(xml, (err) => {
      if (err) {
        this.container
          .removeClass('with-diagram')
          .addClass('with-error');

        this.container.find('.error pre').text(err.message);
        this.workflowModeler.clear();
      } else {
        this.container
          .removeClass('with-error')
          .addClass('with-diagram');
      }

      window["definitions"] = this.definitions = this.workflowModeler.bpmnModeler.getDefinitions()
      window["modeling"] = this.modeling = this.workflowModeler.bpmnModeler.injector.get('modeling')
      window["moddle"] = this.moddle = this.workflowModeler.bpmnModeler.injector.get('moddle')
      window["registry"] = this.registry = this.workflowModeler.bpmnModeler.injector.get("elementRegistry")
    });
  }

  saveSVG(done) {
    this.workflowModeler.bpmnModeler.saveSVG(done);
  }

  copy() {
    var me = this;
    this.saveDiagram((err, xml) => {
      var moddle = new BpmnModdle();

      var model = moddle.fromXML(xml, (err, definitions) => {
        moddle.toXML(definitions, (err, xml) => {
        })
      })
    });
  }

  deploy() {
    let me = this;
    this.saveDiagram((err, xml) => {
      me.workflowModeler.processDefinition.xml = xml;
      me.workflowModeler.processDefinition.key = this.workflowModeler.process.id;
      me.processDeployerSvc.deploy({
        "disableSchemaValidation": true,
        "disableBpmnValidation": false,
        "tenantId": this.loginUser.current.tenantId,
        "name": me.workflowModeler.processDefinition.name,
        "key": me.workflowModeler.processDefinition.key,
        "enableDuplicateFiltering": true,
        "bpmnXML": me.workflowModeler.processDefinition.xml
      }).then((d) => {
        me.ea.publish("deploiedProcessDefinition", me.workflowModeler.processDefinition);
        alert('已部署');
      }).catch((err) => {
        alert('未知错误');
      });
    });
  }

  remove() {
    this.processDeployerSvc.remove(this.workflowModeler.processDefinition.deploymentId);
    this.ea.publish("removedProcessDefinition", this.workflowModeler.processDefinition);
  }

  save() {
    this.saveDiagram((err, xml) => {
      this.workflowModeler.processDefinition.xml = xml;
      this.workflowModeler.processDefinition.key = this.workflowModeler.process.id;
      this.processDeployerSvc.save({
        "disableSchemaValidation": true,
        "disableBpmnValidation": true,
        "tenantId": this.loginUser.current.tenantId,
        "name": this.workflowModeler.processDefinition.name,
        "key": this.workflowModeler.processDefinition.key,
        "enableDuplicateFiltering": false,
        "bpmnXML": this.workflowModeler.processDefinition.xml
      }).then(() => {
        this.ea.publish("deploied", this.workflowModeler.processDefinition);
        alert('已保存');
      }).catch((err) => {
        alert('未知错误');
      });
    });
  }

  saveDiagram(done) {
    if (!this.workflowModeler.isError()) {
      this.workflowModeler.bpmnModeler.saveXML({ format: true }, (err, xml) => {
        this.workflowModeler.processDefinition.xml = xml;
        done(err, xml);
      });
    } else if (this.workflowModeler.processDefinition != null) {
      done("error", this.workflowModeler.processDefinition.xml);
    }

    let xml = this.workflowModeler.processDefinition == null ? "" : this.workflowModeler.processDefinition.xml;
    this.editor.setValue(xml || "");
    this.editor.clearSelection();
    this.editor.scrollToLine(0);

    this.ea.publish("savedProcessDefinition");
  }

  updateBpmnXml() {
    this.workflowModeler.processDefinition.xml = this.editor.getValue();
    this.openDiagram(this.workflowModeler.processDefinition.xml);
  }

  saveAsImage() {
    this.workflowModeler.bpmnModeler.saveSVG((err, svg) => {
      this.setEncoded('diagram.svg', err ? null : svg);
    })
  }

  setEncoded(name, data) {
    var encodedData = encodeURIComponent(data);

    if (data) {
      var anch = $('<a></a>').attr({
        'href': 'data:application/bpmn20-xml;charset=UTF-8,' + encodedData,
        'download': name
      });
      anch[0].click();
      anch.remove();
    }
  }

  downloadBpmXml() {
    if (!this.workflowModeler.isError()) {
      this.workflowModeler.bpmnModeler.saveXML({ format: true }, (err, xml) => {
        this.setEncoded('diagram.bpmn', err ? null : xml);
      });
    }
  }

  showBpmnXml() {
    this.tabBar.select(1);
  }

  setupWorkflowParameter() {
    WorkflowParameterComponent.setupWorkflowParameter(this.aurelia);
  }
}
