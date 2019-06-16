namespace Sys.Workflow.image.impl.icon
{
	using org.apache.batik.svggen;
	using org.w3c.dom;

	public class ErrorThrowIconType : ErrorIconType
	{

		public override string FillValue
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
				return " M20.820839 9.171502  L17.36734 22.58992  L11.54138 12.281818999999999  L7.3386512 18.071607  L11.048949 4.832305699999999  L16.996148 14.132659  L20.820839 9.171502  z";
			}
		}

        public virtual void drawIcon(int imageX, int imageY, int iconPadding, SVGGraphics2D svgGenerator)
		{
			Element gTag = svgGenerator.DOMFactory.createElementNS(null, SVGGraphics2D.SVG_G_TAG);
			gTag.setAttributeNS(null, "transform", "translate(" + (imageX - 4) + "," + (imageY - 4) + ")");

			Element pathTag = svgGenerator.DOMFactory.createElementNS(null, SVGGraphics2D.SVG_PATH_TAG);
			pathTag.setAttributeNS(null, "d", this.DValue);
			pathTag.setAttributeNS(null, "style", this.StyleValue);
			pathTag.setAttributeNS(null, "fill", this.FillValue);
			pathTag.setAttributeNS(null, "stroke", this.StrokeValue);

			gTag.appendChild(pathTag);
			svgGenerator.DOMTreeManager.appendGroup(gTag, null);
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
				return "stroke-width:1.5;stroke-linecap:butt;stroke-linejoin:miter;stroke-miterlimit:10";
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