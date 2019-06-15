using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sys.Net.Http
{
    public class HttpClientProxyOption
    {
        private readonly IConfiguration configuration;

        public HttpClientProxyOption(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public string WorkflowServiceUrl { get; set; }
    }
}
