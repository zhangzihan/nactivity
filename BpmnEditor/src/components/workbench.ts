import { CreateWorkflowDialog } from './createworkflowDialog';
import { ProcessDefinitionDeployerService } from "../services/ProcessDefinitionDeployerService";
import { ProcessDefineService } from "../services/processdefineservice";
import { HttpInvoker } from "../services/httpInvoker";
import { inject } from "aurelia-dependency-injection";
import { EventAggregator } from "aurelia-event-aggregator";

import { TreeView, Toolbar, Menu } from '@syncfusion/ej2-navigations';

import * as uuid from 'uuid'
import { autoinject } from "aurelia-framework";

import $ from 'jquery';
import { DirectionEnum } from "../model/query/ISort";
import contants from "../contants";
import { DropDownList, highlightSearch } from '@syncfusion/ej2-dropdowns';
import { ProcessDefinition } from "../model/entity/ProcessDefinition";
import { ProcessDefinitionDeployer } from "../model/entity/ProcessDefinitionDeployer";

@autoinject()
export class Workbench {

  tvWorkflow: TreeView;

  tbToolbar: Toolbar;

  workflows = [{
    id: "root",
    name: "业务流程",
    children: []
  }];

  root = this.workflows[0];

  selected;

  ddlVersion: DropDownList;

  constructor(private ea: EventAggregator,
    private httpInovker: HttpInvoker,
    private processDefineService: ProcessDefineService,
    private processDeplySevice: ProcessDefinitionDeployerService,
    private dlgCreateWorkflow: CreateWorkflowDialog) {
  }


  attached() {
    var me = this;
    this.tvWorkflow = new TreeView({
      fields: { dataSource: this.workflows, id: "id", text: "name", child: "children" },
      nodeSelected: function (args) {
        me.onSelectedWorkflow(args.nodeData.id);
      }
    });
    this.tvWorkflow.appendTo("#tvWorkflow");

    this.tbToolbar = new Toolbar();
    this.tbToolbar.appendTo("#tbToolbar");

    this.ddlVersion = new DropDownList({
      placeholder: '版本选择',
      fields: { text: "version", value: "id" },
      change: function (e) {
        me.onSelectedDefinitionVersion(e);
      },
      width: 100
    });
    this.ddlVersion.appendTo("#ddlVersion");
    this.reload();

    this.ea.subscribe("deploiedProcessDefinition", () => {
      this.selected.processDefinitions = [];
      this.onSelectedWorkflow(this.selected.id);
    });

    this.ea.subscribe("removedProcessDefinition", (definition) => {
      this.onRemovedProcessDefinition(definition);
    });

    this.ea.subscribe("collapse", (sender) => {
      var hidden = document.body.classList.contains("hidden-menu");
      if (hidden) {
        document.body.classList.remove("hidden-menu");
      } else {
        document.body.classList.add("hidden-menu");
      }
    });
  }

  private onRemovedProcessDefinition(definition) {
    var me = this;
    var idx = (this.selected.processDefinitions || []).findIndex(x => x.id == definition.id);
    if (idx > -1) {
      me.selected.processDefinitions.splice(idx, 1);
      if (me.selected.processDefinitions.length == 0) {
        this.refreshVersion([], null);
        me.tvWorkflow.removeNodes([this.selected.id]);
        idx = me.workflows[0].children.findIndex(x => x.id == this.selected.id);
        me.workflows[0].children.splice(idx, 1);
        me.onSelectedWorkflow(null);
      } else {
        me.refreshVersion(this.selected.processDefinitions);
        me.ddlVersion.notify("select", { itemData: me.selected.processDefinitions[0] });
      }
    }
  }

  private onSelectedDefinitionVersion(e?: any) {
    this.ea.publish("selectedProcessDefinitionVersion", (e || { itemData: null }).itemData);
  }

  private async onSelectedWorkflow(id) {
    var me = this;
    this.selected = this.root.children.find(x => x.id == id);
    var args = { itemData: null };
    var defs = [];
    if (this.selected) {
      defs = this.selected.processDefinitions || [];
      if (defs.length == 0 || defs[0].version != 0) {
        defs = (await this.processDefineService.processDefinitions({
          name: this.selected.name
        })).list.sort((x, y) => x.version - y.version);

        this.selected.processDefinitions = defs;
      }
      args.itemData = defs[defs.length - 1];
    }

    this.refreshVersion(defs, defs.length);
    this.onSelectedDefinitionVersion(args);
  }

  createWorkflow() {

    this.dlgCreateWorkflow.open().subscribe(async (name) => {
      if (name === false) {
        return;
      }

      let dep = await ProcessDefinitionDeployer.create({ name: name });

      let def: any = await ProcessDefinition.create({ name: name });
      def.deploymentId = dep.id;

      dep.processDefinitions.push(def);

      this.workflows[0].children.splice(0, 0, dep);
      this.tvWorkflow.fields.dataSource = this.workflows;
      this.tvWorkflow.selectedNodes = [dep.id];
      this.tvWorkflow.expandedNodes = [this.workflows[0].id];
      this.tvWorkflow.refresh();
      this.selected = dep;

      this.refreshVersion([def], 1);
      this.onSelectedDefinitionVersion({ itemData: def });
    });
  }

  private refreshVersion(defs, index: number = null) {
    let ds = JSON.parse(JSON.stringify(defs));
    if (this.selected) {
      ds.splice(0, 0, { id: "-1", version: "草稿", name: this.selected.name });
    }
    this.ddlVersion.dataSource = ds;
    this.ddlVersion.index = -1;
    this.ddlVersion.index = index == null ? ds.length - 1 : index;
    this.ddlVersion.dataBind();
    this.ddlVersion.refresh();
  }

  dispatch(event) {
    this.ea.publish(event);
  }

  copy() {
    this.ea.publish("copyWorkflow");
  }

  reload() {
    this.processDeplySevice.latest({
      pageable: {
        pageNo: 1,
        pageSize: 1000
      }
    }).then(data => {
      let children: Array<any> = this.root.children;
      children.splice(0, children.length);
      children = children.concat(data.list);
      this.root.children = children;
      this.tvWorkflow.fields.dataSource = this.workflows;
      this.tvWorkflow.refresh();
    });
  }
}
