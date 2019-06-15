import { IProcessDefinitionDeployerService } from './services/IProcessDefinitionDeployerService';
import { BpmnCode } from './bpmncode';
import { HttpClient } from 'aurelia-fetch-client';
import { PLATFORM } from 'aurelia-pal';
import { useView, inject } from "aurelia-framework";

import Axios from 'axios'
import $ from 'jquery';
import BpmnModeler from 'bpmn-js/lib/Modeler';
import BpmnModdle from 'bpmn-moddle'

import propertiesPanelModule from 'bpmn-js-properties-panel';
import propertiesProviderModule from 'bpmn-js-properties-panel/lib/provider/camunda';
import * as camundaModdleDescriptor from '../node_modules/camunda-bpmn-moddle/resources/camunda.json';

import {
  debounce
} from 'lodash';
import { EventAggregator } from 'aurelia-event-aggregator';
import constants from './contants';
import { DialogService, DialogController } from 'aurelia-dialog';
import { EventBus } from 'EventBus';
import { HttpInvoker } from 'services/httpInvoker';
import contants from './contants';
import { IProcessDefinitionDeployer } from 'services/IProcessDefinitionDeployer';

//import diagramXML from '../resources/newDiagram.bpmn';

@inject('eventBus', DialogService, 'httpInvoker', 'processDefinitionDeployer')
export class BpmnIO {

  container;

  canvas;

  bpmnModeler;

  workflow;

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

  constructor(private es: EventBus, private ds: DialogService, private httpInvoker: HttpInvoker, private processDeployerSvc: IProcessDefinitionDeployerService) {
    this.es.subscribe("openWorkflow", (wf) => {
      this.workflow = wf;
      this.openDiagram(wf.xml);
    });
  }

  createNewDiagram(diagramXML) {
    this.openDiagram(diagramXML);
  }

  openDiagram(xml) {
    this.bpmnModeler.importXML(xml, (err) => {
      if (err) {
        this.container
          .removeClass('with-diagram')
          .addClass('with-error');

        this.container.find('.error pre').text(err.message);

        console.error(err);
      } else {
        this.container
          .removeClass('with-error')
          .addClass('with-diagram');
      }

      window["definitions"] = this.definitions = this.bpmnModeler.getDefinitions()
      window["modeling"] = this.modeling = this.bpmnModeler.injector.get('modeling')
      window["moddle"] = this.moddle = this.bpmnModeler.injector.get('moddle')
      window["registry"] = this.registry = this.bpmnModeler.injector.get("elementRegistry")
    });
  }

  saveSVG(done) {
    this.bpmnModeler.saveSVG(done);
  }

  saveDiagram(done) {
    this.bpmnModeler.saveXML({ format: true }, (err, xml) => {
      done(err, xml);
    });
  }

  registerFileDrop(container, callback) {

    function handleFileSelect(e) {
      e.stopPropagation();
      e.preventDefault();

      var files = e.dataTransfer.files;

      var file = files[0];

      var reader = new FileReader();

      reader.onload = function (e: any) {

        var xml = e.target.result;

        callback(xml);
      };

      reader.readAsText(file);
    }

    function handleDragOver(e) {
      e.stopPropagation();
      e.preventDefault();

      e.dataTransfer.dropEffect = 'copy'; // Explicitly show this is a copy.
    }

    container.get(0).addEventListener('dragover', handleDragOver, false);
    container.get(0).addEventListener('drop', handleFileSelect, false);
  }

