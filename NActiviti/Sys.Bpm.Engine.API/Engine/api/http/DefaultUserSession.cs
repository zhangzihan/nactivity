using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Sys.Workflow;
using Sys.Extentions;
using Sys.IdentityModel;

namespace Sys.Net.Http
{
    public class DefaultUserSession : IUserSession
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public DefaultUserSession(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public IUserInfo CreateUser(string userId, string tenantId)
        {
            ClaimsPrincipal principal = new ClaimsPrincipal(new ClaimsIdentity[]
            {
                new ClaimsIdentity(new Claim[]
                {
                    new Claim(JwtClaimTypes.Subject, userId),
                    new Claim(JwtClaimTypes.TenantId, tenantId)
                })
            });

            return CreateUser(principal);

        }

        public IUserInfo CreateUser(ClaimsPrincipal principal)
        {
            string uid = principal.GetSubjectId();
            string name = principal.GetDisplayName();
            string phone = principal.GetPhone();
            string email = principal.GetEmail();
            string tenantId = principal.GetTenantId();

            return new UserInfo
            {
                Id = uid,
                Email = email,
                FullName = name,
                Phone = phone,
                TenantId = tenantId
            };
        }

        public Task<IUserInfo> GetUserAsync(HttpContext context)
        {
            if (context.User is object)
            {
                return Task.FromResult(CreateUser(context.User));
            }

            return Task.FromResult<IUserInfo>(null);
        }
    }
}
