using System;
using System.Collections.Generic;

/*
 * Copyright 2018 Alfresco, Inc. and/or its affiliates.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *       http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace Sys.Workflow.Image.Impl
{

	using Sys.Workflow.Bpmn.Models;
	using Sys.Workflow.Bpmn.Models;
	using Sys.Workflow.Bpmn.Models;
	using Sys.Workflow.Bpmn.Models;
	using Sys.Workflow.Bpmn.Models;
	using Sys.Workflow.Bpmn.Models;
	using Sys.Workflow.Bpmn.Models;
	using Sys.Workflow.Bpmn.Models;
	using Sys.Workflow.Bpmn.Models;
	using Sys.Workflow.Bpmn.Models;
	using Sys.Workflow.Bpmn.Models;
	using Sys.Workflow.Bpmn.Models;
	using Sys.Workflow.Bpmn.Models;
	using Sys.Workflow.Bpmn.Models;
	using Sys.Workflow.Bpmn.Models;
	using Sys.Workflow.Bpmn.Models;
	using Sys.Workflow.Bpmn.Models;
	using Sys.Workflow.Bpmn.Models;
	using Sys.Workflow.Bpmn.Models;
	using Sys.Workflow.Bpmn.Models;
	using Sys.Workflow.Bpmn.Models;
	using Sys.Workflow.Bpmn.Models;
	using Sys.Workflow.Bpmn.Models;
	using Sys.Workflow.Bpmn.Models;
	using Sys.Workflow.Bpmn.Models;
	using Sys.Workflow.Bpmn.Models;
	using Sys.Workflow.Bpmn.Models;
	using Sys.Workflow.Bpmn.Models;
	using Sys.Workflow.Bpmn.Models;
	using Sys.Workflow.Bpmn.Models;
	using Sys.Workflow.Bpmn.Models;
	using Sys.Workflow.Bpmn.Models;
	using Sys.Workflow.Bpmn.Models;
	using Sys.Workflow.Bpmn.Models;
	using Sys.Workflow.Bpmn.Models;
	using Sys.Workflow.Bpmn.Models;
	using Sys.Workflow.Bpmn.Models;
	using Sys.Workflow.Bpmn.Models;
	using Sys.Workflow.Bpmn.Models;
	using Sys.Workflow.Bpmn.Models;
	using Sys.Workflow.Bpmn.Models;
	using Sys.Workflow.Bpmn.Models;
	using Sys.Workflow.Bpmn.Models;
	using Sys.Workflow.Bpmn.Models;
	using Sys.Workflow.Image.Exceptions;
	using Sys.Workflow.Image.Exceptions;
	using org.springframework.stereotype;

	/// <summary>
	/// Class to generate an svg based the diagram interchange information in a
	/// BPMN 2.0 process.
	/// </summary>
    public class DefaultProcessDiagramGenerator : IProcessDiagramGenerator
	{
		private const string DEFAULT_ACTIVITY_FONT_NAME = "Arial";

		private const string DEFAULT_LABEL_FONT_NAME = "Arial";

		private const string DEFAULT_ANNOTATION_FONT_NAME = "Arial";

		private const string DEFAULT_DIAGRAM_IMAGE_FILE_NAME = "/image/na.svg";

		protected internal IDictionary<Type, ActivityDrawInstruction> activityDrawInstructions = new Dictionary<Type, ActivityDrawInstruction>();

		protected internal IDictionary<Type, ArtifactDrawInstruction> artifactDrawInstructions = new Dictionary<Type, ArtifactDrawInstruction>();

		public virtual string DefaultActivityFontName
		{
			get
			{
				return DEFAULT_ACTIVITY_FONT_NAME;
			}
		}

		public virtual string DefaultLabelFontName
		{
			get
			{
				return DEFAULT_LABEL_FONT_NAME;
			}
		}

		public virtual string DefaultAnnotationFontName
		{
			get
			{
				return DEFAULT_ANNOTATION_FONT_NAME;
			}
		}

		public virtual string DefaultDiagramImageFileName
		{
			get
			{
				return DEFAULT_DIAGRAM_IMAGE_FILE_NAME;
			}
		}

		// The instructions on how to draw a certain construct is
		// created statically and stored in a map for performance.
		public DefaultProcessDiagramGenerator()
		{
			// start event
			activityDrawInstructions[typeof(StartEvent)] = new ActivityDrawInstructionAnonymousInnerClass(this);

			// signal catch
			activityDrawInstructions[typeof(IntermediateCatchEvent)] = new ActivityDrawInstructionAnonymousInnerClass2(this);

			// signal throw
			activityDrawInstructions[typeof(ThrowEvent)] = new ActivityDrawInstructionAnonymousInnerClass3(this);

			// end event
			activityDrawInstructions[typeof(EndEvent)] = new ActivityDrawInstructionAnonymousInnerClass4(this);

			// task
			activityDrawInstructions[typeof(Task)] = new ActivityDrawInstructionAnonymousInnerClass5(this);

			// user task
			activityDrawInstructions[typeof(UserTask)] = new ActivityDrawInstructionAnonymousInnerClass6(this);

			// script task
			activityDrawInstructions[typeof(ScriptTask)] = new ActivityDrawInstructionAnonymousInnerClass7(this);

			// service task
			activityDrawInstructions[typeof(ServiceTask)] = new ActivityDrawInstructionAnonymousInnerClass8(this);

			// receive task
			activityDrawInstructions[typeof(ReceiveTask)] = new ActivityDrawInstructionAnonymousInnerClass9(this);

			// send task
			activityDrawInstructions[typeof(SendTask)] = new ActivityDrawInstructionAnonymousInnerClass10(this);

			// manual task
			activityDrawInstructions[typeof(ManualTask)] = new ActivityDrawInstructionAnonymousInnerClass11(this);

			// businessRuleTask task
			activityDrawInstructions[typeof(BusinessRuleTask)] = new ActivityDrawInstructionAnonymousInnerClass12(this);

			// exclusive gateway
			activityDrawInstructions[typeof(ExclusiveGateway)] = new ActivityDrawInstructionAnonymousInnerClass13(this);

			// inclusive gateway
			activityDrawInstructions[typeof(InclusiveGateway)] = new ActivityDrawInstructionAnonymousInnerClass14(this);

			// parallel gateway
			activityDrawInstructions[typeof(ParallelGateway)] = new ActivityDrawInstructionAnonymousInnerClass15(this);

			// event based gateway
			activityDrawInstructions[typeof(EventGateway)] = new ActivityDrawInstructionAnonymousInnerClass16(this);

			// Boundary timer
			activityDrawInstructions[typeof(BoundaryEvent)] = new ActivityDrawInstructionAnonymousInnerClass17(this);

			// subprocess
			activityDrawInstructions[typeof(SubProcess)] = new ActivityDrawInstructionAnonymousInnerClass18(this);

			// Event subprocess
			activityDrawInstructions[typeof(EventSubProcess)] = new ActivityDrawInstructionAnonymousInnerClass19(this);

			// call activity
			activityDrawInstructions[typeof(CallActivity)] = new ActivityDrawInstructionAnonymousInnerClass20(this);

			// text annotation
			artifactDrawInstructions[typeof(TextAnnotation)] = new ArtifactDrawInstructionAnonymousInnerClass(this);

			// association
			artifactDrawInstructions[typeof(Association)] = new ArtifactDrawInstructionAnonymousInnerClass2(this);
		}

		private class ActivityDrawInstructionAnonymousInnerClass : ActivityDrawInstruction
		{
			private readonly DefaultProcessDiagramGenerator outerInstance;

			public ActivityDrawInstructionAnonymousInnerClass(DefaultProcessDiagramGenerator outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public virtual void draw(DefaultProcessDiagramCanvas processDiagramCanvas, BpmnModel bpmnModel, FlowNode flowNode)
			{
				GraphicInfo graphicInfo = bpmnModel.getGraphicInfo(flowNode.Id);
				StartEvent startEvent = (StartEvent) flowNode;
				if (startEvent.EventDefinitions != null && startEvent.EventDefinitions.Count > 0)
				{
					EventDefinition eventDefinition = startEvent.EventDefinitions[0];
					if (eventDefinition is TimerEventDefinition)
					{
						processDiagramCanvas.drawTimerStartEvent(flowNode.Id, graphicInfo);
					}
					else if (eventDefinition is ErrorEventDefinition)
					{
						processDiagramCanvas.drawErrorStartEvent(flowNode.Id, graphicInfo);
					}
					else if (eventDefinition is SignalEventDefinition)
					{
						processDiagramCanvas.drawSignalStartEvent(flowNode.Id, graphicInfo);
					}
					else if (eventDefinition is MessageEventDefinition)
					{
						processDiagramCanvas.drawMessageStartEvent(flowNode.Id, graphicInfo);
					}
					else
					{
						processDiagramCanvas.drawNoneStartEvent(flowNode.Id, graphicInfo);
					}
				}
				else
				{
					processDiagramCanvas.drawNoneStartEvent(flowNode.Id, graphicInfo);
				}
			}
		}

		private class ActivityDrawInstructionAnonymousInnerClass2 : ActivityDrawInstruction
		{
			private readonly DefaultProcessDiagramGenerator outerInstance;

			public ActivityDrawInstructionAnonymousInnerClass2(DefaultProcessDiagramGenerator outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public virtual void draw(DefaultProcessDiagramCanvas processDiagramCanvas, BpmnModel bpmnModel, FlowNode flowNode)
			{
				GraphicInfo graphicInfo = bpmnModel.getGraphicInfo(flowNode.Id);
				IntermediateCatchEvent intermediateCatchEvent = (IntermediateCatchEvent) flowNode;
				if (intermediateCatchEvent.EventDefinitions != null && intermediateCatchEvent.EventDefinitions.Count > 0)
				{
					if (intermediateCatchEvent.EventDefinitions[0] is SignalEventDefinition)
					{
						processDiagramCanvas.drawCatchingSignalEvent(flowNode.Id, flowNode.Name, graphicInfo, true);
					}
					else if (intermediateCatchEvent.EventDefinitions[0] is TimerEventDefinition)
					{
						processDiagramCanvas.drawCatchingTimerEvent(flowNode.Id, flowNode.Name, graphicInfo, true);
					}
					else if (intermediateCatchEvent.EventDefinitions[0] is MessageEventDefinition)
					{
						processDiagramCanvas.drawCatchingMessageEvent(flowNode.Id, flowNode.Name, graphicInfo, true);
					}
				}
			}
		}

		private class ActivityDrawInstructionAnonymousInnerClass3 : ActivityDrawInstruction
		{
			private readonly DefaultProcessDiagramGenerator outerInstance;

			public ActivityDrawInstructionAnonymousInnerClass3(DefaultProcessDiagramGenerator outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public virtual void draw(DefaultProcessDiagramCanvas processDiagramCanvas, BpmnModel bpmnModel, FlowNode flowNode)
			{
				GraphicInfo graphicInfo = bpmnModel.getGraphicInfo(flowNode.Id);
				ThrowEvent throwEvent = (ThrowEvent) flowNode;
				if (throwEvent.EventDefinitions != null && throwEvent.EventDefinitions.Count > 0)
				{
					if (throwEvent.EventDefinitions[0] is SignalEventDefinition)
					{
						processDiagramCanvas.drawThrowingSignalEvent(flowNode.Id, graphicInfo);
					}
					else if (throwEvent.EventDefinitions[0] is CompensateEventDefinition)
					{
						processDiagramCanvas.drawThrowingCompensateEvent(flowNode.Id, graphicInfo);
					}
					else
					{
						processDiagramCanvas.drawThrowingNoneEvent(flowNode.Id, graphicInfo);
					}
				}
				else
				{
					processDiagramCanvas.drawThrowingNoneEvent(flowNode.Id, graphicInfo);
				}
			}
		}

		private class ActivityDrawInstructionAnonymousInnerClass4 : ActivityDrawInstruction
		{
			private readonly DefaultProcessDiagramGenerator outerInstance;

			public ActivityDrawInstructionAnonymousInnerClass4(DefaultProcessDiagramGenerator outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public virtual void draw(DefaultProcessDiagramCanvas processDiagramCanvas, BpmnModel bpmnModel, FlowNode flowNode)
			{
				GraphicInfo graphicInfo = bpmnModel.getGraphicInfo(flowNode.Id);
				EndEvent endEvent = (EndEvent) flowNode;
				if (endEvent.EventDefinitions != null && endEvent.EventDefinitions.Count > 0)
				{
					if (endEvent.EventDefinitions[0] is ErrorEventDefinition)
					{
						processDiagramCanvas.drawErrorEndEvent(flowNode.Id, flowNode.Name, graphicInfo);
					}
					else
					{
						processDiagramCanvas.drawNoneEndEvent(flowNode.Id, flowNode.Name, graphicInfo);
					}
				}
				else
				{
					processDiagramCanvas.drawNoneEndEvent(flowNode.Id, flowNode.Name, graphicInfo);
				}
			}
		}

		private class ActivityDrawInstructionAnonymousInnerClass5 : ActivityDrawInstruction
		{
			private readonly DefaultProcessDiagramGenerator outerInstance;

			public ActivityDrawInstructionAnonymousInnerClass5(DefaultProcessDiagramGenerator outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public virtual void draw(DefaultProcessDiagramCanvas processDiagramCanvas, BpmnModel bpmnModel, FlowNode flowNode)
			{
				GraphicInfo graphicInfo = bpmnModel.getGraphicInfo(flowNode.Id);
				processDiagramCanvas.drawTask(flowNode.Id, flowNode.Name, graphicInfo);
			}
		}

		private class ActivityDrawInstructionAnonymousInnerClass6 : ActivityDrawInstruction
		{
			private readonly DefaultProcessDiagramGenerator outerInstance;

			public ActivityDrawInstructionAnonymousInnerClass6(DefaultProcessDiagramGenerator outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public virtual void draw(DefaultProcessDiagramCanvas processDiagramCanvas, BpmnModel bpmnModel, FlowNode flowNode)
			{
				GraphicInfo graphicInfo = bpmnModel.getGraphicInfo(flowNode.Id);
				processDiagramCanvas.drawUserTask(flowNode.Id, flowNode.Name, graphicInfo);
			}
		}

		private class ActivityDrawInstructionAnonymousInnerClass7 : ActivityDrawInstruction
		{
			private readonly DefaultProcessDiagramGenerator outerInstance;

			public ActivityDrawInstructionAnonymousInnerClass7(DefaultProcessDiagramGenerator outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public virtual void draw(DefaultProcessDiagramCanvas processDiagramCanvas, BpmnModel bpmnModel, FlowNode flowNode)
			{
				GraphicInfo graphicInfo = bpmnModel.getGraphicInfo(flowNode.Id);
				processDiagramCanvas.drawScriptTask(flowNode.Id, flowNode.Name, graphicInfo);
			}
		}

		private class ActivityDrawInstructionAnonymousInnerClass8 : ActivityDrawInstruction
		{
			private readonly DefaultProcessDiagramGenerator outerInstance;

			public ActivityDrawInstructionAnonymousInnerClass8(DefaultProcessDiagramGenerator outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public virtual void draw(DefaultProcessDiagramCanvas processDiagramCanvas, BpmnModel bpmnModel, FlowNode flowNode)
			{
				GraphicInfo graphicInfo = bpmnModel.getGraphicInfo(flowNode.Id);
				ServiceTask serviceTask = (ServiceTask) flowNode;
				processDiagramCanvas.drawServiceTask(flowNode.Id, serviceTask.Name, graphicInfo);
			}
		}

		private class ActivityDrawInstructionAnonymousInnerClass9 : ActivityDrawInstruction
		{
			private readonly DefaultProcessDiagramGenerator outerInstance;

			public ActivityDrawInstructionAnonymousInnerClass9(DefaultProcessDiagramGenerator outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public virtual void draw(DefaultProcessDiagramCanvas processDiagramCanvas, BpmnModel bpmnModel, FlowNode flowNode)
			{
				GraphicInfo graphicInfo = bpmnModel.getGraphicInfo(flowNode.Id);
				processDiagramCanvas.drawReceiveTask(flowNode.Id, flowNode.Name, graphicInfo);
			}
		}

		private class ActivityDrawInstructionAnonymousInnerClass10 : ActivityDrawInstruction
		{
			private readonly DefaultProcessDiagramGenerator outerInstance;

			public ActivityDrawInstructionAnonymousInnerClass10(DefaultProcessDiagramGenerator outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public virtual void draw(DefaultProcessDiagramCanvas processDiagramCanvas, BpmnModel bpmnModel, FlowNode flowNode)
			{
				GraphicInfo graphicInfo = bpmnModel.getGraphicInfo(flowNode.Id);
				processDiagramCanvas.drawSendTask(flowNode.Id, flowNode.Name, graphicInfo);
			}
		}

		private class ActivityDrawInstructionAnonymousInnerClass11 : ActivityDrawInstruction
		{
			private readonly DefaultProcessDiagramGenerator outerInstance;

			public ActivityDrawInstructionAnonymousInnerClass11(DefaultProcessDiagramGenerator outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public virtual void draw(DefaultProcessDiagramCanvas processDiagramCanvas, BpmnModel bpmnModel, FlowNode flowNode)
			{
				GraphicInfo graphicInfo = bpmnModel.getGraphicInfo(flowNode.Id);
				processDiagramCanvas.drawManualTask(flowNode.Id, flowNode.Name, graphicInfo);
			}
		}

		private class ActivityDrawInstructionAnonymousInnerClass12 : ActivityDrawInstruction
		{
			private readonly DefaultProcessDiagramGenerator outerInstance;

			public ActivityDrawInstructionAnonymousInnerClass12(DefaultProcessDiagramGenerator outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public virtual void draw(DefaultProcessDiagramCanvas processDiagramCanvas, BpmnModel bpmnModel, FlowNode flowNode)
			{
				GraphicInfo graphicInfo = bpmnModel.getGraphicInfo(flowNode.Id);
				processDiagramCanvas.drawBusinessRuleTask(flowNode.Id, flowNode.Name, graphicInfo);
			}
		}

		private class ActivityDrawInstructionAnonymousInnerClass13 : ActivityDrawInstruction
		{
			private readonly DefaultProcessDiagramGenerator outerInstance;

			public ActivityDrawInstructionAnonymousInnerClass13(DefaultProcessDiagramGenerator outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public virtual void draw(DefaultProcessDiagramCanvas processDiagramCanvas, BpmnModel bpmnModel, FlowNode flowNode)
			{
				GraphicInfo graphicInfo = bpmnModel.getGraphicInfo(flowNode.Id);
				processDiagramCanvas.drawExclusiveGateway(flowNode.Id, graphicInfo);
			}
		}

		private class ActivityDrawInstructionAnonymousInnerClass14 : ActivityDrawInstruction
		{
			private readonly DefaultProcessDiagramGenerator outerInstance;

			public ActivityDrawInstructionAnonymousInnerClass14(DefaultProcessDiagramGenerator outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public virtual void draw(DefaultProcessDiagramCanvas processDiagramCanvas, BpmnModel bpmnModel, FlowNode flowNode)
			{
				GraphicInfo graphicInfo = bpmnModel.getGraphicInfo(flowNode.Id);
				processDiagramCanvas.drawInclusiveGateway(flowNode.Id, graphicInfo);
			}
		}

		private class ActivityDrawInstructionAnonymousInnerClass15 : ActivityDrawInstruction
		{
			private readonly DefaultProcessDiagramGenerator outerInstance;

			public ActivityDrawInstructionAnonymousInnerClass15(DefaultProcessDiagramGenerator outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public virtual void draw(DefaultProcessDiagramCanvas processDiagramCanvas, BpmnModel bpmnModel, FlowNode flowNode)
			{
				GraphicInfo graphicInfo = bpmnModel.getGraphicInfo(flowNode.Id);
				processDiagramCanvas.drawParallelGateway(flowNode.Id, graphicInfo);
			}
		}

		private class ActivityDrawInstructionAnonymousInnerClass16 : ActivityDrawInstruction
		{
			private readonly DefaultProcessDiagramGenerator outerInstance;

			public ActivityDrawInstructionAnonymousInnerClass16(DefaultProcessDiagramGenerator outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public virtual void draw(DefaultProcessDiagramCanvas processDiagramCanvas, BpmnModel bpmnModel, FlowNode flowNode)
			{
				GraphicInfo graphicInfo = bpmnModel.getGraphicInfo(flowNode.Id);
				processDiagramCanvas.drawEventBasedGateway(flowNode.Id, graphicInfo);
			}
		}

		private class ActivityDrawInstructionAnonymousInnerClass17 : ActivityDrawInstruction
		{
			private readonly DefaultProcessDiagramGenerator outerInstance;

			public ActivityDrawInstructionAnonymousInnerClass17(DefaultProcessDiagramGenerator outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public virtual void draw(DefaultProcessDiagramCanvas processDiagramCanvas, BpmnModel bpmnModel, FlowNode flowNode)
			{
				GraphicInfo graphicInfo = bpmnModel.getGraphicInfo(flowNode.Id);
				BoundaryEvent boundaryEvent = (BoundaryEvent) flowNode;
				if (boundaryEvent.EventDefinitions != null && boundaryEvent.EventDefinitions.Count > 0)
				{
					if (boundaryEvent.EventDefinitions[0] is TimerEventDefinition)
					{

						processDiagramCanvas.drawCatchingTimerEvent(flowNode.Id, flowNode.Name, graphicInfo, boundaryEvent.CancelActivity);
					}
					else if (boundaryEvent.EventDefinitions[0] is ErrorEventDefinition)
					{

						processDiagramCanvas.drawCatchingErrorEvent(flowNode.Id, graphicInfo, boundaryEvent.CancelActivity);
					}
					else if (boundaryEvent.EventDefinitions[0] is SignalEventDefinition)
					{
						processDiagramCanvas.drawCatchingSignalEvent(flowNode.Id, flowNode.Name, graphicInfo, boundaryEvent.CancelActivity);
					}
					else if (boundaryEvent.EventDefinitions[0] is MessageEventDefinition)
					{
						processDiagramCanvas.drawCatchingMessageEvent(flowNode.Id, flowNode.Name, graphicInfo, boundaryEvent.CancelActivity);
					}
					else if (boundaryEvent.EventDefinitions[0] is CompensateEventDefinition)
					{
						processDiagramCanvas.drawCatchingCompensateEvent(flowNode.Id, graphicInfo, boundaryEvent.CancelActivity);
					}
				}
			}
		}

		private class ActivityDrawInstructionAnonymousInnerClass18 : ActivityDrawInstruction
		{
			private readonly DefaultProcessDiagramGenerator outerInstance;

			public ActivityDrawInstructionAnonymousInnerClass18(DefaultProcessDiagramGenerator outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public virtual void draw(DefaultProcessDiagramCanvas processDiagramCanvas, BpmnModel bpmnModel, FlowNode flowNode)
			{
				GraphicInfo graphicInfo = bpmnModel.getGraphicInfo(flowNode.Id);
				if (graphicInfo.Expanded != null && !graphicInfo.Expanded)
				{
					processDiagramCanvas.drawCollapsedSubProcess(flowNode.Id, flowNode.Name, graphicInfo, false);
				}
				else
				{
					processDiagramCanvas.drawExpandedSubProcess(flowNode.Id, flowNode.Name, graphicInfo, false);
				}
			}
		}

		private class ActivityDrawInstructionAnonymousInnerClass19 : ActivityDrawInstruction
		{
			private readonly DefaultProcessDiagramGenerator outerInstance;

			public ActivityDrawInstructionAnonymousInnerClass19(DefaultProcessDiagramGenerator outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public virtual void draw(DefaultProcessDiagramCanvas processDiagramCanvas, BpmnModel bpmnModel, FlowNode flowNode)
			{
				GraphicInfo graphicInfo = bpmnModel.getGraphicInfo(flowNode.Id);
				if (graphicInfo.Expanded != null && !graphicInfo.Expanded)
				{
					processDiagramCanvas.drawCollapsedSubProcess(flowNode.Id, flowNode.Name, graphicInfo, true);
				}
				else
				{
					processDiagramCanvas.drawExpandedSubProcess(flowNode.Id, flowNode.Name, graphicInfo, true);
				}
			}
		}

		private class ActivityDrawInstructionAnonymousInnerClass20 : ActivityDrawInstruction
		{
			private readonly DefaultProcessDiagramGenerator outerInstance;

			public ActivityDrawInstructionAnonymousInnerClass20(DefaultProcessDiagramGenerator outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public virtual void draw(DefaultProcessDiagramCanvas processDiagramCanvas, BpmnModel bpmnModel, FlowNode flowNode)
			{
				GraphicInfo graphicInfo = bpmnModel.getGraphicInfo(flowNode.Id);
				processDiagramCanvas.drawCollapsedCallActivity(flowNode.Id, flowNode.Name, graphicInfo);
			}
		}

		private class ArtifactDrawInstructionAnonymousInnerClass : ArtifactDrawInstruction
		{
			private readonly DefaultProcessDiagramGenerator outerInstance;

			public ArtifactDrawInstructionAnonymousInnerClass(DefaultProcessDiagramGenerator outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public virtual void draw(DefaultProcessDiagramCanvas processDiagramCanvas, BpmnModel bpmnModel, Artifact artifact)
			{
				GraphicInfo graphicInfo = bpmnModel.getGraphicInfo(artifact.Id);
				TextAnnotation textAnnotation = (TextAnnotation) artifact;
				processDiagramCanvas.drawTextAnnotation(textAnnotation.Id, textAnnotation.Text, graphicInfo);
			}
		}

		private class ArtifactDrawInstructionAnonymousInnerClass2 : ArtifactDrawInstruction
		{
			private readonly DefaultProcessDiagramGenerator outerInstance;

			public ArtifactDrawInstructionAnonymousInnerClass2(DefaultProcessDiagramGenerator outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public virtual void draw(DefaultProcessDiagramCanvas processDiagramCanvas, BpmnModel bpmnModel, Artifact artifact)
			{
				Association association = (Association) artifact;
				string sourceRef = association.SourceRef;
				string targetRef = association.TargetRef;

				// source and target can be instance of FlowElement or Artifact
				BaseElement sourceElement = bpmnModel.getFlowElement(sourceRef);
				BaseElement targetElement = bpmnModel.getFlowElement(targetRef);
				if (sourceElement == null)
				{
					sourceElement = bpmnModel.getArtifact(sourceRef);
				}
				if (targetElement == null)
				{
					targetElement = bpmnModel.getArtifact(targetRef);
				}
				IList<GraphicInfo> graphicInfoList = bpmnModel.getFlowLocationGraphicInfo(artifact.Id);
				graphicInfoList = connectionPerfectionizer(processDiagramCanvas, bpmnModel, sourceElement, targetElement, graphicInfoList);
				int[] xPoints = new int[graphicInfoList.Count];
				int[] yPoints = new int[graphicInfoList.Count];
				for (int i = 1; i < graphicInfoList.Count; i++)
				{
					GraphicInfo graphicInfo = graphicInfoList[i];
					GraphicInfo previousGraphicInfo = graphicInfoList[i - 1];

					if (i == 1)
					{
						xPoints[0] = (int) previousGraphicInfo.X;
						yPoints[0] = (int) previousGraphicInfo.Y;
					}
					xPoints[i] = (int) graphicInfo.X;
					yPoints[i] = (int) graphicInfo.Y;
				}

				AssociationDirection associationDirection = association.AssociationDirection;
				processDiagramCanvas.drawAssociation(xPoints, yPoints, associationDirection, false);
			}
		}

		public virtual System.IO.Stream generateDiagram(BpmnModel bpmnModel, IList<string> highLightedActivities, IList<string> highLightedFlows, string activityFontName, string labelFontName, string annotationFontName)
		{
			return generateDiagram(bpmnModel, highLightedActivities, highLightedFlows, activityFontName, labelFontName, annotationFontName, false, null);
		}

		public virtual System.IO.Stream generateDiagram(BpmnModel bpmnModel, IList<string> highLightedActivities, IList<string> highLightedFlows, string activityFontName, string labelFontName, string annotationFontName, bool generateDefaultDiagram)
		{
			return generateDiagram(bpmnModel, highLightedActivities, highLightedFlows, activityFontName, labelFontName, annotationFontName, generateDefaultDiagram, null);
		}

		public virtual System.IO.Stream generateDiagram(BpmnModel bpmnModel, IList<string> highLightedActivities, IList<string> highLightedFlows, string activityFontName, string labelFontName, string annotationFontName, bool generateDefaultDiagram, string defaultDiagramImageFileName)
		{

			if (!bpmnModel.hasDiagramInterchangeInfo())
			{
				if (!generateDefaultDiagram)
				{
					throw new ActivitiInterchangeInfoNotFoundException("No interchange information found.");
				}

				return getDefaultDiagram(defaultDiagramImageFileName);
			}

			return generateProcessDiagram(bpmnModel, highLightedActivities, highLightedFlows, activityFontName, labelFontName, annotationFontName).generateImage();
		}

		/// <summary>
		/// Get default diagram image as bytes array </summary>
		/// <returns> the default diagram image </returns>
		protected internal virtual System.IO.Stream getDefaultDiagram(string diagramImageFileName)
		{
			string imageFileName = !string.ReferenceEquals(diagramImageFileName, null) ? diagramImageFileName : DefaultDiagramImageFileName;
			System.IO.Stream imageStream = this.GetType().getResourceAsStream(imageFileName);
			if (imageStream == null)
			{
				throw new ActivitiImageException("Error occurred while getting default diagram image from file: " + imageFileName);
			}
			return imageStream;
		}

		public virtual System.IO.Stream generateDiagram(BpmnModel bpmnModel, IList<string> highLightedActivities, IList<string> highLightedFlows)
		{
			return generateDiagram(bpmnModel, highLightedActivities, highLightedFlows, null, null, null, false, null);
		}

		public virtual System.IO.Stream generateDiagram(BpmnModel bpmnModel, IList<string> highLightedActivities)
		{
			return generateDiagram(bpmnModel, highLightedActivities, System.Linq.Enumerable.Empty<string>());
		}

		public virtual System.IO.Stream generateDiagram(BpmnModel bpmnModel, string activityFontName, string labelFontName, string annotationFontName)
		{

			return generateDiagram(bpmnModel, System.Linq.Enumerable.Empty<string>(), System.Linq.Enumerable.Empty<string>(), activityFontName, labelFontName, annotationFontName);
		}

		protected internal virtual DefaultProcessDiagramCanvas generateProcessDiagram(BpmnModel bpmnModel, IList<string> highLightedActivities, IList<string> highLightedFlows, string activityFontName, string labelFontName, string annotationFontName)
		{

			prepareBpmnModel(bpmnModel);

			DefaultProcessDiagramCanvas processDiagramCanvas = initProcessDiagramCanvas(bpmnModel, activityFontName, labelFontName, annotationFontName);

			// Draw pool shape, if process is participant in collaboration
			foreach (Pool pool in bpmnModel.Pools)
			{
				GraphicInfo graphicInfo = bpmnModel.getGraphicInfo(pool.Id);
				processDiagramCanvas.drawPoolOrLane(pool.Id, pool.Name, graphicInfo);
			}

			// Draw lanes
			foreach (Process process in bpmnModel.Processes)
			{
				foreach (Lane lane in process.Lanes)
				{
					GraphicInfo graphicInfo = bpmnModel.getGraphicInfo(lane.Id);
					processDiagramCanvas.drawPoolOrLane(lane.Id, lane.Name, graphicInfo);
				}
			}

			// Draw activities and their sequence-flows
			foreach (Process process in bpmnModel.Processes)
			{
				foreach (FlowNode flowNode in process.findFlowElementsOfType(typeof(FlowNode)))
				{
					drawActivity(processDiagramCanvas, bpmnModel, flowNode, highLightedActivities, highLightedFlows);
				}
			}

			// Draw artifacts
			foreach (Process process in bpmnModel.Processes)
			{

				foreach (Artifact artifact in process.Artifacts)
				{
					drawArtifact(processDiagramCanvas, bpmnModel, artifact);
				}

				IList<SubProcess> subProcesses = process.findFlowElementsOfType(typeof(SubProcess), true);
				if (subProcesses != null)
				{
					foreach (SubProcess subProcess in subProcesses)
					{
						foreach (Artifact subProcessArtifact in subProcess.Artifacts)
						{
							drawArtifact(processDiagramCanvas, bpmnModel, subProcessArtifact);
						}
					}
				}
			}

			return processDiagramCanvas;
		}

		protected internal virtual void prepareBpmnModel(BpmnModel bpmnModel)
		{

			// Need to make sure all elements have positive x and y.
			// Check all graphicInfo and update the elements accordingly

			IList<GraphicInfo> allGraphicInfos = new List<GraphicInfo>();
			if (bpmnModel.LocationMap != null)
			{
				((List<GraphicInfo>)allGraphicInfos).AddRange(bpmnModel.LocationMap.Values);
			}
			if (bpmnModel.LabelLocationMap != null)
			{
				((List<GraphicInfo>)allGraphicInfos).AddRange(bpmnModel.LabelLocationMap.Values);
			}
			if (bpmnModel.FlowLocationMap != null)
			{
				foreach (IList<GraphicInfo> flowGraphicInfos in bpmnModel.FlowLocationMap.Values)
				{
					((List<GraphicInfo>)allGraphicInfos).AddRange(flowGraphicInfos);
				}
			}

			if (allGraphicInfos.Count > 0)
			{

				bool needsTranslationX = false;
				bool needsTranslationY = false;

				double lowestX = 0.0;
				double lowestY = 0.0;

				// Collect lowest x and y
				foreach (GraphicInfo graphicInfo in allGraphicInfos)
				{

					double x = graphicInfo.X;
					double y = graphicInfo.Y;

					if (x < lowestX)
					{
						needsTranslationX = true;
						lowestX = x;
					}
					if (y < lowestY)
					{
						needsTranslationY = true;
						lowestY = y;
					}
				}

				// Update all graphicInfo objects
				if (needsTranslationX || needsTranslationY)
				{

					double translationX = Math.Abs(lowestX);
					double translationY = Math.Abs(lowestY);

					foreach (GraphicInfo graphicInfo in allGraphicInfos)
					{
						if (needsTranslationX)
						{
							graphicInfo.X = graphicInfo.X + translationX;
						}
						if (needsTranslationY)
						{
							graphicInfo.Y = graphicInfo.Y + translationY;
						}
					}
				}
			}
		}

		protected internal virtual void drawActivity(DefaultProcessDiagramCanvas processDiagramCanvas, BpmnModel bpmnModel, FlowNode flowNode, IList<string> highLightedActivities, IList<string> highLightedFlows)
		{

			ActivityDrawInstruction drawInstruction = activityDrawInstructions[flowNode.GetType()];
			if (drawInstruction != null)
			{

				drawInstruction.draw(processDiagramCanvas, bpmnModel, flowNode);

				// Gather info on the multi instance marker
				bool multiInstanceSequential = false;
				bool multiInstanceParallel = false;
				bool collapsed = false;
				if (flowNode is Activity)
				{
					Activity activity = (Activity) flowNode;
					MultiInstanceLoopCharacteristics multiInstanceLoopCharacteristics = activity.LoopCharacteristics;
					if (multiInstanceLoopCharacteristics != null)
					{
						multiInstanceSequential = multiInstanceLoopCharacteristics.Sequential;
						multiInstanceParallel = !multiInstanceSequential;
					}
				}

				// Gather info on the collapsed marker
				GraphicInfo graphicInfo = bpmnModel.getGraphicInfo(flowNode.Id);
				if (flowNode is SubProcess)
				{
					collapsed = graphicInfo.Expanded != null && !graphicInfo.Expanded;
				}
				else if (flowNode is CallActivity)
				{
					collapsed = true;
				}

				// Actually draw the markers
				processDiagramCanvas.drawActivityMarkers((int) graphicInfo.X, (int) graphicInfo.Y, (int) graphicInfo.Width, (int) graphicInfo.Height, multiInstanceSequential, multiInstanceParallel, collapsed);

				// Draw highlighted activities
				if (highLightedActivities.Contains(flowNode.Id))
				{
					drawHighLight(processDiagramCanvas, bpmnModel.getGraphicInfo(flowNode.Id));
				}
			}

			// Outgoing transitions of activity
			foreach (SequenceFlow sequenceFlow in flowNode.OutgoingFlows)
			{
				bool highLighted = (highLightedFlows.Contains(sequenceFlow.Id));
				string defaultFlow = null;
				if (flowNode is Activity)
				{
					defaultFlow = ((Activity) flowNode).DefaultFlow;
				}
				else if (flowNode is Gateway)
				{
					defaultFlow = ((Gateway) flowNode).DefaultFlow;
				}

				bool isDefault = false;
				if (!string.ReferenceEquals(defaultFlow, null) && defaultFlow.Equals(sequenceFlow.Id, StringComparison.CurrentCultureIgnoreCase))
				{
					isDefault = true;
				}
				bool drawConditionalIndicator = !string.ReferenceEquals(sequenceFlow.ConditionExpression, null) && !(flowNode is Gateway);

				string sourceRef = sequenceFlow.SourceRef;
				string targetRef = sequenceFlow.TargetRef;
				FlowElement sourceElement = bpmnModel.getFlowElement(sourceRef);
				FlowElement targetElement = bpmnModel.getFlowElement(targetRef);
				IList<GraphicInfo> graphicInfoList = bpmnModel.getFlowLocationGraphicInfo(sequenceFlow.Id);
				if (graphicInfoList != null && graphicInfoList.Count > 0)
				{
					graphicInfoList = connectionPerfectionizer(processDiagramCanvas, bpmnModel, sourceElement, targetElement, graphicInfoList);
					int[] xPoints = new int[graphicInfoList.Count];
					int[] yPoints = new int[graphicInfoList.Count];

					for (int i = 1; i < graphicInfoList.Count; i++)
					{
						GraphicInfo graphicInfo = graphicInfoList[i];
						GraphicInfo previousGraphicInfo = graphicInfoList[i - 1];

						if (i == 1)
						{
							xPoints[0] = (int) previousGraphicInfo.X;
							yPoints[0] = (int) previousGraphicInfo.Y;
						}
						xPoints[i] = (int) graphicInfo.X;
						yPoints[i] = (int) graphicInfo.Y;
					}

					processDiagramCanvas.drawSequenceflow(xPoints, yPoints, drawConditionalIndicator, isDefault, highLighted);

					// Draw sequenceflow label
					GraphicInfo labelGraphicInfo = bpmnModel.getLabelGraphicInfo(sequenceFlow.Id);
					if (labelGraphicInfo != null)
					{
						processDiagramCanvas.drawLabel(sequenceFlow.Name, labelGraphicInfo, false);
					}
				}
			}

			// Nested elements
			if (flowNode is FlowElementsContainer)
			{
				foreach (FlowElement nestedFlowElement in ((FlowElementsContainer) flowNode).FlowElements)
				{
					if (nestedFlowElement is FlowNode)
					{
						drawActivity(processDiagramCanvas, bpmnModel, (FlowNode) nestedFlowElement, highLightedActivities, highLightedFlows);
					}
				}
			}
		}

		/// <summary>
		/// This method makes coordinates of connection flow better. </summary>
		/// <param name="processDiagramCanvas"> </param>
		/// <param name="bpmnModel"> </param>
		/// <param name="sourceElement"> </param>
		/// <param name="targetElement"> </param>
		/// <param name="graphicInfoList">
		/// @return </param>
		protected internal static IList<GraphicInfo> connectionPerfectionizer(DefaultProcessDiagramCanvas processDiagramCanvas, BpmnModel bpmnModel, BaseElement sourceElement, BaseElement targetElement, IList<GraphicInfo> graphicInfoList)
		{
			GraphicInfo sourceGraphicInfo = bpmnModel.getGraphicInfo(sourceElement.Id);
			GraphicInfo targetGraphicInfo = bpmnModel.getGraphicInfo(targetElement.Id);

			DefaultProcessDiagramCanvas.SHAPE_TYPE sourceShapeType = getShapeType(sourceElement);
			DefaultProcessDiagramCanvas.SHAPE_TYPE targetShapeType = getShapeType(targetElement);

			return processDiagramCanvas.connectionPerfectionizer(sourceShapeType, targetShapeType, sourceGraphicInfo, targetGraphicInfo, graphicInfoList);
		}

		/// <summary>
		/// This method returns shape type of base element.<br>
		/// Each element can be presented as rectangle, rhombus, or ellipse. </summary>
		/// <param name="baseElement"> </param>
		/// <returns> DefaultProcessDiagramCanvas.SHAPE_TYPE </returns>
		protected internal static DefaultProcessDiagramCanvas.SHAPE_TYPE getShapeType(BaseElement baseElement)
		{
			if (baseElement is Task || baseElement is Activity || baseElement is TextAnnotation)
			{
				return DefaultProcessDiagramCanvas.SHAPE_TYPE.Rectangle;
			}
			else if (baseElement is Gateway)
			{
				return DefaultProcessDiagramCanvas.SHAPE_TYPE.Rhombus;
			}
			else if (baseElement is Event)
			{
				return DefaultProcessDiagramCanvas.SHAPE_TYPE.Ellipse;
			}
			// unknown source element, just do not correct coordinates
			return null;
		}

		protected internal static GraphicInfo getLineCenter(IList<GraphicInfo> graphicInfoList)
		{
			GraphicInfo gi = new GraphicInfo();

			int[] xPoints = new int[graphicInfoList.Count];
			int[] yPoints = new int[graphicInfoList.Count];

			double length = 0;
			double[] lengths = new double[graphicInfoList.Count];
			lengths[0] = 0;
			double m;
			for (int i = 1; i < graphicInfoList.Count; i++)
			{
				GraphicInfo graphicInfo = graphicInfoList[i];
				GraphicInfo previousGraphicInfo = graphicInfoList[i - 1];

				if (i == 1)
				{
					xPoints[0] = (int) previousGraphicInfo.X;
					yPoints[0] = (int) previousGraphicInfo.Y;
				}
				xPoints[i] = (int) graphicInfo.X;
				yPoints[i] = (int) graphicInfo.Y;

				length += Math.Sqrt(Math.Pow((int) graphicInfo.X - (int) previousGraphicInfo.X, 2) + Math.Pow((int) graphicInfo.Y - (int) previousGraphicInfo.Y, 2));
				lengths[i] = length;
			}
			m = length / 2;
			int p1 = 0;
			int p2 = 1;
			for (int i = 1; i < lengths.Length; i++)
			{
				double len = lengths[i];
				p1 = i - 1;
				p2 = i;
				if (len > m)
				{
					break;
				}
			}

			GraphicInfo graphicInfo1 = graphicInfoList[p1];
			GraphicInfo graphicInfo2 = graphicInfoList[p2];

			double AB = (int) graphicInfo2.X - (int) graphicInfo1.X;
			double OA = (int) graphicInfo2.Y - (int) graphicInfo1.Y;
			double OB = lengths[p2] - lengths[p1];
			double ob = m - lengths[p1];
			double ab = AB * ob / OB;
			double oa = OA * ob / OB;

			double mx = graphicInfo1.X + ab;
			double my = graphicInfo1.Y + oa;

			gi.X = mx;
			gi.Y = my;
			return gi;
		}

		protected internal virtual void drawArtifact(DefaultProcessDiagramCanvas processDiagramCanvas, BpmnModel bpmnModel, Artifact artifact)
		{

			ArtifactDrawInstruction drawInstruction = artifactDrawInstructions[artifact.GetType()];
			if (drawInstruction != null)
			{
				drawInstruction.draw(processDiagramCanvas, bpmnModel, artifact);
			}
		}

		private static void drawHighLight(DefaultProcessDiagramCanvas processDiagramCanvas, GraphicInfo graphicInfo)
		{
			processDiagramCanvas.drawHighLight((int) graphicInfo.X, (int) graphicInfo.Y, (int) graphicInfo.Width, (int) graphicInfo.Height);
		}

		protected internal static DefaultProcessDiagramCanvas initProcessDiagramCanvas(BpmnModel bpmnModel, string activityFontName, string labelFontName, string annotationFontName)
		{

			// We need to calculate maximum values to know how big the image will be in its entirety
			double minX = double.MaxValue;
			double maxX = 0;
			double minY = double.MaxValue;
			double maxY = 0;

			foreach (Pool pool in bpmnModel.Pools)
			{
				GraphicInfo graphicInfo = bpmnModel.getGraphicInfo(pool.Id);
				minX = graphicInfo.X;
				maxX = graphicInfo.X + graphicInfo.Width;
				minY = graphicInfo.Y;
				maxY = graphicInfo.Y + graphicInfo.Height;
			}

			IList<FlowNode> flowNodes = gatherAllFlowNodes(bpmnModel);
			foreach (FlowNode flowNode in flowNodes)
			{

				GraphicInfo flowNodeGraphicInfo = bpmnModel.getGraphicInfo(flowNode.Id);

				if (flowNodeGraphicInfo == null)
				{
					continue;
				}

				// width
				if (flowNodeGraphicInfo.X + flowNodeGraphicInfo.Width > maxX)
				{
					maxX = flowNodeGraphicInfo.X + flowNodeGraphicInfo.Width;
				}
				if (flowNodeGraphicInfo.X < minX)
				{
					minX = flowNodeGraphicInfo.X;
				}
				// height
				if (flowNodeGraphicInfo.Y + flowNodeGraphicInfo.Height > maxY)
				{
					maxY = flowNodeGraphicInfo.Y + flowNodeGraphicInfo.Height;
				}
				if (flowNodeGraphicInfo.Y < minY)
				{
					minY = flowNodeGraphicInfo.Y;
				}

				foreach (SequenceFlow sequenceFlow in flowNode.OutgoingFlows)
				{
					IList<GraphicInfo> graphicInfoList = bpmnModel.getFlowLocationGraphicInfo(sequenceFlow.Id);
					if (graphicInfoList != null)
					{
						foreach (GraphicInfo graphicInfo in graphicInfoList)
						{
							// width
							if (graphicInfo.X > maxX)
							{
								maxX = graphicInfo.X;
							}
							if (graphicInfo.X < minX)
							{
								minX = graphicInfo.X;
							}
							// height
							if (graphicInfo.Y > maxY)
							{
								maxY = graphicInfo.Y;
							}
							if (graphicInfo.Y < minY)
							{
								minY = graphicInfo.Y;
							}
						}
					}
				}
			}

			IList<Artifact> artifacts = gatherAllArtifacts(bpmnModel);
			foreach (Artifact artifact in artifacts)
			{

				GraphicInfo artifactGraphicInfo = bpmnModel.getGraphicInfo(artifact.Id);

				if (artifactGraphicInfo != null)
				{
					// width
					if (artifactGraphicInfo.X + artifactGraphicInfo.Width > maxX)
					{
						maxX = artifactGraphicInfo.X + artifactGraphicInfo.Width;
					}
					if (artifactGraphicInfo.X < minX)
					{
						minX = artifactGraphicInfo.X;
					}
					// height
					if (artifactGraphicInfo.Y + artifactGraphicInfo.Height > maxY)
					{
						maxY = artifactGraphicInfo.Y + artifactGraphicInfo.Height;
					}
					if (artifactGraphicInfo.Y < minY)
					{
						minY = artifactGraphicInfo.Y;
					}
				}

				IList<GraphicInfo> graphicInfoList = bpmnModel.getFlowLocationGraphicInfo(artifact.Id);
				if (graphicInfoList != null)
				{
					foreach (GraphicInfo graphicInfo in graphicInfoList)
					{
						// width
						if (graphicInfo.X > maxX)
						{
							maxX = graphicInfo.X;
						}
						if (graphicInfo.X < minX)
						{
							minX = graphicInfo.X;
						}
						// height
						if (graphicInfo.Y > maxY)
						{
							maxY = graphicInfo.Y;
						}
						if (graphicInfo.Y < minY)
						{
							minY = graphicInfo.Y;
						}
					}
				}
			}

			int nrOfLanes = 0;
			foreach (Process process in bpmnModel.Processes)
			{
				foreach (Lane l in process.Lanes)
				{

					nrOfLanes++;

					GraphicInfo graphicInfo = bpmnModel.getGraphicInfo(l.Id);
					if (graphicInfo != null)
					{
						// width
						if (graphicInfo.X + graphicInfo.Width > maxX)
						{
							maxX = graphicInfo.X + graphicInfo.Width;
						}
						if (graphicInfo.X < minX)
						{
							minX = graphicInfo.X;
						}
						// height
						if (graphicInfo.Y + graphicInfo.Height > maxY)
						{
							maxY = graphicInfo.Y + graphicInfo.Height;
						}
						if (graphicInfo.Y < minY)
						{
							minY = graphicInfo.Y;
						}
					}
				}
			}

			// Special case, see https://activiti.atlassian.net/browse/ACT-1431
			if (flowNodes.Count == 0 && bpmnModel.Pools.Count == 0 && nrOfLanes == 0)
			{
				// Nothing to show
				minX = 0;
				minY = 0;
			}

			return new DefaultProcessDiagramCanvas((int) maxX + 10, (int) maxY + 10, (int) minX, (int) minY, activityFontName, labelFontName, annotationFontName);
		}

		protected internal static IList<Artifact> gatherAllArtifacts(BpmnModel bpmnModel)
		{
			IList<Artifact> artifacts = new List<Artifact>();
			foreach (Process process in bpmnModel.Processes)
			{
				((List<Artifact>)artifacts).AddRange(process.Artifacts);
			}
			return artifacts;
		}

		protected internal static IList<FlowNode> gatherAllFlowNodes(BpmnModel bpmnModel)
		{
			IList<FlowNode> flowNodes = new List<FlowNode>();
			foreach (Process process in bpmnModel.Processes)
			{
				((List<FlowNode>)flowNodes).AddRange(gatherAllFlowNodes(process));
			}
			return flowNodes;
		}

		protected internal static IList<FlowNode> gatherAllFlowNodes(FlowElementsContainer flowElementsContainer)
		{
			IList<FlowNode> flowNodes = new List<FlowNode>();
			foreach (FlowElement flowElement in flowElementsContainer.FlowElements)
			{
				if (flowElement is FlowNode)
				{
					flowNodes.Add((FlowNode) flowElement);
				}
				if (flowElement is FlowElementsContainer)
				{
					((List<FlowNode>)flowNodes).AddRange(gatherAllFlowNodes((FlowElementsContainer) flowElement));
				}
			}
			return flowNodes;
		}

		public virtual IDictionary<Type, ActivityDrawInstruction> ActivityDrawInstructions
		{
			get
			{
				return activityDrawInstructions;
			}
			set
			{
				this.activityDrawInstructions = value;
			}
		}


		public virtual IDictionary<Type, ArtifactDrawInstruction> ArtifactDrawInstructions
		{
			get
			{
				return artifactDrawInstructions;
			}
			set
			{
				this.artifactDrawInstructions = value;
			}
		}


		protected internal interface ActivityDrawInstruction
		{

			void draw(DefaultProcessDiagramCanvas processDiagramCanvas, BpmnModel bpmnModel, FlowNode flowNode);
		}

		protected internal interface ArtifactDrawInstruction
		{

			void draw(DefaultProcessDiagramCanvas processDiagramCanvas, BpmnModel bpmnModel, Artifact artifact);
		}
	}

}