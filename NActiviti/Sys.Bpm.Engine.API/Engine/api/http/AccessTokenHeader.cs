using System;
using System.Net;

namespace Sys.Net.Http
{
    /// <summary>
    /// access_token HttpHeader头
    /// </summary>
    class AccessTokenHeader
    {
        /// <summary>
        /// heade头 Authorization: Bearer access_token
        /// </summary>
        public string Header { get => Enum.GetName(typeof(HttpRequestHeader), HttpRequestHeader.Authorization); }

        private string _accessToken = null;

        /// <summary>
        /// access_token验证码
        /// </summary>
        public string AccessToken
        {
            get => $"Bearer {_accessToken}";
            set => _accessToken = value;
        }
    }
}
