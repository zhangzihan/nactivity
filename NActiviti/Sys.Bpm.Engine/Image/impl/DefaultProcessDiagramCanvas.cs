using System;
using System.Collections.Generic;
using System.Text;

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

namespace org.activiti.image.impl
{

    using org.activiti.bpmn.model;
    using org.activiti.bpmn.model;
    using org.activiti.image.exception;
    using org.activiti.image.impl.icon;
    using org.activiti.image.impl.icon;
    using org.activiti.image.impl.icon;
    using org.activiti.image.impl.icon;
    using org.activiti.image.impl.icon;
    using org.activiti.image.impl.icon;
    using org.activiti.image.impl.icon;
    using org.activiti.image.impl.icon;
    using org.activiti.image.impl.icon;
    using org.activiti.image.impl.icon;
    using org.activiti.image.impl.icon;
    using org.activiti.image.impl.icon;
    using org.activiti.image.impl.icon;
    using org.activiti.image.impl.icon;
    using org.activiti.image.impl.icon;
    using org.activiti.image.impl.icon;
    using org.activiti.image.impl.icon;
    using org.apache.batik.dom;
    using org.apache.batik.svggen;
    using org.slf4j;
    using org.slf4j;
    using org.w3c.dom;
    using org.w3c.dom;

    /// <summary>
    /// Represents a canvas on which BPMN 2.0 constructs can be drawn.
    /// <para>
    /// </para>
    /// </summary>
    /// <seealso cref= org.activiti.image.impl.DefaultProcessDiagramGenerator </seealso>
    public class DefaultProcessDiagramCanvas
    {

        protected internal static readonly Logger LOGGER = LoggerFactory.getLogger(typeof(DefaultProcessDiagramCanvas));

        public enum SHAPE_TYPE
        {
            java.awt.Rectangle,
            Rhombus,
            Ellipse
        }

        // Predefined sized
        protected internal const int ARROW_WIDTH = 5;
        protected internal const int CONDITIONAL_INDICATOR_WIDTH = 16;
        protected internal const int DEFAULT_INDICATOR_WIDTH = 10;
        protected internal const int MARKER_WIDTH = 12;
        protected internal const int FONT_SIZE = 11;
        protected internal const int FONT_SPACING = 2;
        protected internal const int TEXT_PADDING = 3;
        protected internal const int ANNOTATION_TEXT_PADDING = 7;
        protected internal static readonly int LINE_HEIGHT = FONT_SIZE + FONT_SPACING;

        // Colors
        protected internal static Color TASK_BOX_COLOR = new Color(249, 249, 249);
        protected internal static Color SUBPROCESS_BOX_COLOR = new Color(255, 255, 255);
        protected internal static Color EVENT_COLOR = new Color(255, 255, 255);
        protected internal static Color CONNECTION_COLOR = new Color(88, 88, 88);
        protected internal static Color CONDITIONAL_INDICATOR_COLOR = new Color(255, 255, 255);
        protected internal static Color HIGHLIGHT_COLOR = Color.RED;
        protected internal static Color LABEL_COLOR = new Color(112, 146, 190);
        protected internal static Color TASK_BORDER_COLOR = new Color(187, 187, 187);
        protected internal static Color EVENT_BORDER_COLOR = new Color(88, 88, 88);
        protected internal static Color SUBPROCESS_BORDER_COLOR = new Color(0, 0, 0);

        // Fonts
        protected internal static Font LABEL_FONT = null;
        protected internal static Font ANNOTATION_FONT = null;

        // Strokes
        protected internal static Stroke THICK_TASK_BORDER_STROKE = new BasicStroke(3.0f);
        protected internal static Stroke GATEWAY_TYPE_STROKE = new BasicStroke(3.0f);
        protected internal static Stroke END_EVENT_STROKE = new BasicStroke(3.0f);
        protected internal static Stroke MULTI_INSTANCE_STROKE = new BasicStroke(1.3f);
        protected internal static Stroke EVENT_SUBPROCESS_STROKE = new BasicStroke(1.0f, BasicStroke.CAP_BUTT, BasicStroke.JOIN_MITER, 1.0f, new float[] { 1.0f }, 0.0f);
        protected internal static Stroke NON_INTERRUPTING_EVENT_STROKE = new BasicStroke(1.0f, BasicStroke.CAP_BUTT, BasicStroke.JOIN_MITER, 1.0f, new float[] { 4.0f, 3.0f }, 0.0f);
        protected internal static Stroke HIGHLIGHT_FLOW_STROKE = new BasicStroke(1.3f);
        protected internal static Stroke ANNOTATION_STROKE = new BasicStroke(2.0f);
        protected internal static Stroke ASSOCIATION_STROKE = new BasicStroke(2.0f, BasicStroke.CAP_BUTT, BasicStroke.JOIN_MITER, 1.0f, new float[] { 2.0f, 2.0f }, 0.0f);

        // icons
        protected internal static int ICON_PADDING = 5;
        protected internal static TaskIconType USERTASK_IMAGE;
        protected internal static TaskIconType SCRIPTTASK_IMAGE;
        protected internal static TaskIconType SERVICETASK_IMAGE;
        protected internal static TaskIconType RECEIVETASK_IMAGE;
        protected internal static TaskIconType SENDTASK_IMAGE;
        protected internal static TaskIconType MANUALTASK_IMAGE;
        protected internal static TaskIconType BUSINESS_RULE_TASK_IMAGE;

        protected internal static IconType TIMER_IMAGE;
        protected internal static IconType COMPENSATE_THROW_IMAGE;
        protected internal static IconType COMPENSATE_CATCH_IMAGE;
        protected internal static IconType ERROR_THROW_IMAGE;
        protected internal static IconType ERROR_CATCH_IMAGE;
        protected internal static IconType MESSAGE_CATCH_IMAGE;
        protected internal static IconType SIGNAL_CATCH_IMAGE;
        protected internal static IconType SIGNAL_THROW_IMAGE;

        protected internal int canvasWidth = -1;
        protected internal int canvasHeight = -1;
        protected internal int minX = -1;
        protected internal int minY = -1;
        protected internal ProcessDiagramSVGGraphics2D g;
        protected internal FontMetrics fontMetrics;
        protected internal bool closed;
        protected internal string activityFontName = "Arial";
        protected internal string labelFontName = "Arial";
        protected internal string annotationFontName = "Arial";

        /// <summary>
        /// Creates an empty canvas with given width and height.
        /// <para>
        /// Allows to specify minimal boundaries on the left and upper side of the
        /// canvas. This is useful for diagrams that have white space there.
        /// Everything beneath these minimum values will be cropped.
        /// It's also possible to pass a specific font name and a class loader for the icon images.
        /// </para>
        /// </summary>
        public DefaultProcessDiagramCanvas(int width, int height, int minX, int minY, string activityFontName, string labelFontName, string annotationFontName)
        {

            this.canvasWidth = width;
            this.canvasHeight = height;
            this.minX = minX;
            this.minY = minY;
            if (!string.ReferenceEquals(activityFontName, null))
            {
                this.activityFontName = activityFontName;
            }
            if (!string.ReferenceEquals(labelFontName, null))
            {
                this.labelFontName = labelFontName;
            }
            if (!string.ReferenceEquals(annotationFontName, null))
            {
                this.annotationFontName = annotationFontName;
            }

            initialize();
        }

