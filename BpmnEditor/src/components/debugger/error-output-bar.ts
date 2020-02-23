import { ActivitiDebugger, DebugLogger } from "./../../services/ActivitiDebugger";
import { autoinject } from "aurelia-framework";

@autoinject()
export class ErrorOutputBar {
  isShowoutput;

  loggers = [];

  constructor(private wfDebug: ActivitiDebugger) {
    this.wfDebug.subscribe(
      (log: DebugLogger) => {
        if (log.error != null) {
          log.error = log.error.replace(/\r\n/, '<br >');
          log.message = log.error;
          this.isShowoutput = true;
        }
        if (log.executionTrace != null) {
          log.executionTrace = log.executionTrace.replace(/\r\n/, '<br >');
          log.message = log.executionTrace;
        }
        this.loggers.splice(0, 0, log);
      },
      err => {
        debugger;
      });
  }

  attached() {

  }

  private showOutput() {
    this.isShowoutput = !this.isShowoutput
  }
}
