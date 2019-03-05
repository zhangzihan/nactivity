using System;
using System.Collections.Generic;
using System.Text;

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
        /// 用户姓名
        /// </summary>
        string Name { get; set; }
    }
}