        /// <summary>
        /// Creates an empty canvas with given width and height.
        /// <para>
        /// Allows to specify minimal boundaries on the left and upper side of the
        /// canvas. This is useful for diagrams that have white space there (eg
        /// Signavio). Everything beneath these minimum values will be cropped.
        /// </para>
        /// </summary>
        /// <param name="minX"> Hint that will be used when generating the image. Parts that fall
        /// below minX on the horizontal scale will be cropped. </param>
        /// <param name="minY"> Hint that will be used when generating the image. Parts that fall
        /// below minX on the horizontal scale will be cropped. </param>
        public DefaultProcessDiagramCanvas(int width, int height, int minX, int minY)
        {
            this.canvasWidth = width;
            this.canvasHeight = height;
            this.minX = minX;
            this.minY = minY;

            initialize();
        }

        public virtual void initialize()
        {
            // Get a DOMImplementation.
            DOMImplementation domImpl = GenericDOMImplementation.DOMImplementation;

            // Create an instance of org.w3c.dom.Document.
            string svgNS = "http://www.w3.org/2000/svg";
            Document document = domImpl.createDocument(svgNS, "svg", null);

            // Create an instance of the SVG Generator.
            this.g = new ProcessDiagramSVGGraphics2D(document);

            this.g.SVGCanvasSize = new Dimension(this.canvasWidth, this.canvasHeight);

            this.g.Background = new Color(255, 255, 255, 0);
            this.g.clearRect(0, 0, canvasWidth, canvasHeight);

            g.setRenderingHint(RenderingHints.KEY_ANTIALIASING, RenderingHints.VALUE_ANTIALIAS_ON);
            g.Paint = Color.black;

            Font font = new Font(activityFontName, Font.BOLD, FONT_SIZE);
            g.Font = font;
            this.fontMetrics = g.FontMetrics;

            LABEL_FONT = new Font(labelFontName, Font.ITALIC, 10);
            ANNOTATION_FONT = new Font(annotationFontName, Font.PLAIN, FONT_SIZE);

            USERTASK_IMAGE = new UserTaskIconType();
            SCRIPTTASK_IMAGE = new ScriptTaskIconType();
            SERVICETASK_IMAGE = new ServiceTaskIconType();
            RECEIVETASK_IMAGE = new ReceiveTaskIconType();
            SENDTASK_IMAGE = new SendTaskIconType();
            MANUALTASK_IMAGE = new ManualTaskIconType();
            BUSINESS_RULE_TASK_IMAGE = new BusinessRuleTaskIconType();

            TIMER_IMAGE = new TimerIconType();
            COMPENSATE_THROW_IMAGE = new CompensateThrowIconType();
            COMPENSATE_CATCH_IMAGE = new CompensateIconType();
            ERROR_THROW_IMAGE = new ErrorThrowIconType();
            ERROR_CATCH_IMAGE = new ErrorIconType();
            MESSAGE_CATCH_IMAGE = new MessageIconType();
            SIGNAL_THROW_IMAGE = new SignalThrowIconType();
            SIGNAL_CATCH_IMAGE = new SignalIconType();
        }

        /// <summary>
        /// Generates an image of what currently is drawn on the canvas.
        /// <para>
        /// Throws an <seealso cref="ActivitiImageException"/> when <seealso cref="#close()"/> is already
        /// called.
        /// </para>
        /// </summary>
        public virtual System.IO.Stream generateImage()
        {
            if (closed)
            {
                throw new ActivitiImageException("ProcessDiagramGenerator already closed");
            }

            try
            {
                System.IO.MemoryStream stream = new System.IO.MemoryStream();
                Writer @out;
                @out = new System.IO.StreamWriter(stream, Encoding.UTF8);
                g.stream(@out, true);
                return new System.IO.MemoryStream(stream.toByteArray());
            }
            catch (Exception e) when (e is UnsupportedEncodingException || e is SVGGraphics2DIOException)
            {
                throw new ActivitiImageException("Error while generating process image", e);
            }
        }

        /// <summary>
        /// Closes the canvas which dissallows further drawing and releases graphical
        /// resources.
        /// </summary>
        public virtual void close()
        {
            g.dispose();
            closed = true;
        }

        public virtual void drawNoneStartEvent(string id, GraphicInfo graphicInfo)
        {
            drawStartEvent(id, graphicInfo, null);
        }

        public virtual void drawTimerStartEvent(string id, GraphicInfo graphicInfo)
        {
            drawStartEvent(id, graphicInfo, TIMER_IMAGE);
        }

        public virtual void drawSignalStartEvent(string id, GraphicInfo graphicInfo)
        {
            drawStartEvent(id, graphicInfo, SIGNAL_CATCH_IMAGE);
        }

        public virtual void drawMessageStartEvent(string id, GraphicInfo graphicInfo)
        {
            drawStartEvent(id, graphicInfo, MESSAGE_CATCH_IMAGE);
        }

        public virtual void drawStartEvent(string id, GraphicInfo graphicInfo, IconType icon)
        {
            Paint originalPaint = g.Paint;
            g.Paint = EVENT_COLOR;
            Ellipse2D circle = new Ellipse2D.Double(graphicInfo.X, graphicInfo.Y, graphicInfo.Width, graphicInfo.Height);
            g.fill(circle);
            g.Paint = EVENT_BORDER_COLOR;
            g.draw(circle);
            g.Paint = originalPaint;

            // calculate coordinates to center image
            if (icon != null)
            {
                int imageX = (int)Math.Round(graphicInfo.X + (graphicInfo.Width / 2) - (icon.Width / 2));
                int imageY = (int)Math.Round(graphicInfo.Y + (graphicInfo.Height / 2) - (icon.Height / 2));

                icon.drawIcon(imageX, imageY, ICON_PADDING, g);
            }

            // set element's id
            g.CurrentGroupId = id;
        }

        public virtual void drawNoneEndEvent(string id, string name, GraphicInfo graphicInfo)
        {
            Paint originalPaint = g.Paint;
            Stroke originalStroke = g.Stroke;
            g.Paint = EVENT_COLOR;
            Ellipse2D circle = new Ellipse2D.Double(graphicInfo.X, graphicInfo.Y, graphicInfo.Width, graphicInfo.Height);
            g.fill(circle);
            g.Paint = EVENT_BORDER_COLOR;
            g.Stroke = END_EVENT_STROKE;
            g.draw(circle);
            g.Stroke = originalStroke;
            g.Paint = originalPaint;

            // set element's id
            g.CurrentGroupId = id;

            drawLabel(name, graphicInfo);
        }

