using Microsoft.Extensions.Logging;
using org.activiti.bpmn.model;
using org.activiti.engine;
using org.activiti.image;
using Sys;
using System;
using System.Collections.Generic;
using System.IO;

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

namespace org.activiti.cloud.services.core
{
    /// <summary>
    /// Service logic for generating process diagrams
    /// </summary>
    //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
    //ORIGINAL LINE: @Service public class ProcessDiagramGeneratorWrapper
    public class ProcessDiagramGeneratorWrapper
    {
        private static readonly ILogger LOGGER = ProcessEngineServiceProvider.LoggerService<ProcessDiagramGeneratorWrapper>();

        private readonly IProcessDiagramGenerator processDiagramGenerator;

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @Value("${activiti.diagram.activity.font:}") private String activityFontName;
        private string activityFontName;

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @Value("${activiti.diagram.label.font:}") private String labelFontName;
        private string labelFontName;

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @Value("${activiti.diagram.annotation.font:}") private String annotationFontName;
        private string annotationFontName;

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @Value("${activiti.diagram.default.image.file:}") private String defaultDiagramImageFileName;
        private string defaultDiagramImageFileName;

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @Value("${activiti.diagram.generate.default:false}") private boolean generateDefaultDiagram;
        private bool generateDefaultDiagram;

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @Autowired public ProcessDiagramGeneratorWrapper(org.activiti.image.ProcessDiagramGenerator processDiagramGenerator)
        public ProcessDiagramGeneratorWrapper(IProcessDiagramGenerator processDiagramGenerator)
        {
            this.processDiagramGenerator = processDiagramGenerator;
        }

        /// <summary>
        /// Generate the diagram for a BPNM model </summary>
        /// <param name="bpmnModel"> the BPNM model </param>
        /// <returns> the diagram for the given model </returns>
        public virtual byte[] generateDiagram(BpmnModel bpmnModel)
        {
            return generateDiagram(bpmnModel, new List<string>(), new List<string>());
        }

        /// <summary>
        /// Generate the diagram for a BPNM model </summary>
        /// <param name="bpmnModel"> the BPNM model </param>
        /// <param name="highLightedActivities"> the activity ids to highlight in diagram </param>
        /// <param name="highLightedFlows"> the flow ids to highlight in diagram </param>
        /// <returns> the diagram for the given model </returns>
        public virtual byte[] generateDiagram(BpmnModel bpmnModel, IList<string> highLightedActivities, IList<string> highLightedFlows)
        {
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: try (final java.io.InputStream imageStream = processDiagramGenerator.generateDiagram(bpmnModel, highLightedActivities, highLightedFlows, getActivityFontName(), getLabelFontName(), getAnnotationFontName(), isGenerateDefaultDiagram(), getDiagramImageFileName()))
            try
            {
                using (Stream imageStream = processDiagramGenerator.generateDiagram(bpmnModel, highLightedActivities, highLightedFlows, ActivityFontName, LabelFontName, AnnotationFontName, GenerateDefaultDiagram, DiagramImageFileName))
                {
                    byte[] data = new byte[imageStream.Length];
                    imageStream.Read(data, 0, data.Length);
                    return data;
                }
            }
            catch (Exception e)
            {
                throw new ActivitiException("Error occurred while getting process diagram for model: " + bpmnModel, e);
            }
        }

        public virtual bool GenerateDefaultDiagram
        {
            get
            {
                return generateDefaultDiagram;
            }
        }

        public virtual string DefaultDiagramImageFileName
        {
            get
            {
                return defaultDiagramImageFileName;
            }
        }

        /// <summary>
        /// Get diagram file name to use when there is no diagram graphic info inside model. </summary>
        /// <returns> the file name </returns>
        public virtual string DiagramImageFileName
        {
            get
            {
                return string.IsNullOrWhiteSpace(DefaultDiagramImageFileName) ? DefaultDiagramImageFileName : processDiagramGenerator.DefaultDiagramImageFileName;
            }
        }

        /// <summary>
        /// Get activity font name </summary>
        /// <returns> the activity font name </returns>
        public virtual string ActivityFontName
        {
            get
            {
                return isFontAvailable(activityFontName) ? activityFontName : processDiagramGenerator.DefaultActivityFontName;
            }
        }

        /// <summary>
        /// Get label font name </summary>
        /// <returns> the label font name </returns>
        public virtual string LabelFontName
        {
            get
            {
                return isFontAvailable(labelFontName) ? labelFontName : processDiagramGenerator.DefaultLabelFontName;
            }
        }

        /// <summary>
        /// Get annotation font name </summary>
        /// <returns> the annotation font name </returns>
        public virtual string AnnotationFontName
        {
            get
            {
                return isFontAvailable(annotationFontName) ? annotationFontName : processDiagramGenerator.DefaultAnnotationFontName;
            }
        }

        /// <summary>
        /// Check if a given font is available in the current system </summary>
        /// <param name="fontName"> the font name to check </param>
        /// <returns> true if the specified font name exists </returns>
        private bool isFontAvailable(string fontName)
        {
            if (string.IsNullOrWhiteSpace(fontName))
            {
                return false;
            }

            //bool available = java.util.AvailableFonts.Any(availbleFontName => availbleFontName.ToLower().StartsWith(fontName.ToLower(), StringComparison.Ordinal));

            //if (!available)
            //{
            //    LOGGER.debug("Font not available while generating process diagram: " + fontName);
            //}

            return true;// available;
        }

        protected internal virtual string[] AvailableFonts
        {
            get
            {
                throw new NotImplementedException();
                //return LocalGraphicsEnvironment.AvailableFontFamilyNames;
            }
        }
    }

}