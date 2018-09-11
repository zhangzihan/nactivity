using System;
using System.Collections.Generic;

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
namespace org.activiti.engine.impl.bpmn.listener
{

    using org.activiti.engine.@delegate;
    using org.activiti.engine.impl.bpmn.helper;
    using org.activiti.engine.impl.bpmn.parser;
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.@delegate.invocation;

    /// 
    [Serializable]
	public class DelegateExpressionTaskListener : ITaskListener
	{

	  protected internal IExpression expression;
	  private readonly IList<FieldDeclaration> fieldDeclarations;

	  public DelegateExpressionTaskListener(IExpression expression, IList<FieldDeclaration> fieldDeclarations)
	  {
		this.expression = expression;
		this.fieldDeclarations = fieldDeclarations;
	  }

	  public virtual void notify(IDelegateTask delegateTask)
	  {
		object @delegate = DelegateExpressionUtil.resolveDelegateExpression(expression, delegateTask, fieldDeclarations);
		if (@delegate is ITaskListener)
		{
		  try
		  {
			Context.ProcessEngineConfiguration.DelegateInterceptor.handleInvocation(new TaskListenerInvocation((ITaskListener) @delegate, delegateTask));
		  }
		  catch (Exception e)
		  {
			throw new ActivitiException("Exception while invoking TaskListener: " + e.Message, e);
		  }
		}
		else
		{
		  throw new ActivitiIllegalArgumentException("Delegate expression " + expression + " did not resolve to an implementation of " + typeof(ITaskListener));
		}
	  }

	  /// <summary>
	  /// returns the expression text for this task listener. Comes in handy if you want to check which listeners you already have.
	  /// </summary>
	  public virtual string ExpressionText
	  {
		  get
		  {
			return expression.ExpressionText;
		  }
	  }

	}

}