        public virtual void drawErrorEndEvent(string id, string name, GraphicInfo graphicInfo)
        {
            drawNoneEndEvent(id, name, graphicInfo);

            int imageX = (int)(graphicInfo.X + (graphicInfo.Width / 4));
            int imageY = (int)(graphicInfo.Y + (graphicInfo.Height / 4));

            ERROR_THROW_IMAGE.drawIcon(imageX, imageY, ICON_PADDING, g);
        }

        public virtual void drawErrorStartEvent(string id, GraphicInfo graphicInfo)
        {
            drawNoneStartEvent(id, graphicInfo);

            int imageX = (int)(graphicInfo.X + (graphicInfo.Width / 4));
            int imageY = (int)(graphicInfo.Y + (graphicInfo.Height / 4));

            ERROR_THROW_IMAGE.drawIcon(imageX, imageY, ICON_PADDING, g);
        }

        public virtual void drawCatchingEvent(string id, GraphicInfo graphicInfo, bool isInterrupting, IconType icon, string eventType)
        {

            // event circles
            Ellipse2D outerCircle = new Ellipse2D.Double(graphicInfo.X, graphicInfo.Y, graphicInfo.Width, graphicInfo.Height);
            int innerCircleSize = 4;
            int innerCircleX = (int)graphicInfo.X + innerCircleSize;
            int innerCircleY = (int)graphicInfo.Y + innerCircleSize;
            int innerCircleWidth = (int)graphicInfo.Width - (2 * innerCircleSize);
            int innerCircleHeight = (int)graphicInfo.Height - (2 * innerCircleSize);
            Ellipse2D innerCircle = new Ellipse2D.Double(innerCircleX, innerCircleY, innerCircleWidth, innerCircleHeight);

            Paint originalPaint = g.Paint;
            Stroke originalStroke = g.Stroke;
            g.Paint = EVENT_COLOR;
            g.fill(outerCircle);

            g.Paint = EVENT_BORDER_COLOR;
            if (!isInterrupting)
            {
                g.Stroke = NON_INTERRUPTING_EVENT_STROKE;
            }
            g.draw(outerCircle);
            g.Stroke = originalStroke;
            g.Paint = originalPaint;
            g.draw(innerCircle);

            if (icon != null)
            {
                // calculate coordinates to center image
                int imageX = (int)(graphicInfo.X + (graphicInfo.Width / 2) - (icon.Width / 2));
                int imageY = (int)(graphicInfo.Y + (graphicInfo.Height / 2) - (icon.Height / 2));
                if ("timer".Equals(eventType))
                {
                    // move image one pixel to center timer image
                    imageX++;
                    imageY++;
                }
                icon.drawIcon(imageX, imageY, ICON_PADDING, g);
            }

            // set element's id
            g.CurrentGroupId = id;
        }

        public virtual void drawCatchingCompensateEvent(string id, string name, GraphicInfo graphicInfo, bool isInterrupting)
        {
            drawCatchingCompensateEvent(id, graphicInfo, isInterrupting);
            drawLabel(name, graphicInfo);
        }

        public virtual void drawCatchingCompensateEvent(string id, GraphicInfo graphicInfo, bool isInterrupting)
        {

            drawCatchingEvent(id, graphicInfo, isInterrupting, COMPENSATE_CATCH_IMAGE, "compensate");
        }

        public virtual void drawCatchingTimerEvent(string id, string name, GraphicInfo graphicInfo, bool isInterrupting)
        {
            drawCatchingTimerEvent(id, graphicInfo, isInterrupting);
            drawLabel(name, graphicInfo);
        }

        public virtual void drawCatchingTimerEvent(string id, GraphicInfo graphicInfo, bool isInterrupting)
        {
            drawCatchingEvent(id, graphicInfo, isInterrupting, TIMER_IMAGE, "timer");
        }

        public virtual void drawCatchingErrorEvent(string id, string name, GraphicInfo graphicInfo, bool isInterrupting)
        {
            drawCatchingErrorEvent(id, graphicInfo, isInterrupting);
            drawLabel(name, graphicInfo);
        }

        public virtual void drawCatchingErrorEvent(string id, GraphicInfo graphicInfo, bool isInterrupting)
        {

            drawCatchingEvent(id, graphicInfo, isInterrupting, ERROR_CATCH_IMAGE, "error");
        }

        public virtual void drawCatchingSignalEvent(string id, string name, GraphicInfo graphicInfo, bool isInterrupting)
        {
            drawCatchingSignalEvent(id, graphicInfo, isInterrupting);
            drawLabel(name, graphicInfo);
        }

        public virtual void drawCatchingSignalEvent(string id, GraphicInfo graphicInfo, bool isInterrupting)
        {
            drawCatchingEvent(id, graphicInfo, isInterrupting, SIGNAL_CATCH_IMAGE, "signal");
        }

        public virtual void drawCatchingMessageEvent(string id, GraphicInfo graphicInfo, bool isInterrupting)
        {

            drawCatchingEvent(id, graphicInfo, isInterrupting, MESSAGE_CATCH_IMAGE, "message");
        }

        public virtual void drawCatchingMessageEvent(string id, string name, GraphicInfo graphicInfo, bool isInterrupting)
        {
            drawCatchingEvent(id, graphicInfo, isInterrupting, MESSAGE_CATCH_IMAGE, "message");

            drawLabel(name, graphicInfo);
        }

        public virtual void drawThrowingCompensateEvent(string id, GraphicInfo graphicInfo)
        {
            drawCatchingEvent(id, graphicInfo, true, COMPENSATE_THROW_IMAGE, "compensate");
        }

        public virtual void drawThrowingSignalEvent(string id, GraphicInfo graphicInfo)
        {
            drawCatchingEvent(id, graphicInfo, true, SIGNAL_THROW_IMAGE, "signal");
        }

        public virtual void drawThrowingNoneEvent(string id, GraphicInfo graphicInfo)
        {
            drawCatchingEvent(id, graphicInfo, true, null, "none");
        }

        public virtual void drawSequenceflow(int srcX, int srcY, int targetX, int targetY, bool conditional)
        {
            drawSequenceflow(srcX, srcY, targetX, targetY, conditional, false);
        }

        public virtual void drawSequenceflow(int srcX, int srcY, int targetX, int targetY, bool conditional, bool highLighted)
        {
            Paint originalPaint = g.Paint;
            if (highLighted)
            {
                g.Paint = HIGHLIGHT_COLOR;
            }

            Line2D.Double line = new Line2D.Double(srcX, srcY, targetX, targetY);
            g.draw(line);
            drawArrowHead(line);

            if (conditional)
            {
                drawConditionalSequenceFlowIndicator(line);
            }

            if (highLighted)
            {
                g.Paint = originalPaint;
            }
        }

        public virtual void drawAssociation(int[] xPoints, int[] yPoints, AssociationDirection associationDirection, bool highLighted)
        {
            bool conditional = false;
            bool isDefault = false;
            drawConnection(xPoints, yPoints, conditional, isDefault, "association", associationDirection, highLighted);
        }

