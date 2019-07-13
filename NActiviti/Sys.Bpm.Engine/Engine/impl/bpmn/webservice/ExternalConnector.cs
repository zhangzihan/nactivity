using System;
using System.Collections.Generic;
using System.Text;

namespace Sys.Workflow.Engine.Impl.Bpmn.Webservice
{
    public static class ExternalConnector
    {
        public static string Url(string url)
        {
            ExternalConnectorProvider provider = ProcessEngineServiceProvider.Resolve<ExternalConnectorProvider>();

            return provider.ResolveUrl(url);
        }
    }
}
