using Sys.Workflow.Bpmn.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.Bpm.Client.Tests
{
	public class Program
	{
		static void Main(string[] args)
		{
			var model = new BpmnXMLConverter().ConvertToBpmnModel($@"C:\Users\zhn\Desktop\1.bpmn");
		}
	}
}
