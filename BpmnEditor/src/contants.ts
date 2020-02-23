
var uuid = require('uuid');
var shortid = require('shortid');

export default {
  defaultBpmXml: function (name) {
    let key = `Process_${shortid().replace(/-/g, "")}`;
    let sid = 'W' + shortid().replace(/-/g, "");
    return {
      key: key,
      xml: `<?xml version="1.0" encoding="UTF-8"?>
      <bpmn2:definitions xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:bpmn2="http://www.omg.org/spec/BPMN/20100524/MODEL" xmlns:bpmndi="http://www.omg.org/spec/BPMN/20100524/DI" xmlns:dc="http://www.omg.org/spec/DD/20100524/DC" targetNamespace="http://bpmn.io/schema/bpmn" xsi:schemaLocation="http://www.omg.org/spec/BPMN/20100524/MODEL BPMN20.xsd">
        <bpmn2:process id="${key}" name="${name}" isExecutable="true">
          <bpmn2:startEvent id="StartEvent_${sid}" />
        </bpmn2:process>
        <bpmndi:BPMNDiagram id="BPMNDiagram_1">
          <bpmndi:BPMNPlane id="BPMNPlane_1" bpmnElement="${key}">
            <bpmndi:BPMNShape id="StartEvent_${sid}_di" bpmnElement="StartEvent_${sid}">
              <dc:Bounds x="164" y="187" width="36" height="36" />
            </bpmndi:BPMNShape>
          </bpmndi:BPMNPlane>
        </bpmndi:BPMNDiagram>
      </bpmn2:definitions>`
    }
  }
}
