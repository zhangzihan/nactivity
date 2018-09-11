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
namespace org.activiti.engine.impl.@delegate.invocation
{
    using org.activiti.engine.@delegate;
    using org.activiti.engine.impl.persistence.entity;

    /// <summary>
    /// Class handling invocations of JavaDelegates
    /// 
    /// 
    /// </summary>
    public class JavaDelegateInvocation : DelegateInvocation
	{

	  protected internal readonly IJavaDelegate delegateInstance;
	  protected internal readonly IExecutionEntity execution;

	  public JavaDelegateInvocation(IJavaDelegate delegateInstance, IExecutionEntity execution)
	  {
		this.delegateInstance = delegateInstance;
		this.execution = execution;
	  }

	  protected internal override void invoke()
	  {
		delegateInstance.execute(execution);
	  }

	  public override object Target
	  {
		  get
		  {
			return delegateInstance;
		  }
	  }

	}

}