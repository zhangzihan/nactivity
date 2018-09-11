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
namespace org.activiti.image
{

    using org.activiti.bpmn.model;

    /// <summary>
    /// This interface declares methods to generate process diagram
    /// </summary>
    public interface IProcessDiagramGenerator
    {
        /// <summary>
        /// Generates a diagram of the given process definition, using the diagram interchange information of the process.
        /// If there is no interchange information available, an ActivitiInterchangeInfoNotFoundException is thrown. </summary>
        /// <param name="bpmnModel"> bpmn model to get diagram for </param>
        /// <param name="highLightedActivities"> activities to highlight </param>
        /// <param name="highLightedFlows"> flows to highlight </param>
        /// <param name="activityFontName"> override the default activity font </param>
        /// <param name="labelFontName"> override the default label font </param>
        System.IO.Stream generateDiagram(BpmnModel bpmnModel, IList<string> highLightedActivities, IList<string> highLightedFlows, string activityFontName, string labelFontName, string annotationFontName);

        /// <summary>
        /// Generates a diagram of the given process definition, using the diagram interchange information of the process,
        /// or the default diagram image, if generateDefaultDiagram param is true. </summary>
        /// <param name="bpmnModel"> bpmn model to get diagram for </param>
        /// <param name="highLightedActivities"> activities to highlight </param>
        /// <param name="highLightedFlows"> flows to highlight </param>
        /// <param name="activityFontName"> override the default activity font </param>
        /// <param name="labelFontName"> override the default label font </param>
        /// <param name="generateDefaultDiagram"> true if a default diagram should be generated if there is no graphic info available </param>
        System.IO.Stream generateDiagram(BpmnModel bpmnModel, IList<string> highLightedActivities, IList<string> highLightedFlows, string activityFontName, string labelFontName, string annotationFontName, bool generateDefaultDiagram);

        /// <summary>
        /// Generates a diagram of the given process definition, using the diagram interchange information of the process,
        /// or the default diagram image, if generateDefaultDiagram param is true. </summary>
        /// <param name="bpmnModel"> bpmn model to get diagram for </param>
        /// <param name="highLightedActivities"> activities to highlight </param>
        /// <param name="highLightedFlows"> flows to highlight </param>
        /// <param name="activityFontName"> override the default activity font </param>
        /// <param name="labelFontName"> override the default label font </param>
        /// <param name="generateDefaultDiagram"> true if a default diagram should be generated if there is no graphic info available </param>
        /// <param name="defaultDiagramImageFileName"> override the default diagram image file name </param>
        System.IO.Stream generateDiagram(BpmnModel bpmnModel, IList<string> highLightedActivities, IList<string> highLightedFlows, string activityFontName, string labelFontName, string annotationFontName, bool generateDefaultDiagram, string defaultDiagramImageFileName);

        /// <summary>
        /// Generates a diagram of the given process definition, using the diagram interchange information of the process.
        /// If there is no interchange information available, an ActivitiInterchangeInfoNotFoundException is thrown. </summary>
        /// <param name="bpmnModel"> bpmn model to get diagram for </param>
        /// <param name="highLightedActivities"> activities to highlight </param>
        /// <param name="highLightedFlows"> flows to highlight </param>
        System.IO.Stream generateDiagram(BpmnModel bpmnModel, IList<string> highLightedActivities, IList<string> highLightedFlows);

        /// <summary>
        /// Generates a diagram of the given process definition, using the diagram interchange information of the process.
        /// If there is no interchange information available, an ActivitiInterchangeInfoNotFoundException is thrown. </summary>
        /// <param name="bpmnModel"> bpmn model to get diagram for </param>
        /// <param name="highLightedActivities"> activities to highlight </param>
        System.IO.Stream generateDiagram(BpmnModel bpmnModel, IList<string> highLightedActivities);

        /// <summary>
        /// Generates a diagram of the given process definition, using the diagram interchange information of the process.
        /// If there is no interchange information available, an ActivitiInterchangeInfoNotFoundException is thrown. </summary>
        /// <param name="bpmnModel"> bpmn model to get diagram for </param>
        System.IO.Stream generateDiagram(BpmnModel bpmnModel, string activityFontName, string labelFontName, string annotationFontName);

        string DefaultActivityFontName { get; }

        string DefaultLabelFontName { get; }

        string DefaultAnnotationFontName { get; }

        string DefaultDiagramImageFileName { get; }
    }
}