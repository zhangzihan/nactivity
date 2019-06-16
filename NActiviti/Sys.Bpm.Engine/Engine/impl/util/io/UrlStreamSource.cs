/* Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *      http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using Sys;
using System;
using System.IO;
using Sys.Workflow.Engine;

namespace Sys.Workflow.Engine.Impl.Util.IO
{

    /// 
    public class UrlStreamSource : IStreamSource
    {

        internal Uri url;

        public UrlStreamSource(Uri url)
        {
            this.url = url;
        }

        public virtual Stream InputStream
        {
            get
            {
                try
                {
                    return new BufferedStream(url.OpenStream());
                }
                catch (IOException e)
                {
                    throw new ActivitiIllegalArgumentException("couldn't open url '" + url + "'", e);
                }
            }
        }
    }

}