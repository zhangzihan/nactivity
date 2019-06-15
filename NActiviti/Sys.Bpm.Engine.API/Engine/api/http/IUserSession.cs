using Microsoft.AspNetCore.Http;
using Sys.Workflow;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Sys.Net.Http
{
    public interface IUserSession
    {
        IUserInfo CreateUser(string userId, string tenantId);

        IUserInfo CreateUser(ClaimsPrincipal user);

        Task<IUserInfo> GetUserAsync(HttpContext context);
    }
}
