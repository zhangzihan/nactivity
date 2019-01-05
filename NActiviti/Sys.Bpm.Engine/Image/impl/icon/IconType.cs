namespace org.activiti.image.impl.icon
{

	public abstract class IconType
	{

		public abstract int? Width {get;}

		public abstract int? Height {get;}

		public abstract string AnchorValue {get;}

		public abstract string FillValue {get;}

		public abstract string StyleValue {get;}

		public abstract string DValue {get;}

        public abstract void drawIcon(int imageX, int imageY, int iconPadding, ProcessDiagramSVGGraphics2D svgGenerator);

		public abstract string StrokeValue {get;}

		public abstract string StrokeWidth {get;}
	}

}