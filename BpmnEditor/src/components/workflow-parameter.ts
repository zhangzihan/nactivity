import { PLATFORM } from "aurelia-pal";
import { Dialog } from "@syncfusion/ej2-popups";
import { EventAggregator } from "aurelia-event-aggregator";
import { Aurelia, NewInstance, useView, autoinject } from "aurelia-framework";
import { DialogService, DialogController } from 'aurelia-dialog';
import { Grid, Edit, Filter, Sort, Resize, getComplexFieldID, DialogEditEventArgs } from '@syncfusion/ej2-grids';
import { LocalParameterStore } from "model/storage/localparameterstore";

import { Observable, Subject } from 'rxjs';
import { workflowParameterDescriptor } from "model/entity/WorkflowParameter";
import { WorkflowModeler } from './../model/entity/WorkflowModeler';

let editTmpl = require('./debugger/editTemplate.tmpl').default.replace(/\r\n/g, '');

declare var ace: any;

Grid.Inject(Edit, Filter, Sort, Resize);

@autoinject()
export class WorkflowParameterComponent {

  private dlgWorkflowParameter: Dialog;

  private dgWorkflowParameter: Grid;

  private result: Subject<any>;
  flowElements: any;

  constructor(private dlgService: DialogService,
    private ea: EventAggregator,
    private localParamStore: LocalParameterStore,
    private workflowModeler: WorkflowModeler) {
  }

  attached() {
    this.createWorkflowParameterDialog();

    this.createWorkflowDataGrid();
  }

  detached() {
    this.dgWorkflowParameter.destroy();
    this.dlgWorkflowParameter.destroy();
  }

  private createWorkflowDataGrid() {
    this.dgWorkflowParameter = new Grid({
      allowFiltering: true,
      allowResizing: true,
      allowSorting: true,
      filterSettings: { type: 'Menu' },
      editSettings: {
        allowAdding: false,
        allowDeleting: false,
        allowEditing: true,
        mode: 'Dialog',
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
        allowEditing: false,
        defaultValue: ""
      }, {
        headerText: "参数",
        field: "parameterValue",
        allowEditing: true,
        defaultValue: ""
      }
      ],
      height: 280,
      rowSelected: (e) => {
        this.ea.publish("selectedElement", e.data);
      },
      actionComplete: (args: DialogEditEventArgs) => {
        this.onEditParameter(args);
      }
    });

    this.dgWorkflowParameter.appendTo("#dgWorkflowParameter");
  }

  private parameterEditor;

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
    if (args.requestType == 'cancel') {
      this.parameterEditor.destroy();
    }
    if (args.requestType == 'save') {
      this.parameterEditor.destroy();
    }
  }

  private createWorkflowParameterDialog() {
    this.dlgWorkflowParameter = new Dialog({
      showCloseIcon: true,
      allowDragging: true,
      enableResize: false,
      header: "工作流参数设置",
      isModal: false,
      height: 480,
      width: 800,
      close: (e) => {
        this.result.next();
        this.dlgService.closeAll();
      }
    });

    this.dlgWorkflowParameter.appendTo("#dlgWorkflowParameter");
  }

  static setupWorkflowParameter(aurelia: Aurelia): Observable<any> {
    var dlg: any = document.querySelector("#dgWorkflowParameter");
    if (dlg && dlg.ej2_instances != null) {
      return;
    }

    if (document.querySelector("#editTemplate") == null) {
      var editTemplate = document.createElement('script');
      editTemplate.id = "editTemplate";
      editTemplate.type = 'text/x-template';
      editTemplate.innerText = editTmpl;
      editTemplate.innerHTML = editTemplate.innerHTML.replace(/<br>/g, "");
      document.body.appendChild(editTemplate);
    }

    var instance = NewInstance.of(WorkflowParameterComponent).get(aurelia.container);
    return instance.show();
  }

  ok() {

    let params: any = {};

    this.dgWorkflowParameter.getRowsObject().forEach(row => {
      params[row.data["id"]] = {
        parameterValue: row.data["parameterValue"]
      };
    });

    this.localParamStore.save(this.workflowModeler.process.id, params);

    this.dlgWorkflowParameter.close();
  }

  erase() {
    this.parameterEditor.setValue("");
  }

  cancel() {
    this.dlgWorkflowParameter.close();
  }

  show(): Observable<any> {
    var me = this;
    me.result = new Subject();
    me.dlgService.open({
      viewModel: me
    }).then(r => {
      this.flowElements = JSON.parse(JSON.stringify(this.workflowModeler.flowElements));
      let params = this.localParamStore.getParameters(this.workflowModeler.process.id) || {};
      this.flowElements.forEach(ele => {
        ele.type = ele.$type;
        var value = (params[ele.id] || { parameterValue: null }).parameterValue;
        ele.parameterValue = value;
      });
      me.dgWorkflowParameter.dataSource = this.flowElements;
      me.dgWorkflowParameter.dataBind();
      me.dlgWorkflowParameter.show();
    });

    return me.result;
  }
}
