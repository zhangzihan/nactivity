namespace Sys.Workflow.Image.Impl.Icon
{
	using org.apache.batik.svggen;
	using org.w3c.dom;

	public class ErrorIconType : IconType
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
				return " M21.820839 10.171502  L18.36734 23.58992  L12.541380000000002 13.281818999999999  L8.338651200000001 19.071607  L12.048949000000002 5.832305699999999  L17.996148000000005 15.132659  L21.820839 10.171502  z";
			}
		}

        public override void drawIcon(int imageX, int imageY, int iconPadding, ProcessDiagramSVGGraphics2D svgGenerator)
		{
			Element gTag = svgGenerator.DOMFactory.createElementNS(null, SVGGraphics2D.SVG_G_TAG);
			gTag.setAttributeNS(null, "transform", "translate(" + (imageX - 6) + "," + (imageY - 3) + ")");

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
				return "fill:none;stroke-width:1.5;stroke-linecap:butt;stroke-linejoin:miter;stroke-miterlimit:10";
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
				return 22;
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