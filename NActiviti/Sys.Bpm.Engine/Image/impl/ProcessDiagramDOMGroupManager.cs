namespace Sys.Workflow.image.impl
{
	using org.apache.batik.ext.awt.g2d;
	using org.apache.batik.svggen;
	using org.apache.batik.svggen;

	public class ProcessDiagramDOMGroupManager : DOMGroupManager
	{

		public ProcessDiagramDOMGroupManager(GraphicContext gc, DOMTreeManager domTreeManager) : base(gc, domTreeManager)
		{
		}

		public virtual string CurrentGroupId
		{
			set
			{
				this.currentGroup.setAttribute("id", value);
			}
		}
	}
}