        public virtual void drawSequenceflow(int[] xPoints, int[] yPoints, bool conditional, bool isDefault, bool highLighted)
        {
            drawConnection(xPoints, yPoints, conditional, isDefault, "sequenceFlow", AssociationDirection.ONE, highLighted);
        }

        public virtual void drawConnection(int[] xPoints, int[] yPoints, bool conditional, bool isDefault, string connectionType, AssociationDirection associationDirection, bool highLighted)
        {

            Paint originalPaint = g.Paint;
            Stroke originalStroke = g.Stroke;

            g.Paint = CONNECTION_COLOR;
            if ("association".Equals(connectionType))
            {
                g.Stroke = ASSOCIATION_STROKE;
            }
            else if (highLighted)
            {
                g.Paint = HIGHLIGHT_COLOR;
                g.Stroke = HIGHLIGHT_FLOW_STROKE;
            }

            for (int i = 1; i < xPoints.Length; i++)
            {
                int? sourceX = xPoints[i - 1];
                int? sourceY = yPoints[i - 1];
                int? targetX = xPoints[i];
                int? targetY = yPoints[i];
                Line2D.Double line = new Line2D.Double(sourceX, sourceY, targetX, targetY);
                g.draw(line);
            }

            if (isDefault)
            {
                Line2D.Double line = new Line2D.Double(xPoints[0], yPoints[0], xPoints[1], yPoints[1]);
                drawDefaultSequenceFlowIndicator(line);
            }

            if (conditional)
            {
                Line2D.Double line = new Line2D.Double(xPoints[0], yPoints[0], xPoints[1], yPoints[1]);
                drawConditionalSequenceFlowIndicator(line);
            }

            if (associationDirection.Equals(AssociationDirection.ONE) || associationDirection.Equals(AssociationDirection.BOTH))
            {
                Line2D.Double line = new Line2D.Double(xPoints[xPoints.Length - 2], yPoints[xPoints.Length - 2], xPoints[xPoints.Length - 1], yPoints[xPoints.Length - 1]);
                drawArrowHead(line);
            }
            if (associationDirection.Equals(AssociationDirection.BOTH))
            {
                Line2D.Double line = new Line2D.Double(xPoints[1], yPoints[1], xPoints[0], yPoints[0]);
                drawArrowHead(line);
            }
            g.Paint = originalPaint;
            g.Stroke = originalStroke;
        }

        public virtual void drawSequenceflowWithoutArrow(int srcX, int srcY, int targetX, int targetY, bool conditional)
        {
            drawSequenceflowWithoutArrow(srcX, srcY, targetX, targetY, conditional, false);
        }

        public virtual void drawSequenceflowWithoutArrow(int srcX, int srcY, int targetX, int targetY, bool conditional, bool highLighted)
        {
            Paint originalPaint = g.Paint;
            if (highLighted)
            {
                g.Paint = HIGHLIGHT_COLOR;
            }

            Line2D.Double line = new Line2D.Double(srcX, srcY, targetX, targetY);
            g.draw(line);

            if (conditional)
            {
                drawConditionalSequenceFlowIndicator(line);
            }

            if (highLighted)
            {
                g.Paint = originalPaint;
            }
        }

        public virtual void drawArrowHead(Line2D.Double line)
        {
            int doubleArrowWidth = (int)(2 * ARROW_WIDTH);
            if (doubleArrowWidth == 0)
            {
                doubleArrowWidth = 2;
            }
            Polygon arrowHead = new Polygon();
            arrowHead.addPoint(0, 0);
            int arrowHeadPoint = (int)(-ARROW_WIDTH);
            if (arrowHeadPoint == 0)
            {
                arrowHeadPoint = -1;
            }
            arrowHead.addPoint(arrowHeadPoint, -doubleArrowWidth);
            arrowHeadPoint = (int)(ARROW_WIDTH);
            if (arrowHeadPoint == 0)
            {
                arrowHeadPoint = 1;
            }
            arrowHead.addPoint(arrowHeadPoint, -doubleArrowWidth);

            AffineTransform transformation = new AffineTransform();
            transformation.setToIdentity();
            double angle = Math.Atan2(line.y2 - line.y1, line.x2 - line.x1);
            transformation.translate(line.x2, line.y2);
            transformation.rotate((angle - Math.PI / 2d));

            AffineTransform originalTransformation = g.Transform;
            g.Transform = transformation;
            g.fill(arrowHead);
            g.Transform = originalTransformation;
        }

        public virtual void drawDefaultSequenceFlowIndicator(Line2D.Double line)
        {
            double length = DEFAULT_INDICATOR_WIDTH;
            double halfOfLength = length / 2;
            double f = 8;
            Line2D.Double defaultIndicator = new Line2D.Double(-halfOfLength, 0, halfOfLength, 0);

            double angle = Math.Atan2(line.y2 - line.y1, line.x2 - line.x1);
            double dx = f * Math.Cos(angle);
            double dy = f * Math.Sin(angle);
            double x1 = line.x1 + dx;
            double y1 = line.y1 + dy;

            AffineTransform transformation = new AffineTransform();
            transformation.setToIdentity();
            transformation.translate(x1, y1);
            transformation.rotate((angle - 3 * Math.PI / 4));

            AffineTransform originalTransformation = g.Transform;
            g.Transform = transformation;
            g.draw(defaultIndicator);

            g.Transform = originalTransformation;
        }

        public virtual void drawConditionalSequenceFlowIndicator(Line2D.Double line)
        {
            int horizontal = (int)(CONDITIONAL_INDICATOR_WIDTH * 0.7);
            int halfOfHorizontal = horizontal / 2;
            int halfOfVertical = CONDITIONAL_INDICATOR_WIDTH / 2;

            Polygon conditionalIndicator = new Polygon();
            conditionalIndicator.addPoint(0, 0);
            conditionalIndicator.addPoint(-halfOfHorizontal, halfOfVertical);
            conditionalIndicator.addPoint(0, CONDITIONAL_INDICATOR_WIDTH);
            conditionalIndicator.addPoint(halfOfHorizontal, halfOfVertical);

            AffineTransform transformation = new AffineTransform();
            transformation.setToIdentity();
            double angle = Math.Atan2(line.y2 - line.y1, line.x2 - line.x1);
            transformation.translate(line.x1, line.y1);
            transformation.rotate((angle - Math.PI / 2d));

            AffineTransform originalTransformation = g.Transform;
            g.Transform = transformation;
            g.draw(conditionalIndicator);

            Paint originalPaint = g.Paint;
            g.Paint = CONDITIONAL_INDICATOR_COLOR;
            g.fill(conditionalIndicator);

            g.Paint = originalPaint;
            g.Transform = originalTransformation;
        }

        public virtual void drawTask(TaskIconType icon, string id, string name, GraphicInfo graphicInfo)
        {
            drawTask(id, name, graphicInfo);

            icon.drawIcon((int)graphicInfo.X, (int)graphicInfo.Y, ICON_PADDING, g);
        }

