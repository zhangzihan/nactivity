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

namespace Sys.Workflow.Engine.Impl.Bpmn.Listeners
{
    using Sys.Workflow.Engine.Delegate;

    /// 
    [Serializable]
	public class ExpressionTaskListener : ITaskListener
	{

	  protected internal IExpression expression;

	  public ExpressionTaskListener(IExpression expression)
	  {
		this.expression = expression;
	  }

	  public virtual void Notify(IDelegateTask delegateTask)
	  {
		expression.GetValue(delegateTask);
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