  attached() {

    window["bpmnViewModel"] = this;

    window["bpmContainer"] = this.container = $('#js-drop-zone');

    window["bpmCanvas"] = this.canvas = $('#js-canvas');

    window["bpmModeler"] = this.bpmnModeler = new BpmnModeler({
      container: this.canvas,
      propertiesPanel: {
        parent: '#js-properties-panel'
      },
      additionalModules: [
        propertiesPanelModule,
        propertiesProviderModule
      ],
      moddleExtensions: {
        camunda: camundaModdleDescriptor
      }
    });

    //取到画布事件总线
    window["eventBus"] = this.eventBus = this.bpmnModeler.get('eventBus')
    //this.eventBus = this.bpmnModeler.get("eventBus");

    this.eventBus.on("element.click", (sender, /**元素节点 */elem: any) => {
      //数据
      var bizobj = elem.element.businessObject;
      this.element = elem.element;
      this.node = bizobj;
      this.gfx = document.querySelector(`[data-element-id=${bizobj.id}]`);

      /*      
      //元素库
      var elementRegistry = this.bpmnModeler.injector.get("elementRegistry");

      //查找单个元素
      var element = elementRegistry.get("Process_0"),
        elementObj = element.businessObject;

      var moddle = this.bpmnModeler.injector.get("moddle");
      //创建属性对象 moddle.create("bpmn:XXXX", {property:value});
      var extElem = moddle.create("bpmn:ExtensionElements", {
        values: []
      });
      extElem.$parent = elementObj;

      //创建属性数据源
      var extProperties = moddle.create("camunda:Properties", {
        values: []
      });
      extProperties.$parent = extElem;
      var extProperty = moddle.create("bpmn:Property", {
        name: '关联会议',
        value: '31用户大会'
      });
      extProperty.value = '31用户大会';
      extProperty.$parent = extProperties
      extProperties.values.push(extProperty);

      extElem.values.push(extProperties);

      //modeling
      var modeling = this.bpmnModeler.injector.get("modeling");
      //更新属性
      modeling.updateProperties(element, { extensionElements: extElem });

      //更新label
      modeling.updateLabel(elementRegistry.get("SequenceFlow_0ihgz1d"), "流程引擎");
      */
    });

    ////// file drag / drop ///////////////////////

    // check file api availability
    // if (!window.FileList || !window.FileReader) {
    //   window.alert(
    //     'Looks like you use an older browser that does not support drag and drop. ' +
    //     'Try using Chrome, Firefox or the Internet Explorer > 10.');
    // } else {
    //   registerFileDrop(container, openDiagram);
    // }

    // bootstrap diagram functions

    //this.createNewDiagram();
  }

  addError() {
    if (this.node.$type != 'bpmn:BoundaryEvent') {
      return;
    }

    var errNode = this.moddle.get("BoundaryEvent_0265s8h");

    // 创建节点方法
    var createBpmnNode = (node, parent, v) => {
      const extElem = this.moddle.create(node, v)
      extElem.$parent = parent
      return extElem
    }

    // 点击元素切换节点对象
    // this.eventBus.on('element.click', (sender, elem) => {
    //   this.element = elem.element
    //   this.node = elem.element.businessObject
    // })

    // 更新当前元素上的错误属性
    const errorEvent = createBpmnNode('bpmn:ErrorEventDefinition', this.node.eventDefinitions, {
      errorCode: "错误代码1",
      id: "Error_0ar2oln",
      name: "错误1",
    })
    this.node.eventDefinitions = [errorEvent]

    // 在根节点上添加错误节点
    const error = createBpmnNode('bpmn:Error', this.definitions, {
      errorCodeVariable: "错误代码变量2",
      errorMessageVariable: "错误代码消息2",
    })

    this.definitions.rootElements.push(error)
    this.modeling.updateProperties(this.element, { eventDefinitions: this.node.eventDefinitions })
  }

  deploy($event) {
    this.saveDiagram((err, xml) => {
      this.workflow.xml = xml;
      this.processDeployerSvc.deploy({
          "disableSchemaValidation": true,
          "disableBpmnValidation": false,
          "tenantId": contants.tenantId,
          "name": this.workflow.name,
          "key": this.workflow.id,
          "enableDuplicateFiltering": true,
          "bpmnXML": this.workflow.xml
        }).then(() => {
          this.es.publish("deploied", this.workflow);
          alert('已部署');
        }).catch(() => {
          alert('未知错误');
        });
    });
  }

  copy($event) {
    var me = this;
    this.saveDiagram((err, xml) => {
      var moddle = new BpmnModdle();

      var model = moddle.fromXML(xml, (err, definitions) => {
        moddle.toXML(definitions, (err, xml) => {
          debugger;
        })
        debugger;
      })
    });
  }

  saveBpmn($event) {
    this.saveDiagram((err, xml) => {
      //this.setEncoded('diagram.bpmn', err ? null : xml);      
      this.workflow.xml = xml;
      this.processDeployerSvc.save({
        "disableSchemaValidation": true,
        "disableBpmnValidation": true,
        "tenantId": contants.tenantId,
        "name": this.workflow.name,
        "key": this.workflow.id,
        "enableDuplicateFiltering": false,
        "bpmnXML": this.workflow.xml
      }).then(() => {
        this.es.publish("deploied", this.workflow);
        alert('已部署');
      }).catch(() => {
        alert('未知错误');
      });
    });
  }

  showBpmnXml($event) {
    this.saveDiagram((err, xml) => {
      this.ds.open({
        viewModel: BpmnCode,
        model: xml
      }).whenClosed(res => {
        if (res.wasCancelled == false && res.output && res.output.trim() != "") {
          this.openDiagram(res.output);
        }
      });
    });
  }

  setEncoded(name, data) {
    var encodedData = encodeURIComponent(data);
    var link = $('<a></a>');

    link.addClass('active').attr({
      'href': 'data:application/bpmn20-xml;charset=UTF-8,' + encodedData,
      'download': name
    });

    link[0].click();
    link.remove();
  }
}
