import { inject } from "aurelia-framework";
import { EventAggregator } from "aurelia-event-aggregator";

let shortid = require('shortid');
let uuid = require('uuid');

const defBpmn = `<?xml version="1.0" encoding="UTF-8"?>
<bpmn2:definitions xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:bpmn2="http://www.omg.org/spec/BPMN/20100524/MODEL" xmlns:bpmndi="http://www.omg.org/spec/BPMN/20100524/DI" xmlns:dc="http://www.omg.org/spec/DD/20100524/DC" id="sample-diagram" targetNamespace="http://bpmn.io/schema/bpmn" xsi:schemaLocation="http://www.omg.org/spec/BPMN/20100524/MODEL BPMN20.xsd">
  <bpmn2:process id="{0}" isExecutable="true">
    <bpmn2:startEvent id="{1}" />
  </bpmn2:process>
  <bpmndi:BPMNDiagram id="BPMNDiagram_1">
    <bpmndi:BPMNPlane id="BPMNPlane_1" bpmnElement="{0}">
      <bpmndi:BPMNShape id="_BPMNShape_{1}" bpmnElement="{1}">
        <dc:Bounds x="208" y="250" width="36" height="36" />
      </bpmndi:BPMNShape>
    </bpmndi:BPMNPlane>
  </bpmndi:BPMNDiagram>
</bpmn2:definitions>`

@inject(EventAggregator, 'processDefineService')
export class Processes {

