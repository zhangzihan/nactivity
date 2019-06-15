import { EventAggregator } from 'aurelia-event-aggregator';
import { LoginUser } from 'loginuser';
import { inject, observable } from "aurelia-framework";
import { EventBus } from 'EventBus';

@inject('loginUser', 'eventBus')
export class UserList {
  users = [{
    id: "新用户1",
    name: "新用户1"
  }, {
    id: "评审员",
    name: "评审员"
  }, {
    id: "主办方评审员",
    name: "主办方评审员"
  }];

  constructor(private loginUser: LoginUser, private es: EventBus) {

  }

  activate(model, nctx) {
    var me = this;
    this.es.subscribe('registeredUser', (user) => {
      var u = me.users.find(x => x.id == user.current.id);
      if (u) {
        u.name = user.current.name;
      }
    });
  }

  register() {
    let id = this.users.length + 1;
    let user = {
      id: "新用户" + id,
      name: "新用户" + id
    };

    this.loginUser.current = user;

    this.users.push(user);

    this.es.publish('registerUser', user);
  }

  @observable select;

  selectedUser(user) {
    this.select = user;

    this.loginUser.current = user;

    this.es.publish("userLogined", user);
  }
}
