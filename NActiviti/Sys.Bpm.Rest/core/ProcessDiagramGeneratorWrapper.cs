using Microsoft.Extensions.Logging;
using org.activiti.bpmn.model;
using org.activiti.engine;
using org.activiti.image;
using Sys.Workflow;
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
    public class ProcessDiagramGeneratorWrapper
    {
        private readonly ILogger logger = null;

        private readonly IProcessDiagramGenerator processDiagramGenerator;
        private readonly string activityFontName;
        private readonly string labelFontName;
        private readonly string annotationFontName;
        private readonly string defaultDiagramImageFileName;
        private readonly bool generateDefaultDiagram;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="processDiagramGenerator"></param>
        /// <param name="loggerFactory"></param>
        public ProcessDiagramGeneratorWrapper(IProcessDiagramGenerator processDiagramGenerator,
            ILoggerFactory loggerFactory)
        {
            this.processDiagramGenerator = processDiagramGenerator;
            logger = loggerFactory.CreateLogger<ProcessDiagramGeneratorWrapper>();
        }

        /// <summary>
        /// Generate the diagram for a BPNM model </summary>
        /// <param name="bpmnModel"> the BPNM model </param>
        /// <returns> the diagram for the given model </returns>
        public virtual byte[] GenerateDiagram(BpmnModel bpmnModel)
        {
            return GenerateDiagram(bpmnModel, new List<string>(), new List<string>());
        }

        /// <summary>
        /// Generate the diagram for a BPNM model </summary>
        /// <param name="bpmnModel"> the BPNM model </param>
        /// <param name="highLightedActivities"> the activity ids to highlight in diagram </param>
        /// <param name="highLightedFlows"> the flow ids to highlight in diagram </param>
        /// <returns> the diagram for the given model </returns>
        public virtual byte[] GenerateDiagram(BpmnModel bpmnModel, IList<string> highLightedActivities, IList<string> highLightedFlows)
        {
            try
            {
                using (Stream imageStream = processDiagramGenerator.GenerateDiagram(bpmnModel, highLightedActivities, highLightedFlows, ActivityFontName, LabelFontName, AnnotationFontName, GenerateDefaultDiagram, DiagramImageFileName))
                {
                    byte[] data = new byte[imageStream.Length];
                    imageStream.Read(data, 0, data.Length);
                    return data;
                }
            }
            catch (Exception e)
            {
                if (logger.IsEnabled(LogLevel.Debug))
                {
                    logger.LogDebug("Error occurred while getting process diagram for model: " + bpmnModel);
                }
                throw new ActivitiException("Error occurred while getting process diagram for model: " + bpmnModel, e);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual bool GenerateDefaultDiagram
        {
            get
            {
                return generateDefaultDiagram;
            }
        }

        /// <summary>
        /// 
        /// </summary>
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
                return IsFontAvailable(activityFontName) ? activityFontName : processDiagramGenerator.DefaultActivityFontName;
            }
        }

        /// <summary>
        /// Get label font name </summary>
        /// <returns> the label font name </returns>
        public virtual string LabelFontName
        {
            get
            {
                return IsFontAvailable(labelFontName) ? labelFontName : processDiagramGenerator.DefaultLabelFontName;
            }
        }

        /// <summary>
        /// Get annotation font name </summary>
        /// <returns> the annotation font name </returns>
        public virtual string AnnotationFontName
        {
            get
            {
                return IsFontAvailable(annotationFontName) ? annotationFontName : processDiagramGenerator.DefaultAnnotationFontName;
            }
        }

        /// <summary>
        /// Check if a given font is available in the current system </summary>
        /// <param name="fontName"> the font name to check </param>
        /// <returns> true if the specified font name exists </returns>
        private bool IsFontAvailable(string fontName)
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

        /// <summary>
        /// 
        /// </summary>
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