  defines = [
    {
      id: 1,
      key: "Process_0",
      name: '会议注册路径',
      businessKey: "normalPath",
      xml: `<?xml version="1.0" encoding="UTF-8"?>
      <bpmn2:definitions xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:bpmn2="http://www.omg.org/spec/BPMN/20100524/MODEL" xmlns:bpmndi="http://www.omg.org/spec/BPMN/20100524/DI" xmlns:dc="http://www.omg.org/spec/DD/20100524/DC" xmlns:di="http://www.omg.org/spec/DD/20100524/DI" xmlns:camunda="http://camunda.org/schema/1.0/bpmn" id="sample-diagram" targetNamespace="http://bpmn.io/schema/bpmn" xsi:schemaLocation="http://www.omg.org/spec/BPMN/20100524/MODEL BPMN20.xsd">
        <bpmn2:process id="Process_0" isExecutable="true">
          <bpmn2:startEvent id="StartEvent_1">
            <bpmn2:outgoing>SequenceFlow_0ihgz1d</bpmn2:outgoing>
          </bpmn2:startEvent>
          <bpmn2:sequenceFlow id="SequenceFlow_0ihgz1d" sourceRef="StartEvent_1" targetRef="Task_085qd6k" />
          <bpmn2:userTask id="Task_0opugm1" name="教师注册" camunda:formKey="3" camunda:assignee="\${name}">
            <bpmn2:incoming>SequenceFlow_1kbdvpj</bpmn2:incoming>
          </bpmn2:userTask>
          <bpmn2:sequenceFlow id="SequenceFlow_1kbdvpj" sourceRef="Task_085qd6k" targetRef="Task_0opugm1" />
          <bpmn2:userTask id="Task_085qd6k" name="学生注册" camunda:formKey="2" camunda:assignee="\${name}" camunda:candidateUsers="&#39;1;2;3&#39;">
            <bpmn2:incoming>SequenceFlow_0ihgz1d</bpmn2:incoming>
            <bpmn2:outgoing>SequenceFlow_1kbdvpj</bpmn2:outgoing>
            <bpmn2:multiInstanceLoopCharacteristics camunda:collection="\${userList}" />
          </bpmn2:userTask>
        </bpmn2:process>
        <bpmndi:BPMNDiagram id="BPMNDiagram_1">
          <bpmndi:BPMNPlane id="BPMNPlane_1" bpmnElement="Process_0">
            <bpmndi:BPMNShape id="_BPMNShape_StartEvent_2" bpmnElement="StartEvent_1">
              <dc:Bounds x="127" y="161" width="36" height="36" />
            </bpmndi:BPMNShape>
            <bpmndi:BPMNEdge id="SequenceFlow_0ihgz1d_di" bpmnElement="SequenceFlow_0ihgz1d">
              <di:waypoint x="163" y="179" />
              <di:waypoint x="338" y="179" />
            </bpmndi:BPMNEdge>
            <bpmndi:BPMNShape id="UserTask_16tu8q7_di" bpmnElement="Task_0opugm1">
              <dc:Bounds x="567" y="139" width="100" height="80" />
            </bpmndi:BPMNShape>
            <bpmndi:BPMNEdge id="SequenceFlow_1kbdvpj_di" bpmnElement="SequenceFlow_1kbdvpj">
              <di:waypoint x="438" y="179" />
              <di:waypoint x="567" y="179" />
            </bpmndi:BPMNEdge>
            <bpmndi:BPMNShape id="UserTask_0ornknv_di" bpmnElement="Task_085qd6k">
              <dc:Bounds x="338" y="139" width="100" height="80" />
            </bpmndi:BPMNShape>
          </bpmndi:BPMNPlane>
        </bpmndi:BPMNDiagram>
      </bpmn2:definitions>`
    },
    {
      id: 2,
      key: "Process_1",
      name: '条件会议注册路径',
      businessKey: "registerPath",
      xml: `<?xml version="1.0" encoding="UTF-8"?>
      <bpmn2:definitions xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:bpmn2="http://www.omg.org/spec/BPMN/20100524/MODEL" xmlns:bpmndi="http://www.omg.org/spec/BPMN/20100524/DI" xmlns:dc="http://www.omg.org/spec/DD/20100524/DC" xmlns:di="http://www.omg.org/spec/DD/20100524/DI" xmlns:camunda="http://camunda.org/schema/1.0/bpmn" id="sample-diagram" targetNamespace="http://bpmn.io/schema/bpmn" xsi:schemaLocation="http://www.omg.org/spec/BPMN/20100524/MODEL BPMN20.xsd">
        <bpmn2:process id="Process_1" name="条件会议注册路径" isExecutable="true">
          <bpmn2:startEvent id="StartEvent_1">
            <bpmn2:outgoing>SequenceFlow_0ihgz1d</bpmn2:outgoing>
          </bpmn2:startEvent>
          <bpmn2:sequenceFlow id="SequenceFlow_0ihgz1d" name="从基本信息提交的数据判断会议注册人是否为教师" sourceRef="StartEvent_1" targetRef="ExclusiveGateway_1y6gcmn" />
          <bpmn2:userTask id="Task_0opugm1" name="教师注册" camunda:formKey="3" camunda:assignee="\${name}">
            <bpmn2:incoming>SequenceFlow_1gm6tlc</bpmn2:incoming>
          </bpmn2:userTask>
          <bpmn2:userTask id="Task_085qd6k" name="学生注册" camunda:formKey="2" camunda:assignee="\${name}">
            <bpmn2:incoming>SequenceFlow_0uc2k2q</bpmn2:incoming>
          </bpmn2:userTask>
          <bpmn2:exclusiveGateway id="ExclusiveGateway_1y6gcmn">
            <bpmn2:incoming>SequenceFlow_0ihgz1d</bpmn2:incoming>
            <bpmn2:outgoing>SequenceFlow_1gm6tlc</bpmn2:outgoing>
            <bpmn2:outgoing>SequenceFlow_0uc2k2q</bpmn2:outgoing>
          </bpmn2:exclusiveGateway>
          <bpmn2:sequenceFlow id="SequenceFlow_1gm6tlc" name="如果是教师跳转教师注册" sourceRef="ExclusiveGateway_1y6gcmn" targetRef="Task_0opugm1">
            <bpmn2:conditionExpression xsi:type="bpmn2:tFormalExpression">\${isTecher==true}</bpmn2:conditionExpression>
          </bpmn2:sequenceFlow>
          <bpmn2:sequenceFlow id="SequenceFlow_0uc2k2q" name="如果不是教师跳转学生注册" sourceRef="ExclusiveGateway_1y6gcmn" targetRef="Task_085qd6k">
            <bpmn2:conditionExpression xsi:type="bpmn2:tFormalExpression">\${isTecher==false}</bpmn2:conditionExpression>
          </bpmn2:sequenceFlow>
        </bpmn2:process>
        <bpmndi:BPMNDiagram id="BPMNDiagram_1">
          <bpmndi:BPMNPlane id="BPMNPlane_1" bpmnElement="Process_1">
            <bpmndi:BPMNShape id="_BPMNShape_StartEvent_2" bpmnElement="StartEvent_1">
              <dc:Bounds x="127" y="161" width="36" height="36" />
            </bpmndi:BPMNShape>
            <bpmndi:BPMNEdge id="SequenceFlow_0ihgz1d_di" bpmnElement="SequenceFlow_0ihgz1d">
              <di:waypoint x="163" y="179" />
              <di:waypoint x="308" y="179" />
              <bpmndi:BPMNLabel>
                <dc:Bounds x="190" y="136" width="88" height="40" />
              </bpmndi:BPMNLabel>
            </bpmndi:BPMNEdge>
            <bpmndi:BPMNShape id="UserTask_16tu8q7_di" bpmnElement="Task_0opugm1">
              <dc:Bounds x="535" y="245" width="100" height="80" />
            </bpmndi:BPMNShape>
            <bpmndi:BPMNShape id="UserTask_0ornknv_di" bpmnElement="Task_085qd6k">
              <dc:Bounds x="535" y="48" width="100" height="80" />
            </bpmndi:BPMNShape>
            <bpmndi:BPMNShape id="ExclusiveGateway_1y6gcmn_di" bpmnElement="ExclusiveGateway_1y6gcmn" isMarkerVisible="true">
              <dc:Bounds x="308" y="154" width="50" height="50" />
            </bpmndi:BPMNShape>
            <bpmndi:BPMNEdge id="SequenceFlow_1gm6tlc_di" bpmnElement="SequenceFlow_1gm6tlc">
              <di:waypoint x="333" y="204" />
              <di:waypoint x="333" y="285" />
              <di:waypoint x="535" y="285" />
              <bpmndi:BPMNLabel>
                <dc:Bounds x="340" y="256" width="77" height="27" />
              </bpmndi:BPMNLabel>
            </bpmndi:BPMNEdge>
            <bpmndi:BPMNEdge id="SequenceFlow_0uc2k2q_di" bpmnElement="SequenceFlow_0uc2k2q">
              <di:waypoint x="333" y="154" />
              <di:waypoint x="333" y="88" />
              <di:waypoint x="535" y="88" />
              <bpmndi:BPMNLabel>
                <dc:Bounds x="338" y="93" width="77" height="27" />
              </bpmndi:BPMNLabel>
            </bpmndi:BPMNEdge>
          </bpmndi:BPMNPlane>
        </bpmndi:BPMNDiagram>
      </bpmn2:definitions>`
    },
    {
      id: 3,
      key: "Process_3",
      name: '征文评审',
      businessKey: "bizApproval",
      xml: `<?xml version="1.0" encoding="UTF-8"?>
      <bpmn2:definitions xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:bpmn2="http://www.omg.org/spec/BPMN/20100524/MODEL" xmlns:bpmndi="http://www.omg.org/spec/BPMN/20100524/DI" xmlns:dc="http://www.omg.org/spec/DD/20100524/DC" xmlns:di="http://www.omg.org/spec/DD/20100524/DI" id="sample-diagram" targetNamespace="http://bpmn.io/schema/bpmn" xsi:schemaLocation="http://www.omg.org/spec/BPMN/20100524/MODEL BPMN20.xsd">
        <bpmn2:process id="Process_2" name="征文评审" isExecutable="true">
          <bpmn2:startEvent id="StartEvent_1vos7ug">
            <bpmn2:outgoing>SequenceFlow_0ovi1nz</bpmn2:outgoing>
          </bpmn2:startEvent>
          <bpmn2:userTask id="Task_18pm3q5" name="提交征文">
            <bpmn2:incoming>SequenceFlow_0ovi1nz</bpmn2:incoming>
            <bpmn2:incoming>SequenceFlow_02pvqsr</bpmn2:incoming>
            <bpmn2:incoming>SequenceFlow_05gko23</bpmn2:incoming>
            <bpmn2:outgoing>SequenceFlow_0xpzfvy</bpmn2:outgoing>
          </bpmn2:userTask>
          <bpmn2:userTask id="Task_14u14ic" name="评审征文">
            <bpmn2:incoming>SequenceFlow_0zetnhm</bpmn2:incoming>
            <bpmn2:outgoing>SequenceFlow_1ngdvso</bpmn2:outgoing>
          </bpmn2:userTask>
          <bpmn2:sequenceFlow id="SequenceFlow_0ovi1nz" sourceRef="StartEvent_1vos7ug" targetRef="Task_18pm3q5" />
          <bpmn2:exclusiveGateway id="ExclusiveGateway_0utkmqq">
            <bpmn2:incoming>SequenceFlow_1ngdvso</bpmn2:incoming>
            <bpmn2:outgoing>SequenceFlow_02pvqsr</bpmn2:outgoing>
            <bpmn2:outgoing>SequenceFlow_0vyo6vm</bpmn2:outgoing>
            <bpmn2:outgoing>SequenceFlow_1ihs5aa</bpmn2:outgoing>
          </bpmn2:exclusiveGateway>
          <bpmn2:sequenceFlow id="SequenceFlow_1ngdvso" sourceRef="Task_14u14ic" targetRef="ExclusiveGateway_0utkmqq" />
          <bpmn2:sequenceFlow id="SequenceFlow_0xpzfvy" sourceRef="Task_18pm3q5" targetRef="Task_135uxmi" />
          <bpmn2:sendTask id="Task_135uxmi" name="发送提醒消息">
            <bpmn2:incoming>SequenceFlow_0xpzfvy</bpmn2:incoming>
            <bpmn2:outgoing>SequenceFlow_0zetnhm</bpmn2:outgoing>
          </bpmn2:sendTask>
          <bpmn2:userTask id="Task_0579d9q" name="主办方评审">
            <bpmn2:incoming>SequenceFlow_0vyo6vm</bpmn2:incoming>
            <bpmn2:outgoing>SequenceFlow_18s58ao</bpmn2:outgoing>
          </bpmn2:userTask>
          <bpmn2:sequenceFlow id="SequenceFlow_02pvqsr" name="需要修改" sourceRef="ExclusiveGateway_0utkmqq" targetRef="Task_18pm3q5" />
          <bpmn2:sequenceFlow id="SequenceFlow_0vyo6vm" name="通过" sourceRef="ExclusiveGateway_0utkmqq" targetRef="Task_0579d9q" />
          <bpmn2:endEvent id="EndEvent_0uf4c6r">
            <bpmn2:incoming>SequenceFlow_1ihs5aa</bpmn2:incoming>
          </bpmn2:endEvent>
          <bpmn2:sequenceFlow id="SequenceFlow_1ihs5aa" sourceRef="ExclusiveGateway_0utkmqq" targetRef="EndEvent_0uf4c6r" />
          <bpmn2:exclusiveGateway id="ExclusiveGateway_0yelbqd">
            <bpmn2:incoming>SequenceFlow_18s58ao</bpmn2:incoming>
            <bpmn2:outgoing>SequenceFlow_1idx54d</bpmn2:outgoing>
            <bpmn2:outgoing>SequenceFlow_1rzufci</bpmn2:outgoing>
            <bpmn2:outgoing>SequenceFlow_05gko23</bpmn2:outgoing>
          </bpmn2:exclusiveGateway>
          <bpmn2:sequenceFlow id="SequenceFlow_18s58ao" sourceRef="Task_0579d9q" targetRef="ExclusiveGateway_0yelbqd" />
          <bpmn2:endEvent id="EndEvent_1osg9wf">
            <bpmn2:incoming>SequenceFlow_1idx54d</bpmn2:incoming>
          </bpmn2:endEvent>
          <bpmn2:sequenceFlow id="SequenceFlow_1idx54d" name="拒绝" sourceRef="ExclusiveGateway_0yelbqd" targetRef="EndEvent_1osg9wf" />
          <bpmn2:sequenceFlow id="SequenceFlow_1rzufci" name="通过" sourceRef="ExclusiveGateway_0yelbqd" targetRef="ExclusiveGateway_1icn7au" />
          <bpmn2:parallelGateway id="ExclusiveGateway_1icn7au">
            <bpmn2:incoming>SequenceFlow_1rzufci</bpmn2:incoming>
            <bpmn2:outgoing>SequenceFlow_0n9mdzn</bpmn2:outgoing>
            <bpmn2:outgoing>SequenceFlow_0g31ro9</bpmn2:outgoing>
          </bpmn2:parallelGateway>
          <bpmn2:sequenceFlow id="SequenceFlow_0n9mdzn" sourceRef="ExclusiveGateway_1icn7au" targetRef="Task_1fp82yl" />
          <bpmn2:sequenceFlow id="SequenceFlow_0g31ro9" sourceRef="ExclusiveGateway_1icn7au" targetRef="Task_18ybx93" />
          <bpmn2:serviceTask id="Task_1fp82yl" name="转为日程">
            <bpmn2:incoming>SequenceFlow_0n9mdzn</bpmn2:incoming>
            <bpmn2:outgoing>SequenceFlow_1igad7i</bpmn2:outgoing>
          </bpmn2:serviceTask>
          <bpmn2:serviceTask id="Task_18ybx93" name="转为电子海报">
            <bpmn2:incoming>SequenceFlow_0g31ro9</bpmn2:incoming>
            <bpmn2:outgoing>SequenceFlow_1u9620v</bpmn2:outgoing>
          </bpmn2:serviceTask>
          <bpmn2:endEvent id="EndEvent_0kv5dog">
            <bpmn2:incoming>SequenceFlow_1u9620v</bpmn2:incoming>
            <bpmn2:incoming>SequenceFlow_1igad7i</bpmn2:incoming>
          </bpmn2:endEvent>
          <bpmn2:sequenceFlow id="SequenceFlow_1u9620v" sourceRef="Task_18ybx93" targetRef="EndEvent_0kv5dog" />
          <bpmn2:sequenceFlow id="SequenceFlow_1igad7i" sourceRef="Task_1fp82yl" targetRef="EndEvent_0kv5dog" />
          <bpmn2:sequenceFlow id="SequenceFlow_05gko23" name="重新修改" sourceRef="ExclusiveGateway_0yelbqd" targetRef="Task_18pm3q5" />
          <bpmn2:sequenceFlow id="SequenceFlow_0zetnhm" sourceRef="Task_135uxmi" targetRef="Task_14u14ic" />
        </bpmn2:process>
        <bpmndi:BPMNDiagram id="BPMNDiagram_1">
          <bpmndi:BPMNPlane id="BPMNPlane_1" bpmnElement="Process_2">
            <bpmndi:BPMNShape id="StartEvent_1vos7ug_di" bpmnElement="StartEvent_1vos7ug">
              <dc:Bounds x="112" y="131" width="36" height="36" />
            </bpmndi:BPMNShape>
            <bpmndi:BPMNShape id="UserTask_14e99t3_di" bpmnElement="Task_18pm3q5">
              <dc:Bounds x="198" y="109" width="100" height="80" />
            </bpmndi:BPMNShape>
            <bpmndi:BPMNShape id="UserTask_1501v9q_di" bpmnElement="Task_14u14ic">
              <dc:Bounds x="505" y="109" width="100" height="80" />
            </bpmndi:BPMNShape>
            <bpmndi:BPMNEdge id="SequenceFlow_0ovi1nz_di" bpmnElement="SequenceFlow_0ovi1nz">
              <di:waypoint x="148" y="149" />
              <di:waypoint x="198" y="149" />
            </bpmndi:BPMNEdge>
            <bpmndi:BPMNShape id="ExclusiveGateway_0utkmqq_di" bpmnElement="ExclusiveGateway_0utkmqq" isMarkerVisible="true">
              <dc:Bounds x="663" y="124" width="50" height="50" />
            </bpmndi:BPMNShape>
            <bpmndi:BPMNEdge id="SequenceFlow_1ngdvso_di" bpmnElement="SequenceFlow_1ngdvso">
              <di:waypoint x="605" y="149" />
              <di:waypoint x="663" y="149" />
            </bpmndi:BPMNEdge>
            <bpmndi:BPMNEdge id="SequenceFlow_0xpzfvy_di" bpmnElement="SequenceFlow_0xpzfvy">
              <di:waypoint x="298" y="149" />
              <di:waypoint x="345" y="149" />
            </bpmndi:BPMNEdge>
            <bpmndi:BPMNShape id="SendTask_1on2zal_di" bpmnElement="Task_135uxmi">
              <dc:Bounds x="345" y="109" width="100" height="80" />
            </bpmndi:BPMNShape>
            <bpmndi:BPMNShape id="UserTask_1rglnei_di" bpmnElement="Task_0579d9q">
              <dc:Bounds x="776" y="109" width="100" height="80" />
            </bpmndi:BPMNShape>
            <bpmndi:BPMNEdge id="SequenceFlow_02pvqsr_di" bpmnElement="SequenceFlow_02pvqsr">
              <di:waypoint x="688" y="124" />
              <di:waypoint x="688" y="31" />
              <di:waypoint x="248" y="31" />
              <di:waypoint x="248" y="109" />
              <bpmndi:BPMNLabel>
                <dc:Bounds x="447" y="13" width="44" height="14" />
              </bpmndi:BPMNLabel>
            </bpmndi:BPMNEdge>
            <bpmndi:BPMNEdge id="SequenceFlow_0vyo6vm_di" bpmnElement="SequenceFlow_0vyo6vm">
              <di:waypoint x="713" y="149" />
              <di:waypoint x="776" y="149" />
              <bpmndi:BPMNLabel>
                <dc:Bounds x="734" y="131" width="22" height="14" />
              </bpmndi:BPMNLabel>
            </bpmndi:BPMNEdge>
            <bpmndi:BPMNShape id="EndEvent_0uf4c6r_di" bpmnElement="EndEvent_0uf4c6r">
              <dc:Bounds x="808" y="13" width="36" height="36" />
            </bpmndi:BPMNShape>
            <bpmndi:BPMNEdge id="SequenceFlow_1ihs5aa_di" bpmnElement="SequenceFlow_1ihs5aa">
              <di:waypoint x="688" y="124" />
              <di:waypoint x="688" y="31" />
              <di:waypoint x="808" y="31" />
            </bpmndi:BPMNEdge>
            <bpmndi:BPMNShape id="ExclusiveGateway_0yelbqd_di" bpmnElement="ExclusiveGateway_0yelbqd" isMarkerVisible="true">
              <dc:Bounds x="801" y="253" width="50" height="50" />
            </bpmndi:BPMNShape>
            <bpmndi:BPMNEdge id="SequenceFlow_18s58ao_di" bpmnElement="SequenceFlow_18s58ao">
              <di:waypoint x="826" y="189" />
              <di:waypoint x="826" y="253" />
            </bpmndi:BPMNEdge>
            <bpmndi:BPMNShape id="EndEvent_1osg9wf_di" bpmnElement="EndEvent_1osg9wf">
              <dc:Bounds x="914" y="310" width="36" height="36" />
            </bpmndi:BPMNShape>
            <bpmndi:BPMNEdge id="SequenceFlow_1idx54d_di" bpmnElement="SequenceFlow_1idx54d">
              <di:waypoint x="826" y="303" />
              <di:waypoint x="826" y="328" />
              <di:waypoint x="914" y="328" />
              <bpmndi:BPMNLabel>
                <dc:Bounds x="854" y="260" width="22" height="14" />
              </bpmndi:BPMNLabel>
            </bpmndi:BPMNEdge>
            <bpmndi:BPMNEdge id="SequenceFlow_1rzufci_di" bpmnElement="SequenceFlow_1rzufci">
              <di:waypoint x="826" y="303" />
              <di:waypoint x="826" y="376" />
              <bpmndi:BPMNLabel>
                <dc:Bounds x="830" y="342" width="22" height="14" />
              </bpmndi:BPMNLabel>
            </bpmndi:BPMNEdge>
            <bpmndi:BPMNShape id="ParallelGateway_1ui6kco_di" bpmnElement="ExclusiveGateway_1icn7au">
              <dc:Bounds x="801" y="376" width="50" height="50" />
            </bpmndi:BPMNShape>
            <bpmndi:BPMNEdge id="SequenceFlow_0n9mdzn_di" bpmnElement="SequenceFlow_0n9mdzn">
              <di:waypoint x="801" y="401" />
              <di:waypoint x="728" y="401" />
              <di:waypoint x="728" y="448" />
            </bpmndi:BPMNEdge>
            <bpmndi:BPMNEdge id="SequenceFlow_0g31ro9_di" bpmnElement="SequenceFlow_0g31ro9">
              <di:waypoint x="851" y="401" />
              <di:waypoint x="911" y="401" />
              <di:waypoint x="911" y="448" />
            </bpmndi:BPMNEdge>
            <bpmndi:BPMNShape id="ServiceTask_1b1akm1_di" bpmnElement="Task_1fp82yl">
              <dc:Bounds x="678" y="448" width="100" height="80" />
            </bpmndi:BPMNShape>
            <bpmndi:BPMNShape id="ServiceTask_0ytqh0g_di" bpmnElement="Task_18ybx93">
              <dc:Bounds x="861" y="448" width="100" height="80" />
            </bpmndi:BPMNShape>
            <bpmndi:BPMNShape id="EndEvent_0kv5dog_di" bpmnElement="EndEvent_0kv5dog">
              <dc:Bounds x="808" y="548" width="36" height="36" />
            </bpmndi:BPMNShape>
            <bpmndi:BPMNEdge id="SequenceFlow_1u9620v_di" bpmnElement="SequenceFlow_1u9620v">
              <di:waypoint x="911" y="528" />
              <di:waypoint x="911" y="566" />
              <di:waypoint x="844" y="566" />
            </bpmndi:BPMNEdge>
            <bpmndi:BPMNEdge id="SequenceFlow_1igad7i_di" bpmnElement="SequenceFlow_1igad7i">
              <di:waypoint x="728" y="528" />
              <di:waypoint x="728" y="566" />
              <di:waypoint x="808" y="566" />
            </bpmndi:BPMNEdge>
            <bpmndi:BPMNEdge id="SequenceFlow_05gko23_di" bpmnElement="SequenceFlow_05gko23">
              <di:waypoint x="801" y="278" />
              <di:waypoint x="248" y="278" />
              <di:waypoint x="248" y="189" />
              <bpmndi:BPMNLabel>
                <dc:Bounds x="503" y="260" width="44" height="14" />
              </bpmndi:BPMNLabel>
            </bpmndi:BPMNEdge>
            <bpmndi:BPMNEdge id="SequenceFlow_0zetnhm_di" bpmnElement="SequenceFlow_0zetnhm">
              <di:waypoint x="445" y="149" />
              <di:waypoint x="505" y="149" />
            </bpmndi:BPMNEdge>
          </bpmndi:BPMNPlane>
        </bpmndi:BPMNDiagram>
      </bpmn2:definitions>`
    }
  ]

