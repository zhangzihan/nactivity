using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Sys.Bpm.Rest.Client
{
    public class AccessTokenProvider
    {
        public AccessTokenHeader GetAccessTokenRequestHeader(string accessToken)
        {
            return new AccessTokenHeader
            {
                AccessToken = accessToken
            };
        }
    }

    public class AccessTokenHeader
    {
        public string Header { get => Enum.GetName(typeof(HttpRequestHeader), HttpRequestHeader.Authorization); }

        private string _accessToken = null;

        public string AccessToken
        {
            get => $"Bearer {_accessToken}";
            set => _accessToken = value;
        }
    }
}
