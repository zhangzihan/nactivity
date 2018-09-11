namespace org.activiti.image.impl
{

	using org.apache.batik.svggen;
	using org.w3c.dom;

	public class ProcessDiagramSVGGraphics2D : SVGGraphics2D
	{

		public ProcessDiagramSVGGraphics2D(Document domFactory) : base(domFactory)
		{
			this.DOMGroupManager = new ProcessDiagramDOMGroupManager(this.GraphicContext, this.DOMTreeManager);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void setRenderingHints(@SuppressWarnings("rawtypes") java.util.Map hints)
		public override ("rawtypes") System.Collections.IDictionary RenderingHints
		{
			set
			{
				base.RenderingHints = value;
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void addRenderingHints(@SuppressWarnings("rawtypes") java.util.Map hints)
		public override void addRenderingHints(("rawtypes") System.Collections.IDictionary hints)
		{
			base.addRenderingHints(hints);
		}

		public virtual string CurrentGroupId
		{
			set
			{
				this.ExtendDOMGroupManager.CurrentGroupId = value;
			}
		}

		public virtual ProcessDiagramDOMGroupManager ExtendDOMGroupManager
		{
			get
			{
				return (ProcessDiagramDOMGroupManager) base.DOMGroupManager;
			}
		}
	}

}