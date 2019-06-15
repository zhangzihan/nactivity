using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Spring.Util;
using Sys.Net.Http;

namespace Sys.Workflow
{
    /// <summary>
    /// 用户信息
    /// </summary>
    public interface IUserInfo
    {
        /// <summary>
        /// 用户id
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// 登录用户id
        /// </summary>
        string LoginUserId { get; set; }

        /// <summary>
        /// 用户姓名
        /// </summary>
        string FullName { get; set; }

        /// <summary>
        /// Email
        /// </summary>
        string Email { get; set; }

        /// <summary>
        /// phone
        /// </summary>
        string Phone { get; set; }

        /// <summary>
        /// 租户id
        /// </summary>
        string TenantId { get; set; }
    }
}
