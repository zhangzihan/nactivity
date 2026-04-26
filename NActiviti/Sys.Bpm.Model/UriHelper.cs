using System;
using System.IO;
using System.Net;

namespace Sys
{
    public static class UriHelper
    {
        public static Stream OpenStream(this Uri uri)
        {
            try
            {
                WebClient client = new WebClient();
                return client.OpenRead(uri);
            }
            catch (Exception)
            {
                //todo: handle me
                throw;
            }
        }
    }
}
