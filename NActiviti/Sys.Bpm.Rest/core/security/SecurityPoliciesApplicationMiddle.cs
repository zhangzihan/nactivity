/*
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */

using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;
using Sys.Net.Http;
using Sys.Workflow.Engine.Impl.Identities;
using Microsoft.Extensions.Logging;

namespace Sys.Workflow.Cloud.Services.Core
{
    /// <summary>
    /// authentication midlleware
    /// </summary>
    public class SecurityPoliciesApplicationMiddle
    {
        private readonly RequestDelegate next;
        private readonly IOptions<SecurityPoliciesProviderOptions> options;
        private readonly ILogger<SecurityPoliciesApplicationMiddle> logger;

        /// <summary>
        /// 
        /// </summary>
        public SecurityPoliciesApplicationMiddle(RequestDelegate next, IOptions<SecurityPoliciesProviderOptions> options,
            ILogger<SecurityPoliciesApplicationMiddle> logger)
        {
            this.next = next;
            this.options = options;
            this.logger = logger;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context)
        {
            //IMvcControllerDiscovery mvcControllerDiscovery = context.RequestServices.GetService<IMvcControllerDiscovery>();

            //var controllers = mvcControllerDiscovery.GetControllers();

#if DEBUG
            if (context.Request.Path.HasValue &&
                new Regex("/swagger/?").IsMatch(context.Request.Path.Value))
            {
                await next(context).ConfigureAwait(false);
                return;
            }
#endif
            IAccessTokenProvider tokenProvider = context.RequestServices.GetRequiredService<IAccessTokenProvider>();

            SecurityPoliciesApplicationService spas = context.RequestServices.GetService<SecurityPoliciesApplicationService>();

            spas.User = await tokenProvider.GetUser(context).ConfigureAwait(false);

            Authentication.AuthenticatedUser = spas.User;

            await next(context).ConfigureAwait(false);
        }
    }
}