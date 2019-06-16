namespace Sys.Workflow.Image.Impl.Icon
{
	using org.apache.batik.svggen;
	using org.w3c.dom;

	public class CompensateThrowIconType : CompensateIconType
	{

		public override int? Width
		{
			get
			{
				return 15;
			}
		}

		public override int? Height
		{
			get
			{
				return 16;
			}
		}

		public override string FillValue
		{
			get
			{
				return "#585858";
			}
		}

		public override void drawIcon(int imageX, int imageY, int iconPadding, ProcessDiagramSVGGraphics2D svgGenerator)
		{
			Element gTag = svgGenerator.DOMFactory.createElementNS(null, SVGGraphics2D.SVG_G_TAG);
			gTag.setAttributeNS(null, "transform", "translate(" + (imageX - 8) + "," + (imageY - 6) + ")");

			Element polygonTag1 = svgGenerator.DOMFactory.createElementNS(null, SVGGraphics2D.SVG_POLYGON_TAG);
			polygonTag1.setAttributeNS(null, "points", "14 8 14 22 7 15 ");
			polygonTag1.setAttributeNS(null, "fill", this.FillValue);
			polygonTag1.setAttributeNS(null, "stroke", this.StrokeValue);
			polygonTag1.setAttributeNS(null, "stroke-width", this.StrokeWidth);
			polygonTag1.setAttributeNS(null, "stroke-linecap", "butt");
			polygonTag1.setAttributeNS(null, "stroke-linejoin", "miter");
			polygonTag1.setAttributeNS(null, "stroke-miterlimit", "10");
			gTag.appendChild(polygonTag1);

			Element polygonTag2 = svgGenerator.DOMFactory.createElementNS(null, SVGGraphics2D.SVG_POLYGON_TAG);
			polygonTag2.setAttributeNS(null, "points", "21 8 21 22 14 15 ");
			polygonTag2.setAttributeNS(null, "fill", this.FillValue);
			polygonTag2.setAttributeNS(null, "stroke", this.StrokeValue);
			polygonTag2.setAttributeNS(null, "stroke-width", this.StrokeWidth);
			polygonTag2.setAttributeNS(null, "stroke-linecap", "butt");
			polygonTag2.setAttributeNS(null, "stroke-linejoin", "miter");
			polygonTag2.setAttributeNS(null, "stroke-miterlimit", "10");
			gTag.appendChild(polygonTag2);

			svgGenerator.ExtendDOMGroupManager.addElement(gTag);
		}
	}

}