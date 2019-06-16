namespace Sys.Workflow.Image.Impl.Icon
{
	using org.apache.batik.svggen;
	using org.w3c.dom;

	public class MessageIconType : IconType
	{

		public override string FillValue
		{
			get
			{
				return "#585858";
			}
		}

		public override string StrokeValue
		{
			get
			{
				return "none";
			}
		}

		public override string StrokeWidth
		{
			get
			{
				return "1";
			}
		}

		public override string DValue
		{
			get
			{
				return " m0 1.5  l0 13  l17 0  l0 -13  z M1.5 3  L6 7.5  L1.5 12  z M3.5 3  L13.5 3  L8.5 8  z m12 0  l0 9  l-4.5 -4.5  z M7 8.5  L8.5 10  L10 8.5  L14.5 13  L2.5 13  z";
			}
		}

        public override void drawIcon(int imageX, int imageY, int iconPadding, ProcessDiagramSVGGraphics2D svgGenerator)
		{
			Element gTag = svgGenerator.DOMFactory.createElementNS(null, SVGGraphics2D.SVG_G_TAG);
			gTag.setAttributeNS(null, "transform", "translate(" + (imageX - 1) + "," + (imageY - 2) + ")");

			Element pathTag = svgGenerator.DOMFactory.createElementNS(null, SVGGraphics2D.SVG_PATH_TAG);
			pathTag.setAttributeNS(null, "d", this.DValue);
			pathTag.setAttributeNS(null, "fill", this.FillValue);
			pathTag.setAttributeNS(null, "stroke", this.StrokeValue);
			pathTag.setAttributeNS(null, "stroke-widthh", this.StrokeWidth);

			gTag.appendChild(pathTag);
			svgGenerator.ExtendDOMGroupManager.addElement(gTag);
		}

		public override string AnchorValue
		{
			get
			{
				return null;
			}
		}

		public override string StyleValue
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
				return 17;
			}
		}

		public override int? Height
		{
			get
			{
				return 13;
			}
		}
	}

}