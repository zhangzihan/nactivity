import { Observable, Subject, AsyncSubject, of } from 'rxjs';
import { Dialog } from '@syncfusion/ej2-popups';
import { singleton, observable } from 'aurelia-framework';

var shortId = require('shortid');

@singleton()
export class CreateWorkflowDialog {

  private dlg: Dialog;

  constructor() {

  }

  open(): Subject<any> {
    if (this.dlg == null) {
      this.createDialog();

      this.dlg.on("close", () => {
        input.value = "";
        sub.next(false);
      });
      var input = this.dlg.element.querySelector("input");

      this.dlg.element.querySelector("[data-command=ok]").addEventListener("click", () => {
        var value = input.value.trim();
        if (value == "") {
          input.setAttribute("placeholder", "必须输入流程名称");
        } else {
          sub.next(value);
          this.dlg.hide();
        }
      });
    }

    var sub = new Subject<any>();

    this.dlg.show();

    return sub;
  }

  private createDialog() {
    this.dlg = new Dialog({
      header: "新建流程",
      allowDragging: true,
      enableResize: true,
      showCloseIcon: true,
      isModal: true,
      width: 400,
      height: 200,
      content: `<div><table class="table">
      <tr>
        <td>流程名称</td>
        <td>
          <label class="input"> 
            <input type="text" class="e-input" style="width:240px" />
          </label>
          <i class="fa fa-cogs" style="color:red"></i>
        </td></tr>
      <tr><td colspan="2" style="text-align:center">
      <button data-command="ok" class="e-btn">确认</button></td></tr></table></div>`
    });

    var div: HTMLDivElement = document.createElement("div");
    var id = 'D' + shortId().replace(/-/g, "");
    div.setAttribute("id", id);
    document.body.appendChild(div);

    this.dlg.appendTo("#" + id);
  }
}
