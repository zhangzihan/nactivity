import { EventAggregator } from 'aurelia-event-aggregator';
import { LoginUser } from 'loginuser';
import { inject, observable } from "aurelia-framework";

@inject('loginUser', EventAggregator)
export class UserList {
    users = [{
        id: 1,
        name: "新用户1"
    },{
        id: 100,
        name: "评审员"
    },{
        id: 200,
        name: "主办方评审员"
    }];

    constructor(private loginUser: LoginUser, private es: EventAggregator) {

    }

    activate(model, nctx) {
        this.es.subscribe('registeredUser', (user) => {
            var u = this.users.find(x => x.id == user.id);
            u.name = user.name;
        });
    }

    register() {
        let id = this.users.length + 1;
        let user = {
            id: id,
            name: "新用户" + id
        };

        this.loginUser.id = user.id;
        this.loginUser.name = user.name;

        this.users.push(user);

        this.es.publish('registerUser', user);
    }

    @observable select;

    selectedUser(user) {
        this.select = user;
        
        this.loginUser.name = user.name;

        this.es.publish("userLogined", user);
    }
}