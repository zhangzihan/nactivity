import { PLATFORM } from 'aurelia-pal';
import { BaseInfo } from "forms/baseinfo";
import { Student } from "forms/student";
import { Techer } from "forms/techer";

export class FormList {

    forms = [
        {
            id: 1,
            name: '基本信息',
            type: PLATFORM.moduleName('./forms/baseInfo')
        },
        {
            id: 2,
            name: '学生注册',
            type: PLATFORM.moduleName('./forms/student')
        },
        {
            id: 3,
            name: '教师注册',
            type: PLATFORM.moduleName('./forms/techer')
        }
    ]
}