        public virtual void drawTask(string id, string name, GraphicInfo graphicInfo)
        {
            drawTask(id, name, graphicInfo, false);
        }

        public virtual void drawPoolOrLane(string id, string name, GraphicInfo graphicInfo)
        {
            int x = (int)graphicInfo.X;
            int y = (int)graphicInfo.Y;
            int width = (int)graphicInfo.Width;
            int height = (int)graphicInfo.Height;
            g.drawRect(x, y, width, height);

            // Add the name as text, vertical
            if (!string.ReferenceEquals(name, null) && name.Length > 0)
            {
                // Include some padding
                int availableTextSpace = height - 6;

                // Create rotation for derived font
                AffineTransform transformation = new AffineTransform();
                transformation.setToIdentity();
                transformation.rotate(270 * Math.PI / 180);

                Font currentFont = g.Font;
                Font theDerivedFont = currentFont.deriveFont(transformation);
                g.Font = theDerivedFont;

                string truncated = fitTextToWidth(name, availableTextSpace);
                int realWidth = fontMetrics.stringWidth(truncated);

                g.drawString(truncated, x + 2 + fontMetrics.Height, 3 + y + availableTextSpace - (availableTextSpace - realWidth) / 2);
                g.Font = currentFont;
            }

            // set element's id
            g.CurrentGroupId = id;
        }

        protected internal virtual void drawTask(string id, string name, GraphicInfo graphicInfo, bool thickBorder)
        {
            Paint originalPaint = g.Paint;
            int x = (int)graphicInfo.X;
            int y = (int)graphicInfo.Y;
            int width = (int)graphicInfo.Width;
            int height = (int)graphicInfo.Height;

            // Create a new gradient paint for every task box, gradient depends on x and y and is not relative
            g.Paint = TASK_BOX_COLOR;

            int arcR = 6;
            if (thickBorder)
            {
                arcR = 3;
            }

            // shape
            RoundRectangle2D rect = new RoundRectangle2D.Double(x, y, width, height, arcR, arcR);
            g.fill(rect);
            g.Paint = TASK_BORDER_COLOR;

            if (thickBorder)
            {
                Stroke originalStroke = g.Stroke;
                g.Stroke = THICK_TASK_BORDER_STROKE;
                g.draw(rect);
                g.Stroke = originalStroke;
            }
            else
            {
                g.draw(rect);
            }

            g.Paint = originalPaint;
            // text
            if (!string.ReferenceEquals(name, null) && name.Length > 0)
            {
                int boxWidth = width - (2 * TEXT_PADDING);
                int boxHeight = height - 16 - ICON_PADDING - ICON_PADDING - MARKER_WIDTH - 2 - 2;
                int boxX = x + width / 2 - boxWidth / 2;
                int boxY = y + height / 2 - boxHeight / 2 + ICON_PADDING + ICON_PADDING - 2 - 2;

                drawMultilineCentredText(name, boxX, boxY, boxWidth, boxHeight);
            }

            // set element's id
            g.CurrentGroupId = id;
        }

        protected internal virtual void drawMultilineCentredText(string text, int x, int y, int boxWidth, int boxHeight)
        {
            drawMultilineText(text, x, y, boxWidth, boxHeight, true);
        }

        protected internal virtual void drawMultilineAnnotationText(string text, int x, int y, int boxWidth, int boxHeight)
        {
            drawMultilineText(text, x, y, boxWidth, boxHeight, false);
        }

        protected internal virtual void drawMultilineText(string text, int x, int y, int boxWidth, int boxHeight, bool centered)
        {
            // Create an attributed string based in input text
            AttributedString attributedString = new AttributedString(text);
            attributedString.addAttribute(TextAttribute.FONT, g.Font);
            attributedString.addAttribute(TextAttribute.FOREGROUND, Color.black);

            AttributedCharacterIterator characterIterator = attributedString.Iterator;

            int currentHeight = 0;
            // Prepare a list of lines of text we'll be drawing
            IList<TextLayout> layouts = new List<TextLayout>();
            string lastLine = null;

            LineBreakMeasurer measurer = new LineBreakMeasurer(characterIterator, g.FontRenderContext);

            TextLayout layout = null;
            while (measurer.Position < characterIterator.EndIndex && currentHeight <= boxHeight)
            {

                int previousPosition = measurer.Position;

                // Request next layout
                layout = measurer.nextLayout(boxWidth);

                int height = ((float?)(layout.Descent + layout.Ascent + layout.Leading)).Value;

                if (currentHeight + height > boxHeight)
                {
                    // The line we're about to add should NOT be added anymore, append three dots to previous one instead
                    // to indicate more text is truncated
                    if (layouts.Count > 0)
                    {
                        layouts.Remove(layouts.Count - 1);

                        if (lastLine.Length >= 4)
                        {
                            lastLine = lastLine.Substring(0, lastLine.Length - 4) + "...";
                        }
                        layouts.Add(new TextLayout(lastLine, g.Font, g.FontRenderContext));
                    }
                    break;
                }
                else
                {
                    layouts.Add(layout);
                    lastLine = text.Substring(previousPosition, measurer.Position - previousPosition);
                    currentHeight += height;
                }
            }

            int currentY = y + (centered ? ((boxHeight - currentHeight) / 2) : 0);
            int currentX = 0;

            // Actually draw the lines
            foreach (TextLayout textLayout in layouts)
            {

                currentY += textLayout.Ascent;
                currentX = x + (centered ? ((boxWidth - ((double?)textLayout.Bounds.Width).Value) / 2) : 0);

                textLayout.draw(g, currentX, currentY);
                currentY += textLayout.Descent + textLayout.Leading;
            }
        }

        protected internal virtual string fitTextToWidth(string original, int width)
        {
            string text = original;

            // remove length for "..."
            int maxWidth = width - 10;

            while (fontMetrics.stringWidth(text + "...") > maxWidth && text.Length > 0)
            {
                text = text.Substring(0, text.Length - 1);
            }

            if (!text.Equals(original))
            {
                text = text + "...";
            }

            return text;
        }

        public virtual void drawUserTask(string id, string name, GraphicInfo graphicInfo)
        {
            drawTask(USERTASK_IMAGE, id, name, graphicInfo);
        }

        public virtual void drawScriptTask(string id, string name, GraphicInfo graphicInfo)
        {
            drawTask(SCRIPTTASK_IMAGE, id, name, graphicInfo);
        }

        public virtual void drawServiceTask(string id, string name, GraphicInfo graphicInfo)
        {
            drawTask(SERVICETASK_IMAGE, id, name, graphicInfo);
        }

        public virtual void drawReceiveTask(string id, string name, GraphicInfo graphicInfo)
        {
            drawTask(RECEIVETASK_IMAGE, id, name, graphicInfo);
        }

        public virtual void drawSendTask(string id, string name, GraphicInfo graphicInfo)
        {
            drawTask(SENDTASK_IMAGE, id, name, graphicInfo);
        }

