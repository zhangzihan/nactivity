import { Observable, Subject } from 'rxjs';
import { Dialog } from '@syncfusion/ej2-popups';
import { singleton } from "aurelia-framework";
import { FormField } from "./formfield";
import { compile } from "@syncfusion/ej2-base";

var shortId = require("shortid");

class StringBuffer {
  buffer: Array<string> = [];

  append(str: string) {
    this.buffer.push(str);

    return this;
  }

  appendLine(str: string) {
    this.append(str).append("\r\n");

    return this;
  }

  toString() {
    return this.buffer.join("");
  }

  clear() {
    this.buffer = [];
  }
}

let stringBuffer = new StringBuffer();

@singleton()
export class FormTemplate {

  generate(formid: string, fields: Array<FormField>, options?: {
    buttons: {
      submit?: {
        id?: string,
        title?: string,
        show?: boolean
      },
      cancel?: {
        id?: string,
        title?: string,
        show?: boolean
      }
    }
  }): string {

    let opts = Object.assign({
      buttons: {
        submit: {
          id: "btnSave",
          title: "提交",
          show: true,
        },
        cancel: {
          id: "btnCancel",
          title: "取消",
          show: true
        }
      }
    }, options);

    stringBuffer.appendLine(`<div id="${formid}">`);

    fields.forEach(f => {
      let value = `\${if(${f.id}==null)}'' \${else} \${${f.id}} \${/if}`;
      stringBuffer.appendLine('<div class="form-row">')
        .appendLine('<div class="form-group col-md-12">')
        .appendLine('<div class="e-float-input e-control-wrapper">')
        .appendLine(`<input id="${f.id}" name="${f.id}" class="e-input" type="text" value="\${${f.id}}" />`)
        .appendLine('<span class="e-float-line"></span>')
        .appendLine(`<label class="e-float-text e-label-top" for="${f.id}">${f.name}-${f.id}-${f.type}</label>`)
        .appendLine('</div></div></div>');
    });

    stringBuffer.appendLine('<div class="form-row">')
      .appendLine('<div class="form-group col-md-12" style="text-align:center;width:100%;paddig:4px">')
      .appendLine(`<button id="${opts.buttons.submit.id}" class="e-btn e-primary" style="${(opts.buttons.submit.show === false ? "display:none" : "")}">${opts.buttons.submit.title}</button></button>`)
      .appendLine(`<button id="${opts.buttons.cancel.id}" class="e-btn" style="margin-left:4px;${(opts.buttons.cancel.show === false ? "display:none" : "")
        } ">${opts.buttons.cancel.title}</button>`)
      .appendLine('</div></div>');

    stringBuffer.appendLine('</div>');

    let tmpl = stringBuffer.toString();
    stringBuffer.clear();

    return tmpl;
  }

  openTemplateDialog(fields: Array<FormField>): Observable<any> {
    let subject = new Subject();

    let dlg = document.createElement('div');
    var id = 'D' + shortId().replace(/-/g, "");
    dlg.id = id;
    var tmpl = this.generate(dlg.id + "_form", fields);

    var obj: any = {};
    fields.forEach(f => {
      obj[f.id] = f.value == null ? "" : JSON.stringify(f.value);
    });

    let form = compile(tmpl)(obj);
    dlg.appendChild(form[0]);
    document.body.appendChild(dlg);

    dlg.querySelector("#btnSave").addEventListener("click", () => {

      let variable: any = this.formData(dlg, fields);
      variable.businessKey = obj.businessKey;

      subject.next(variable);

      dlgForm.hide();
    });

    dlg.querySelector("#btnCancel").addEventListener("click", () => {
      dlgForm.hide();
    });

    let dlgForm = new Dialog({
      header: "表单模板",
      showCloseIcon: true,
      allowDragging: true,
      enableResize: true,
      isModal: true,
      width: 400,
      close: () => {
        setTimeout(() => {
          dlgForm.destroy();
          document.body.removeChild(dlg);
        });
      }
    });
    dlgForm.appendTo("#" + dlg.id);
    dlgForm.show();

    return subject;
  }

  formData(form: HTMLElement, fields) {
    let obj: any = {};
    fields.forEach(f => {
      let input = form.querySelector(`#${f.id}`) as HTMLInputElement;
      let val = input.value.trim();
      switch (f.type) {
        case "array":
          obj[f.id] = val == "" ? [] : JSON.parse(val);
          break;
        case "long":
        case "int":
          obj[f.id] = parseInt(val);
          break;
        case "number":
          obj[f.id] = parseFloat(val);
          break;
        case "datetime":
          obj[f.id] = val == "" ? null : Date.parse(val);
          break;
        case "boolean":
          obj[f.id] = val === "true" ? true : false;
          break;
        case "object":
          obj[f.id] = val == "" ? null : JSON.parse(val);
          break;
        default:
          obj[f.id] = val;
          break;
      }
    });

    var variable = {
      variables: {}
    };

    var keys = Object.keys(obj);
    keys.forEach(prop => {
      if (prop != "businessKey") {
        variable.variables[prop] = obj[prop];
      }
    });

    return variable;
  }
}

