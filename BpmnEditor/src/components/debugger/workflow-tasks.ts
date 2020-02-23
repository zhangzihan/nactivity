import { ITaskService } from "./../../services/ITaskService";
import { compile } from '@syncfusion/ej2-base';
import { LocalParameterStore } from './../../model/storage/localparameterstore';
import { TaskAdminService } from '../../services/TaskAdminService';
import { Tab } from '@syncfusion/ej2-navigations';
import { TaskService } from 'services/TaskService';
import { ProcessInstanceServie } from '../../services/ProcessInstanceService';
import { WorkflowModeler } from '../../model/entity/WorkflowModeler';
import { FormTemplate } from 'model/form/formtemplate';
import { Grid, Sort, Filter, Resize, Page, Selection, RowSelectEventArgs } from '@syncfusion/ej2-grids';
import { EventAggregator } from 'aurelia-event-aggregator';
import { autoinject } from 'aurelia-framework';
import { DirectionEnum } from 'model/query/ISort';
import { WorkflowParameter } from 'model/entity/WorkflowParameter';
import { FormField } from 'model/form/formfield';

let shortId = require('shortid');

Grid.Inject(Sort, Filter, Resize, Page, Selection)

@autoinject()
export class WorkflowTasks {

  dgTasks: Grid

  tabBar: Tab;

  private formContent: HTMLDivElement;

  constructor(private ea: EventAggregator,
    private formTemplate: FormTemplate,
    private processInstanceSvc: ProcessInstanceServie,
    private taskAdminSvc: TaskAdminService,
    private taskSvc: TaskService,
    private workflowModeler: WorkflowModeler,
    private localParameterStore: LocalParameterStore,
    private taskService: TaskService) {

  }

  attached() {
    this.createMyTasksGrid();

    setTimeout(() => {
      let divs = document.querySelectorAll("#tab-bar>div")
      // divs[0].classList.add("e-tab-header");
      // divs[1].classList.add("e-content");
      // this.createTabbar();
    });

    this.subscribeEvent();
    this.formContent = document.querySelector("#tab-bar .task-form") as HTMLDivElement;
  }

  subscribeEvent() {
    this.ea.subscribe("reloadProcessTasks", () => {
      this.reloadProcessTasks();
    });

    this.ea.subscribe("terminateTask", async () => {
      let tasks = this.dgTasks.getSelectedRecords();
      if (tasks.length > 0) {
        let task: any = tasks[0];
        await this.taskService.terminateTask({
          taskId: task.id,
          terminateReason: "测试终止"
        });

        await this.reloadProcessTasks();
      }
    });

    this.ea.subscribe("removeTask", async () => {
      let tasks = this.dgTasks.getSelectedRecords();
      if (tasks.length > 0) {
        let task: any = tasks[0];
        await this.taskService.deleteTask(task.id);

        await this.reloadProcessTasks();
      }
    });
  }

  private async reloadProcessTasks() {
    if (this.formContent.hasChildNodes()) {
      this.formContent.removeChild(this.formContent.children[0]);
    }
    await this.taskAdminSvc.getAllTasks({
      processDefinitionId: this.workflowModeler.processDefinition.id,
      pageable: {
        pageNo: 1,
        pageSize: 14327880,
        sort: [{
          property: "createTime",
          direction: DirectionEnum.desc
        }]
      }
    }).then((tasks) => {
      this.dgTasks.dataSource = tasks;
      this.dgTasks.dataBind();
    });
  }

  createTabbar() {
    this.tabBar = new Tab({

    });

    this.tabBar.appendTo("#tab-bar");
  }

  private createMyTasksGrid() {
    this.dgTasks = new Grid({
      allowFiltering: true,
      allowResizing: true,
      allowSorting: true,
      filterSettings: { type: 'Menu' },
      columns: [
        {
          headerText: "节点执行者",
          field: "assignee"
        }, {
          headerText: "任务名称",
          field: "name"
        }, {
          headerText: "流程节点",
          field: "taskDefinitionKey"
        }, {
          headerText: "状态",
          field: "status"
        }, {
          headerText: "创建日期",
          field: "createdDate"
        }
      ],
      allowPaging: true,
      pageSettings: {
        pageCount: 5
      },
      height: 300,
      rowSelected: (args) => {
        this.onSelectedRow(args);
      },
      rowSelecting: (args) => {
        let formContent = document.querySelector("#tab-bar .task-form") as HTMLElement;
        formContent.innerHTML = "";
      }
    });

    this.dgTasks.appendTo("#dgTasks");
  }

  private onSelectedRow(args: RowSelectEventArgs) {
    let data: any = args.data;
    let task = this.workflowModeler.find(data.taskDefinitionKey);
    let fields = this.workflowModeler.fields(task);

    let submitId = 'btn' + shortId().replace(/-/g, "");
    let formid = 'frm' + shortId().replace(/-/g, "");

    let tmpl = this.formTemplate.generate(formid, fields, {
      buttons: {
        submit: {
          id: submitId,
          title: "提交"
        },
        cancel: {
          show: false
        }
      }
    });

    var obj: any = {};
    fields.forEach(f => {
      obj[f.id] = f.value == null ? "" : JSON.stringify(f.value);
    });

    let form: any = compile(tmpl)(obj)[0];
    this.formContent.appendChild(form);
    this.formContent.querySelector(`#${submitId}`).addEventListener("click", () => {
      let variable = this.formTemplate.formData(this.formContent, fields);
      this.taskSvc.completeTask({
        taskId: data.id,
        assignee: data.assignee,
        outputVariables: variable.variables
      }).then(x => {
        this.reloadProcessTasks();
        this.formContent.removeChild(form);
      });
    });
  }
}