        public virtual void drawManualTask(string id, string name, GraphicInfo graphicInfo)
        {
            drawTask(MANUALTASK_IMAGE, id, name, graphicInfo);
        }

        public virtual void drawBusinessRuleTask(string id, string name, GraphicInfo graphicInfo)
        {
            drawTask(BUSINESS_RULE_TASK_IMAGE, id, name, graphicInfo);
        }

        public virtual void drawExpandedSubProcess(string id, string name, GraphicInfo graphicInfo, bool? isTriggeredByEvent)
        {
            RoundRectangle2D rect = new RoundRectangle2D.Double(graphicInfo.X, graphicInfo.Y, graphicInfo.Width, graphicInfo.Height, 8, 8);

            // Use different stroke (dashed)
            if (isTriggeredByEvent.Value)
            {
                Stroke originalStroke = g.Stroke;
                g.Stroke = EVENT_SUBPROCESS_STROKE;
                g.draw(rect);
                g.Stroke = originalStroke;
            }
            else
            {
                Paint originalPaint = g.Paint;
                g.Paint = SUBPROCESS_BOX_COLOR;
                g.fill(rect);
                g.Paint = SUBPROCESS_BORDER_COLOR;
                g.draw(rect);
                g.Paint = originalPaint;
            }

            if (!string.ReferenceEquals(name, null) && name.Length > 0)
            {
                string text = fitTextToWidth(name, (int)graphicInfo.Width);
                g.drawString(text, (int)graphicInfo.X + 10, (int)graphicInfo.Y + 15);
            }

            // set element's id
            g.CurrentGroupId = id;
        }

        public virtual void drawCollapsedSubProcess(string id, string name, GraphicInfo graphicInfo, bool? isTriggeredByEvent)
        {
            drawCollapsedTask(id, name, graphicInfo, false);
        }

        public virtual void drawCollapsedCallActivity(string id, string name, GraphicInfo graphicInfo)
        {
            drawCollapsedTask(id, name, graphicInfo, true);
        }

        protected internal virtual void drawCollapsedTask(string id, string name, GraphicInfo graphicInfo, bool thickBorder)
        {
            // The collapsed marker is now visualized separately
            drawTask(id, name, graphicInfo, thickBorder);
        }

        public virtual void drawCollapsedMarker(int x, int y, int width, int height)
        {
            // rectangle
            int rectangleWidth = MARKER_WIDTH;
            int rectangleHeight = MARKER_WIDTH;
            Rectangle rect = new Rectangle(x + (width - rectangleWidth) / 2, y + height - rectangleHeight - 3, rectangleWidth, rectangleHeight);
            g.draw(rect);

            // plus inside rectangle
            Line2D.Double line = new Line2D.Double(rect.CenterX, rect.Y + 2, rect.CenterX, rect.MaxY - 2);
            g.draw(line);
            line = new Line2D.Double(rect.MinX + 2, rect.CenterY, rect.MaxX - 2, rect.CenterY);
            g.draw(line);
        }

        public virtual void drawActivityMarkers(int x, int y, int width, int height, bool multiInstanceSequential, bool multiInstanceParallel, bool collapsed)
        {
            if (collapsed)
            {
                if (!multiInstanceSequential && !multiInstanceParallel)
                {
                    drawCollapsedMarker(x, y, width, height);
                }
                else
                {
                    drawCollapsedMarker(x - MARKER_WIDTH / 2 - 2, y, width, height);
                    if (multiInstanceSequential)
                    {
                        drawMultiInstanceMarker(true, x + MARKER_WIDTH / 2 + 2, y, width, height);
                    }
                    else
                    {
                        drawMultiInstanceMarker(false, x + MARKER_WIDTH / 2 + 2, y, width, height);
                    }
                }
            }
            else
            {
                if (multiInstanceSequential)
                {
                    drawMultiInstanceMarker(true, x, y, width, height);
                }
                else if (multiInstanceParallel)
                {
                    drawMultiInstanceMarker(false, x, y, width, height);
                }
            }
        }

        public virtual void drawGateway(GraphicInfo graphicInfo)
        {
            Polygon rhombus = new Polygon();
            int x = (int)graphicInfo.X;
            int y = (int)graphicInfo.Y;
            int width = (int)graphicInfo.Width;
            int height = (int)graphicInfo.Height;

            rhombus.addPoint(x, y + (height / 2));
            rhombus.addPoint(x + (width / 2), y + height);
            rhombus.addPoint(x + width, y + (height / 2));
            rhombus.addPoint(x + (width / 2), y);
            g.draw(rhombus);
        }

        public virtual void drawParallelGateway(string id, GraphicInfo graphicInfo)
        {
            // rhombus
            drawGateway(graphicInfo);
            int x = (int)graphicInfo.X;
            int y = (int)graphicInfo.Y;
            int width = (int)graphicInfo.Width;
            int height = (int)graphicInfo.Height;

            // plus inside rhombus
            Stroke orginalStroke = g.Stroke;
            g.Stroke = GATEWAY_TYPE_STROKE;
            Line2D.Double line = new Line2D.Double(x + 10, y + height / 2, x + width - 10, y + height / 2); // horizontal
            g.draw(line);
            line = new Line2D.Double(x + width / 2, y + height - 10, x + width / 2, y + 10); // vertical
            g.draw(line);
            g.Stroke = orginalStroke;

            // set element's id
            g.CurrentGroupId = id;
        }

        public virtual void drawExclusiveGateway(string id, GraphicInfo graphicInfo)
        {
            // rhombus
            drawGateway(graphicInfo);
            int x = (int)graphicInfo.X;
            int y = (int)graphicInfo.Y;
            int width = (int)graphicInfo.Width;
            int height = (int)graphicInfo.Height;

            int quarterWidth = width / 4;
            int quarterHeight = height / 4;

            // X inside rhombus
            Stroke orginalStroke = g.Stroke;
            g.Stroke = GATEWAY_TYPE_STROKE;
            Line2D.Double line = new Line2D.Double(x + quarterWidth + 3, y + quarterHeight + 3, x + 3 * quarterWidth - 3, y + 3 * quarterHeight - 3);
            g.draw(line);
            line = new Line2D.Double(x + quarterWidth + 3, y + 3 * quarterHeight - 3, x + 3 * quarterWidth - 3, y + quarterHeight + 3);
            g.draw(line);
            g.Stroke = orginalStroke;

            // set element's id
            g.CurrentGroupId = id;
        }

        public virtual void drawInclusiveGateway(string id, GraphicInfo graphicInfo)
        {
            // rhombus
            drawGateway(graphicInfo);
            int x = (int)graphicInfo.X;
            int y = (int)graphicInfo.Y;
            int width = (int)graphicInfo.Width;
            int height = (int)graphicInfo.Height;

            int diameter = width / 2;

            // circle inside rhombus
            Stroke orginalStroke = g.Stroke;
            g.Stroke = GATEWAY_TYPE_STROKE;
            Ellipse2D.Double circle = new Ellipse2D.Double(((width - diameter) / 2) + x, ((height - diameter) / 2) + y, diameter, diameter);
            g.draw(circle);
            g.Stroke = orginalStroke;

            // set element's id
            g.CurrentGroupId = id;
        }

