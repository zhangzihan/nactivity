import { Subject, Observable, Subscription } from "rxjs";
import { EventAggregator } from "aurelia-event-aggregator";
import { LoginUser } from "../model/loginuser";
import { singleton } from "aurelia-framework";
import * as signalR from "@aspnet/signalr";
import { Settings } from "model/settings";
import _ from 'lodash';

@singleton()
export class ActivitiDebugger {

  private connection: signalR.HubConnection;

  private subject: Subject<any>;

  private refreshInterval;

  constructor(private settings: Settings,
    private loginUser: LoginUser,
    private ea: EventAggregator) {
    this.subject = new Subject<any>();
  }

  async connect() {
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(this.settings.getUrl('debugger'), {
        transport: signalR.HttpTransportType.LongPolling,
        accessTokenFactory: () => {
          return encodeURIComponent(JSON.stringify(this.loginUser.current));
        }
      })
      .configureLogging(signalR.LogLevel.Trace)
      .build();

    this.connection.on("loggerReceived", (log) => this.onReceivedLogger(log));
    
    let connect = _.throttle(async () => {
      try {
        if (this.connection.state != signalR.HubConnectionState.Connected) {
          await this.connection.start();
        }
      } finally {
        this.refreshInterval = setTimeout(connect, 100);
      }
    }, 100, { trailing: false, leading: true });

    this.refreshInterval = setTimeout(connect);
  }

  async stop() {
    clearTimeout(this.refreshInterval);

    if (this.connection != null && this.connection.state == signalR.HubConnectionState.Connected) {
      await this.connection.stop();
    }
  }

  subscribe(next, error?): Subscription {
    return this.subject.subscribe(next, error);
  }

  private onReceivedLogger(logger) {
    this.subject.next(logger);
  }
}

export interface DebugLogger {
  activityId: string,
  executionTrace: string,
  error: string,
  message: string,
  logLevel: string
}
