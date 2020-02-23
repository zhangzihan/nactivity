import { Subject } from "rxjs";
import { ProcessInstanceServie } from './../../services/ProcessInstanceService';
import { FormTemplate } from 'model/form/formtemplate';
//import { Grid, Edit, Filter, Sort, Resize, DialogEditEventArgs, CommandColumn } from '@syncfusion/ej2-grids';
import { autoinject, observable } from "aurelia-framework";
import { EventAggregator } from 'aurelia-event-aggregator';
import { LocalParameterStore } from 'model/storage/localparameterstore';
import { workflowParameterDescriptor, WorkflowParameter } from 'model/entity/WorkflowParameter';
import { WorkflowModeler } from './../../model/entity/WorkflowModeler';
import { FormField } from 'model/form/formfield';
import { Animation } from '@syncfusion/ej2-base';

import { TreeGrid, Edit, Filter, Sort, Resize, CommandColumn } from '@syncfusion/ej2-treegrid';
import { DialogEditEventArgs } from '@syncfusion/ej2-grids';
import { ActivitiDebugger, DebugLogger } from 'services/ActivitiDebugger';

let editTmpl = require('./editTemplate.tmpl').default;

TreeGrid.Inject(Edit, Filter, Sort, Resize, CommandColumn)

declare let ace: any;

@autoinject()
export class WorkflowDebugger {

  private dgWorkflowParameter: TreeGrid;

  flowElements: any;
  isShowTasks: boolean;

  loggers = [];

  constructor(private ea: EventAggregator,
    private localParamStore: LocalParameterStore,
    private formTemplate: FormTemplate,
    private workflowModeler: WorkflowModeler,
    private processInstanceSvc: ProcessInstanceServie) {
  }

  attached() {
    this.initTemplate();

    this.createWorkflowDataGrid();

    //this.createParameterEditor();

    this.subscribeEvent();
  }

  private subscribeEvent() {
    this.ea.subscribe("savedProcessDefinition", () => {
      this.reload();
    });
  }

  private reload() {
    if (this.workflowModeler.isError()) {
      this.flowElements = [];
    } else {
      this.flowElements = JSON.parse(JSON.stringify(this.workflowModeler.flowElements));
      let params = this.localParamStore.getParameters(this.workflowModeler.process.id) || {};
      this.flowElements.forEach(ele => {
        ele.type = ele.$type;
        var value = (params[ele.id] || { parameterValue: null }).parameterValue;
        ele.parameterValue = value;
      });
    }

    this.dgWorkflowParameter.dataSource = this.flowElements;
    this.dgWorkflowParameter.dataBind();
  }

  private createWorkflowDataGrid() {
    this.dgWorkflowParameter = new TreeGrid({
      allowFiltering: true,
      allowResizing: true,
      allowSorting: true,
      childMapping: "flowElements",
      filterSettings: { type: 'Menu' },
      editSettings: {
        allowAdding: false,
        allowDeleting: false,
        allowEditing: true,
        mode: "Dialog",
        template: "#editTemplate"
      },
      columns: [{
        headerText: "ID",
        field: "id",
        allowEditing: false,
        isPrimaryKey: true
      }, {
        headerText: "类型",
        field: "type",
        allowEditing: false
      }, {
        headerText: "名称",
        field: "name",
        allowEditing: false
      }, {
        headerText: "参数",
        field: "parameterValue",
        allowEditing: true,
        defaultValue: ""
      }, {
        headerText: '',
        commands: [{
          type: 'Edit',
          buttonOption: {
            cssClass: 'e-flat', iconCss: 'e-edit e-icons'
          }
        }]
      }],
      rowSelected: (e) => {
        this.ea.publish("selectedElement", e.data);
        //this.onSelectedRow(e);
      },
      actionComplete: (args: DialogEditEventArgs) => {
        this.onEditParameter(args);
      }
    });

    this.dgWorkflowParameter.appendTo("#dgWorkflowParameter");
  }

  private initTemplate() {

    if (document.querySelector("#editTemplate") == null) {
      var editTemplate = document.createElement('script');
      editTemplate.id = "editTemplate";
      editTemplate.type = 'text/x-template';
      editTemplate.innerText = editTmpl;
      editTemplate.innerHTML = editTemplate.innerHTML.replace(/<br>/g, "");
      document.body.appendChild(editTemplate);
    }
  }

