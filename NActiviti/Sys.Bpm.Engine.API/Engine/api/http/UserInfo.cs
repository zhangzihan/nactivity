using Sys.Workflow;

namespace Sys.Net.Http
{
    /// <inheritdoc />
    public class UserInfo : IUserInfo
    {
        private string loginUserId;

        /// <inheritdoc />
        public string Id { get; set; }

        /// <inheritdoc />
        public string FullName { get; set; }

        /// <inheritdoc />
        public string TenantId { get; set; }

        /// <inheritdoc />
        public string Email { get; set; }

        /// <inheritdoc />
        public string Phone { get; set; }

        /// <inheritdoc />
        public string LoginUserId
        {
            get
            {
                if (string.IsNullOrWhiteSpace(loginUserId))
                    return Id;

                return loginUserId;
            }
            set => loginUserId = value;
        }
    }
}
