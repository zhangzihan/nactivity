namespace org.activiti.image.impl.icon
{
	using org.apache.batik.svggen;
	using org.w3c.dom;

	public abstract class TaskIconType : IconType
	{

		public override string AnchorValue
		{
			get
			{
				return "top left";
			}
		}

		public override string StrokeValue
		{
			get
			{
				return null;
			}
		}

		public override string FillValue
		{
			get
			{
				return null;
			}
		}

		public override int? Width
		{
			get
			{
				return null;
			}
		}

		public override int? Height
		{
			get
			{
				return null;
			}
		}

		public override string StrokeWidth
		{
			get
			{
				return null;
			}
		}

        public override void drawIcon(int imageX, int imageY, int iconPadding, ProcessDiagramSVGGraphics2D svgGenerator)
		{
			Element gTag = svgGenerator.DOMFactory.createElementNS(null, SVGGraphics2D.SVG_G_TAG);
			gTag.setAttributeNS(null, "transform", "translate(" + (imageX + iconPadding) + "," + (imageY + iconPadding) + ")");

			Element pathTag = svgGenerator.DOMFactory.createElementNS(null, SVGGraphics2D.SVG_PATH_TAG);
			pathTag.setAttributeNS(null, "d", this.DValue);
			pathTag.setAttributeNS(null, "anchors", this.AnchorValue);
			pathTag.setAttributeNS(null, "style", this.StyleValue);

			gTag.appendChild(pathTag);
			svgGenerator.ExtendDOMGroupManager.addElement(gTag);
		}
	}

}