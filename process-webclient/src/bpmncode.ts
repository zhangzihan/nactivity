import { inject } from "aurelia-framework";
import { DialogController } from "aurelia-dialog";


@inject(DialogController)
export class BpmnCode {

    constructor(private dc: DialogController) {

    }

    code

    activate(code, nctx) {
        this.code = code;
    }

    update(){
        this.dc.ok(this.code);
    }

    close() {
        this.dc.close(false);
    }
}