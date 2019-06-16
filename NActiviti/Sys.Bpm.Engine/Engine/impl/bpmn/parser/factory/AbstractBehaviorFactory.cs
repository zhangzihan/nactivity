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
namespace Sys.Workflow.engine.impl.bpmn.parser.factory
{

    using Sys.Workflow.bpmn.model;
    using Sys.Workflow.engine.@delegate;
    using Sys.Workflow.engine.impl.el;

    /// 
    public abstract class AbstractBehaviorFactory
    {

        protected internal ExpressionManager expressionManager;

        public virtual IList<FieldDeclaration> CreateFieldDeclarations(IList<FieldExtension> fieldList)
        {
            IList<FieldDeclaration> fieldDeclarations = new List<FieldDeclaration>();

            foreach (FieldExtension fieldExtension in fieldList)
            {
                FieldDeclaration fieldDeclaration = null;
                if (!string.IsNullOrWhiteSpace(fieldExtension.Expression))
                {
                    fieldDeclaration = new FieldDeclaration(fieldExtension.FieldName, typeof(IExpression).FullName, expressionManager.CreateExpression(fieldExtension.Expression));
                }
                else
                {
                    fieldDeclaration = new FieldDeclaration(fieldExtension.FieldName, typeof(IExpression).FullName, new FixedValue(fieldExtension.StringValue));
                }

                fieldDeclarations.Add(fieldDeclaration);
            }
            return fieldDeclarations;
        }

        public virtual ExpressionManager ExpressionManager
        {
            get
            {
                return expressionManager;
            }
            set
            {
                this.expressionManager = value;
            }
        }


    }

}