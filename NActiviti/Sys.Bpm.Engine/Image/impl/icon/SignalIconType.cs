namespace Sys.Workflow.Image.Impl.Icon
{
	using org.apache.batik.svggen;
	using org.w3c.dom;

	public class SignalIconType : IconType
	{

		public override string FillValue
		{
			get
			{
				return "none";
			}
		}

		public override string StrokeValue
		{
			get
			{
				return "#585858";
			}
		}

		public override string DValue
		{
			get
			{
				return " M7.7124971 20.247342  L22.333334 20.247342  L15.022915000000001 7.575951200000001  L7.7124971 20.247342  z";
			}
		}

        public override void drawIcon(int imageX, int imageY, int iconPadding, ProcessDiagramSVGGraphics2D svgGenerator)
		{
			Element gTag = svgGenerator.DOMFactory.createElementNS(null, SVGGraphics2D.SVG_G_TAG);
			gTag.setAttributeNS(null, "transform", "translate(" + (imageX - 7) + "," + (imageY - 7) + ")");

			Element pathTag = svgGenerator.DOMFactory.createElementNS(null, SVGGraphics2D.SVG_PATH_TAG);
			pathTag.setAttributeNS(null, "d", this.DValue);
			pathTag.setAttributeNS(null, "style", this.StyleValue);
			pathTag.setAttributeNS(null, "fill", this.FillValue);
			pathTag.setAttributeNS(null, "stroke", this.StrokeValue);

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
				return "fill:none;stroke-width:1.4;stroke-miterlimit:4;stroke-dasharray:none";
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
				return 15;
			}
		}

		public override string StrokeWidth
		{
			get
			{
				return null;
			}
		}
	}

}