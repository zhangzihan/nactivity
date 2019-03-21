using Sys.Workflow;

namespace Sys.Net.Http
{
    /// <summary>
    /// 用户信息
    /// </summary>
    internal class UserInfo : IUserInfo
    {
        /// <summary>
        /// user id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// user name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// tenant id
        /// </summary>
        public string TenantId { get; set; }
    }
}
