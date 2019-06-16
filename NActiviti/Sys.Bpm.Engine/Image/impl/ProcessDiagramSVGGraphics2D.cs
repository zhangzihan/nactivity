namespace Sys.Workflow.image.impl
{

	using org.apache.batik.svggen;
	using org.w3c.dom;

	public class ProcessDiagramSVGGraphics2D : SVGGraphics2D
	{

		public ProcessDiagramSVGGraphics2D(Document domFactory) : base(domFactory)
		{
			this.DOMGroupManager = new ProcessDiagramDOMGroupManager(this.GraphicContext, this.DOMTreeManager);
		}

        public override ("rawtypes") System.Collections.IDictionary RenderingHints
		{
			set
			{
				base.RenderingHints = value;
			}
		}

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