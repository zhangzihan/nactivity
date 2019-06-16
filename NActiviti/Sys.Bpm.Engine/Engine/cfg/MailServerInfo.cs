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

namespace Sys.Workflow.Engine.Cfg
{
	/// 
	public class MailServerInfo
	{

	  protected internal string mailServerDefaultFrom;
	  protected internal string mailServerHost;
	  protected internal int mailServerPort;
	  protected internal string mailServerUsername;
	  protected internal string mailServerPassword;
	  protected internal bool mailServerUseSSL;
	  protected internal bool mailServerUseTLS;

	  public virtual string MailServerDefaultFrom
	  {
		  get
		  {
			return mailServerDefaultFrom;
		  }
		  set
		  {
			this.mailServerDefaultFrom = value;
		  }
	  }


	  public virtual string MailServerHost
	  {
		  get
		  {
			return mailServerHost;
		  }
		  set
		  {
			this.mailServerHost = value;
		  }
	  }


	  public virtual int MailServerPort
	  {
		  get
		  {
			return mailServerPort;
		  }
		  set
		  {
			this.mailServerPort = value;
		  }
	  }


	  public virtual string MailServerUsername
	  {
		  get
		  {
			return mailServerUsername;
		  }
		  set
		  {
			this.mailServerUsername = value;
		  }
	  }


	  public virtual string MailServerPassword
	  {
		  get
		  {
			return mailServerPassword;
		  }
		  set
		  {
			this.mailServerPassword = value;
		  }
	  }


	  public virtual bool MailServerUseSSL
	  {
		  get
		  {
			return mailServerUseSSL;
		  }
		  set
		  {
			this.mailServerUseSSL = value;
		  }
	  }


	  public virtual bool MailServerUseTLS
	  {
		  get
		  {
			return mailServerUseTLS;
		  }
		  set
		  {
			this.mailServerUseTLS = value;
		  }
	  }

	}

}