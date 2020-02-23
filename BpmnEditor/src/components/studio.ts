import { ActivitiDebugger } from "../services/ActivitiDebugger";
import { Router } from 'aurelia-router';

import $ from 'jquery';
import { LoginUser } from 'model/loginuser';
import { autoinject } from 'aurelia-framework';
import { EventAggregator } from 'aurelia-event-aggregator';

@autoinject()
export class Studio {

  constructor(private loginUser: LoginUser,
    private router: Router,
    private ea: EventAggregator,
    private wfDebug: ActivitiDebugger) {
  }

  attached() {
    this.wfDebug.connect();
  }

  detached() {
    this.wfDebug.stop();
  }

  launchFullScreen($event) {
    if (!$("body").hasClass("full-screen")) {

      $("body").addClass("full-screen");

      var element: any = document.documentElement;

      if (element.requestFullscreen) {
        element.requestFullscreen();
      } else if (element.mozRequestFullScreen) {
        element.mozRequestFullScreen();
      } else if (element.webkitRequestFullscreen) {
        element.webkitRequestFullscreen();
      } else if (element.msRequestFullscreen) {
        element.msRequestFullscreen();
      }
    } else {

      try {
        $("body").removeClass("full-screen");

        var doc: any = document;

        if (doc.exitFullscreen) {
          doc.exitFullscreen();
        } else if (doc.mozCancelFullScreen) {
          doc.mozCancelFullScreen();
        } else if (doc.webkitExitFullscreen) {
          doc.webkitExitFullscreen();
        }
      } catch (e) { }
    }
  }

  async signout() {
    await this.loginUser.signout();

    setTimeout(() => {
      window.location.href = "#/login";

      window.location.reload(true);
    });
  }

  collapse() {
    this.ea.publish("collapse");
  }
}