  constructor(private es: EventAggregator, private processDefineService: IProcessDefineService) {
    this.processDefineService.latest().then(data => this.defines = data);
  }

  activate(model, nctx){
    this.es.subscribe("deploied", (wf) =>{
      this.processDefineService.latest().then(data => this.defines = data);
    });
  }

  createProcess() {
    let model = this.defaultBmpnModel();

    let def = {
      id: uuid.v4(),
      name: "新的流程",
      businessKey: "",
      key: model.key,
      xml: model.xml
    };

    this.defines.push(def);

    this.es.publish("openWorkflow", def);
  }

  private defaultBmpnModel() {
    let key = 'Process_' + shortid.generate().replace(/-/g, "_");
    let xml = defBpmn.replace(/\{0\}/g, key).replace(/\{1\}/g, 'Start_' + shortid.generate().replace(/-/g, '_'));

    return { key: key, xml: xml };
  }

  select;

  selected(id) {
    this.processDefineService.getProcessModel(id).then(data => {
      var def = this.defines.find(x => x.id == id);
      this.select = def;
      if (data != null && data != "") {
        def.xml = data;
      } else if (def.xml == null) {
        def.xml = this.defaultBmpnModel().xml;
      }
      this.es.publish("openWorkflow", def);
    })
  }
}