        public virtual void drawEventBasedGateway(string id, GraphicInfo graphicInfo)
        {
            // rhombus
            drawGateway(graphicInfo);

            int x = (int)graphicInfo.X;
            int y = (int)graphicInfo.Y;
            int width = (int)graphicInfo.Width;
            int height = (int)graphicInfo.Height;

            double scale = .6;

            GraphicInfo eventInfo = new GraphicInfo();
            eventInfo.X = x + width * (1 - scale) / 2;
            eventInfo.Y = y + height * (1 - scale) / 2;
            eventInfo.Width = width * scale;
            eventInfo.Height = height * scale;
            drawCatchingEvent(null, eventInfo, true, null, "eventGateway");

            double r = width / 6.0;

            // create pentagon (coords with respect to center)
            int topX = (int)(.95 * r); // top right corner
            int topY = (int)(-.31 * r);
            int bottomX = (int)(.59 * r); // bottom right corner
            int bottomY = (int)(.81 * r);

            int[] xPoints = new int[] { 0, topX, bottomX, -bottomX, -topX };
            int[] yPoints = new int[] { -(int)r, topY, bottomY, bottomY, topY };
            Polygon pentagon = new Polygon(xPoints, yPoints, 5);
            pentagon.translate(x + width / 2, y + width / 2);

            // draw
            g.drawPolygon(pentagon);

            // set element's id
            g.CurrentGroupId = id;
        }

        public virtual void drawMultiInstanceMarker(bool sequential, int x, int y, int width, int height)
        {
            int rectangleWidth = MARKER_WIDTH;
            int rectangleHeight = MARKER_WIDTH;
            int lineX = x + (width - rectangleWidth) / 2;
            int lineY = y + height - rectangleHeight - 3;

            Stroke orginalStroke = g.Stroke;
            g.Stroke = MULTI_INSTANCE_STROKE;

            if (sequential)
            {
                g.draw(new Line2D.Double(lineX, lineY, lineX + rectangleWidth, lineY));
                g.draw(new Line2D.Double(lineX, lineY + rectangleHeight / 2, lineX + rectangleWidth, lineY + rectangleHeight / 2));
                g.draw(new Line2D.Double(lineX, lineY + rectangleHeight, lineX + rectangleWidth, lineY + rectangleHeight));
            }
            else
            {
                g.draw(new Line2D.Double(lineX, lineY, lineX, lineY + rectangleHeight));
                g.draw(new Line2D.Double(lineX + rectangleWidth / 2, lineY, lineX + rectangleWidth / 2, lineY + rectangleHeight));
                g.draw(new Line2D.Double(lineX + rectangleWidth, lineY, lineX + rectangleWidth, lineY + rectangleHeight));
            }

            g.Stroke = orginalStroke;
        }

        public virtual void drawHighLight(int x, int y, int width, int height)
        {
            Paint originalPaint = g.Paint;
            Stroke originalStroke = g.Stroke;

            g.Paint = HIGHLIGHT_COLOR;
            g.Stroke = THICK_TASK_BORDER_STROKE;

            RoundRectangle2D rect = new RoundRectangle2D.Double(x, y, width, height, 20, 20);
            g.draw(rect);

            g.Paint = originalPaint;
            g.Stroke = originalStroke;
        }

        public virtual void drawTextAnnotation(string id, string text, GraphicInfo graphicInfo)
        {
            int x = (int)graphicInfo.X;
            int y = (int)graphicInfo.Y;
            int width = (int)graphicInfo.Width;
            int height = (int)graphicInfo.Height;

            Font originalFont = g.Font;
            Stroke originalStroke = g.Stroke;

            g.Font = ANNOTATION_FONT;

            Path2D path = new Path2D.Double();
            x += .5;
            int lineLength = 18;
            path.moveTo(x + lineLength, y);
            path.lineTo(x, y);
            path.lineTo(x, y + height);
            path.lineTo(x + lineLength, y + height);

            path.lineTo(x + lineLength, y + height - 1);
            path.lineTo(x + 1, y + height - 1);
            path.lineTo(x + 1, y + 1);
            path.lineTo(x + lineLength, y + 1);
            path.closePath();

            g.draw(path);

            int boxWidth = width - (2 * ANNOTATION_TEXT_PADDING);
            int boxHeight = height - (2 * ANNOTATION_TEXT_PADDING);
            int boxX = x + width / 2 - boxWidth / 2;
            int boxY = y + height / 2 - boxHeight / 2;

            if (!string.ReferenceEquals(text, null) && text.Length > 0)
            {
                drawMultilineAnnotationText(text, boxX, boxY, boxWidth, boxHeight);
            }

            // restore originals
            g.Font = originalFont;
            g.Stroke = originalStroke;

            // set element's id
            g.CurrentGroupId = id;
        }

        public virtual void drawLabel(string text, GraphicInfo graphicInfo)
        {
            drawLabel(text, graphicInfo, true);
        }

        public virtual void drawLabel(string text, GraphicInfo graphicInfo, bool centered)
        {
            float interline = 1.0f;

            // text
            if (!string.ReferenceEquals(text, null) && text.Length > 0)
            {
                Paint originalPaint = g.Paint;
                Font originalFont = g.Font;

                g.Paint = LABEL_COLOR;
                g.Font = LABEL_FONT;

                int wrapWidth = 100;
                int textY = (int)graphicInfo.Y;

                // TODO: use drawMultilineText()
                AttributedString @as = new AttributedString(text);
                @as.addAttribute(TextAttribute.FOREGROUND, g.Paint);
                @as.addAttribute(TextAttribute.FONT, g.Font);
                AttributedCharacterIterator aci = @as.Iterator;
                FontRenderContext frc = new FontRenderContext(null, true, false);
                LineBreakMeasurer lbm = new LineBreakMeasurer(aci, frc);

                while (lbm.Position < text.Length)
                {
                    TextLayout tl = lbm.nextLayout(wrapWidth);
                    textY += tl.Ascent;
                    Rectangle2D bb = tl.Bounds;
                    double tX = graphicInfo.X;
                    if (centered)
                    {
                        tX += (int)(graphicInfo.Width / 2 - bb.Width / 2);
                    }
                    tl.draw(g, (float)tX, textY);
                    textY += tl.Descent + tl.Leading + (interline - 1.0f) * tl.Ascent;
                }

                // restore originals
                g.Font = originalFont;
                g.Paint = originalPaint;
            }
        }

