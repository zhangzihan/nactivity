using System;

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

namespace org.activiti.engine.impl.cmd
{

    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;
    using System.IO;
    using System.Collections.Generic;

    /// 
    [Serializable]
    public class GetAttachmentContentCmd : ICommand<Stream>
    {

        private const long serialVersionUID = 1L;
        protected internal string attachmentId;

        public GetAttachmentContentCmd(string attachmentId)
        {
            this.attachmentId = attachmentId;
        }

        public  virtual System.IO.Stream  execute(ICommandContext  commandContext)
        {
            IAttachmentEntity attachment = commandContext.AttachmentEntityManager.findById<IAttachmentEntity>(new KeyValuePair<string, object>("id", attachmentId));

            string contentId = attachment.ContentId;
            if (ReferenceEquals(contentId, null))
            {
                return null;
            }

            IByteArrayEntity byteArray = commandContext.ByteArrayEntityManager.findById<IByteArrayEntity>(new KeyValuePair<string, object>("id", contentId));
            byte[] bytes = byteArray.Bytes;

            return new System.IO.MemoryStream(bytes);
        }

    }

}