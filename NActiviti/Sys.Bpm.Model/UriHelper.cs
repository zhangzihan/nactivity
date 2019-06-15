using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

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