        /// <summary>
        /// This method makes coordinates of connection flow better. </summary>
        /// <param name="sourceShapeType"> </param>
        /// <param name="targetShapeType"> </param>
        /// <param name="sourceGraphicInfo"> </param>
        /// <param name="targetGraphicInfo"> </param>
        /// <param name="graphicInfoList"> </param>
        public virtual IList<GraphicInfo> connectionPerfectionizer(SHAPE_TYPE sourceShapeType, SHAPE_TYPE targetShapeType, GraphicInfo sourceGraphicInfo, GraphicInfo targetGraphicInfo, IList<GraphicInfo> graphicInfoList)
        {
            Shape shapeFirst = createShape(sourceShapeType, sourceGraphicInfo);
            Shape shapeLast = createShape(targetShapeType, targetGraphicInfo);

            if (graphicInfoList != null && graphicInfoList.Count > 0)
            {
                GraphicInfo graphicInfoFirst = graphicInfoList[0];
                GraphicInfo graphicInfoLast = graphicInfoList[graphicInfoList.Count - 1];
                if (shapeFirst != null)
                {
                    graphicInfoFirst.X = shapeFirst.Bounds2D.CenterX;
                    graphicInfoFirst.Y = shapeFirst.Bounds2D.CenterY;
                }
                if (shapeLast != null)
                {
                    graphicInfoLast.X = shapeLast.Bounds2D.CenterX;
                    graphicInfoLast.Y = shapeLast.Bounds2D.CenterY;
                }

                Point p = null;

                if (shapeFirst != null)
                {
                    Line2D.Double lineFirst = new Line2D.Double(graphicInfoFirst.X, graphicInfoFirst.Y, graphicInfoList[1].X, graphicInfoList[1].Y);
                    p = getIntersection(shapeFirst, lineFirst);
                    if (p != null)
                    {
                        graphicInfoFirst.X = p.X;
                        graphicInfoFirst.Y = p.Y;
                    }
                }

                if (shapeLast != null)
                {
                    Line2D.Double lineLast = new Line2D.Double(graphicInfoLast.X, graphicInfoLast.Y, graphicInfoList[graphicInfoList.Count - 2].X, graphicInfoList[graphicInfoList.Count - 2].Y);
                    p = getIntersection(shapeLast, lineLast);
                    if (p != null)
                    {
                        graphicInfoLast.X = p.X;
                        graphicInfoLast.Y = p.Y;
                    }
                }
            }

            return graphicInfoList;
        }

        /// <summary>
        /// This method creates shape by type and coordinates. </summary>
        /// <param name="shapeType"> </param>
        /// <param name="graphicInfo"> </param>
        /// <returns> Shape </returns>
        private static Shape createShape(SHAPE_TYPE shapeType, GraphicInfo graphicInfo)
        {
            if (SHAPE_TYPE.Rectangle.Equals(shapeType))
            {
                // source is rectangle
                return new Rectangle2D.Double(graphicInfo.X, graphicInfo.Y, graphicInfo.Width, graphicInfo.Height);
            }
            else if (SHAPE_TYPE.Rhombus.Equals(shapeType))
            {
                // source is rhombus
                Path2D.Double rhombus = new Path2D.Double();
                rhombus.moveTo(graphicInfo.X, graphicInfo.Y + graphicInfo.Height / 2);
                rhombus.lineTo(graphicInfo.X + graphicInfo.Width / 2, graphicInfo.Y + graphicInfo.Height);
                rhombus.lineTo(graphicInfo.X + graphicInfo.Width, graphicInfo.Y + graphicInfo.Height / 2);
                rhombus.lineTo(graphicInfo.X + graphicInfo.Width / 2, graphicInfo.Y);
                rhombus.lineTo(graphicInfo.X, graphicInfo.Y + graphicInfo.Height / 2);
                rhombus.closePath();
                return rhombus;
            }
            else if (SHAPE_TYPE.Ellipse.Equals(shapeType))
            {
                // source is ellipse
                return new Ellipse2D.Double(graphicInfo.X, graphicInfo.Y, graphicInfo.Width, graphicInfo.Height);
            }
            // unknown source element, just do not correct coordinates
            return null;
        }

        /// <summary>
        /// This method returns intersection point of shape border and line. </summary>
        /// <param name="shape"> </param>
        /// <param name="line"> </param>
        /// <returns> Point </returns>
        private static Point getIntersection(Shape shape, Line2D.Double line)
        {
            if (shape is Ellipse2D)
            {
                return getEllipseIntersection(shape, line);
            }
            else if (shape is Rectangle2D || shape is Path2D)
            {
                return getShapeIntersection(shape, line);
            }
            else
            {
                // something strange
                return null;
            }
        }

        /// <summary>
        /// This method calculates ellipse intersection with line </summary>
        /// <param name="shape"> Bounds of this shape used to calculate parameters of inscribed into this bounds ellipse. </param>
        /// <param name="line"> </param>
        /// <returns> Intersection point </returns>
        private static Point getEllipseIntersection(Shape shape, Line2D.Double line)
        {
            double angle = Math.Atan2(line.y2 - line.y1, line.x2 - line.x1);
            double x = shape.Bounds2D.Width / 2 * Math.Cos(angle) + shape.Bounds2D.CenterX;
            double y = shape.Bounds2D.Height / 2 * Math.Sin(angle) + shape.Bounds2D.CenterY;
            Point p = new Point();
            p.setLocation(x, y);
            return p;
        }

        /// <summary>
        /// This method calculates shape intersection with line. </summary>
        /// <param name="shape"> </param>
        /// <param name="line"> </param>
        /// <returns> Intersection point </returns>
        private static Point getShapeIntersection(Shape shape, Line2D.Double line)
        {
            PathIterator it = shape.getPathIterator(null);
            double[] coords = new double[6];
            double[] pos = new double[2];
            Line2D.Double l = new Line2D.Double();
            while (!it.Done)
            {
                int type = it.currentSegment(coords);
                switch (type)
                {
                    case PathIterator.SEG_MOVETO:
                        pos[0] = coords[0];
                        pos[1] = coords[1];
                        break;
                    case PathIterator.SEG_LINETO:
                        l = new Line2D.Double(pos[0], pos[1], coords[0], coords[1]);
                        if (line.intersectsLine(l))
                        {
                            return getLinesIntersection(line, l);
                        }
                        pos[0] = coords[0];
                        pos[1] = coords[1];
                        break;
                    case PathIterator.SEG_CLOSE:
                        break;
                    default:
                        // whatever
                        break;
                }
                it.next();
            }
            return null;
        }

        /// <summary>
        /// This method calculates intersections of two lines. </summary>
        /// <param name="a"> Line 1 </param>
        /// <param name="b"> Line 2 </param>
        /// <returns> Intersection point </returns>
        private static Point getLinesIntersection(Line2D a, Line2D b)
        {
            double d = (a.X1 - a.X2) * (b.Y2 - b.Y1) - (a.Y1 - a.Y2) * (b.X2 - b.X1);
            double da = (a.X1 - b.X1) * (b.Y2 - b.Y1) - (a.Y1 - b.Y1) * (b.X2 - b.X1);
            double ta = da / d;
            Point p = new Point();
            p.setLocation(a.X1 + ta * (a.X2 - a.X1), a.Y1 + ta * (a.Y2 - a.Y1));
            return p;
        }
    }

}