  private parameterEditor;

  // private onSelectedRow(e) {
  //   let value = e.data["parameterValue"] || workflowParameterDescriptor;
  //   this.parameterEditor.setValue(value);
  //   this.parameterEditor.clearSelection();
  //   this.parameterEditor.scrollToLine(0);
  // }

  // private createParameterEditor() {
  //   this.parameterEditor = ace.edit("paramEditor");
  //   this.parameterEditor.setTheme("ace/theme/textmate");
  //   this.parameterEditor.getSession().setMode("ace/mode/json");
  //   this.parameterEditor.getSession().setUseWrapMode(true);
  // }

  private saveParameter() {
    let params: any = {};

    // this.dgWorkflowParameter.getRowsObject().forEach(row => {
    //   params[row.data["id"]] = {
    //     parameterValue: row.data["parameterValue"]
    //   };
    // });

    this.dgWorkflowParameter.getCurrentViewRecords().forEach(row => {
      params[row["id"]] = {
        parameterValue: row["parameterValue"]
      };
    });

    this.localParamStore.save(this.workflowModeler.process.id, params);
  }

  private onEditParameter(args: DialogEditEventArgs) {
    if (args.requestType === 'beginEdit') {
      let dlg: any = args.dialog;
      dlg.closeOnEscape = false;
      dlg.dataBind();
      let txtInput: any = args.form.elements.namedItem('parameterValue');
      let value = args.rowData["parameterValue"] || workflowParameterDescriptor;
      txtInput.value = value;
      this.parameterEditor = ace.edit("paramEditor");
      this.parameterEditor.setTheme("ace/theme/textmate");
      this.parameterEditor.getSession().setMode("ace/mode/json");
      this.parameterEditor.getSession().setUseWrapMode(true);
      this.parameterEditor.setValue(value);
      this.parameterEditor.clearSelection();
      this.parameterEditor.scrollToLine(0);
      this.parameterEditor.on("change", (sender) => {
        txtInput.value = this.parameterEditor.getValue();
      });
      document.querySelector("#btnHelp").addEventListener("click", () => {
        (document.querySelector(".parameter-help") as HTMLElement).classList.add("show-help");
      });
      document.querySelector("#btnHelpClose").addEventListener("click", () => {
        (document.querySelector(".parameter-help") as HTMLElement).classList.remove("show-help");
      });
      (document.querySelector(".parameter-help .content") as HTMLElement).innerText = workflowParameterDescriptor
    }
  }

  private cancelEdit() {
    let data: any = (this.dgWorkflowParameter.getSelectedRecords() || [])[0];
    let value = data.parameterValue || workflowParameterDescriptor;
    this.parameterEditor.setValue(value);
  }

  private runWorkflow() {
    var startEvent = this.workflowModeler.flowElements.find(x => x.$type == "bpmn:StartEvent");
    let fields = this.workflowModeler.fields(startEvent);

    if (fields != null) {
      this.formTemplate.openTemplateDialog(fields).subscribe((data) => {
        debugger;
        this.processInstanceSvc.start([{
          processDefinitionId: this.workflowModeler.processDefinition.id,
          businessKey: data.businessKey,
          variables: data.variables
        }]).then(instances => {
          debugger;
        }).catch((err) => {
          alert(err);
        })
      });
    }
  }

  private onSelectedParameter() {
    let tabs = document.querySelectorAll(".alert .e-tab-wrap");
    tabs[0].classList.add("activate");
    tabs[1].classList.remove("activate");

    let list = document.querySelectorAll(".debugger-parameter .list");
    list[0].classList.remove('no-activate');
    list[1].classList.add('no-activate');

    this.isShowTasks = false;
  }

  private onSelectedProcessTasks() {
    let tabs = document.querySelectorAll(".alert .e-tab-wrap");
    tabs[0].classList.remove("activate");
    tabs[1].classList.add("activate");

    let list = document.querySelectorAll(".debugger-parameter .list");
    list[1].classList.remove('no-activate');
    list[0].classList.add('no-activate');

    this.isShowTasks = true;
    this.ea.publish("reloadProcessTasks");
  }

  private reloadProcessTasks() {
    this.ea.publish("reloadProcessTasks");
  }

  terminateTask() {
    this.ea.publish('terminateTask');
  }

  removeTask() {
    this.ea.publish("removeTask");
  }
}
