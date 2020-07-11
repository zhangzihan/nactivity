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
namespace Sys.Workflow.Engine.Impl.EL
{

    /// <summary>
    /// Simple implementation of the <seealso cref="ELContext"/> used during parsings.
    /// 
    /// Currently this implementation does nothing, but a non-null implementation of the <seealso cref="ELContext"/> interface is required by the <seealso cref="ExpressionFactory"/> when create value- and methodexpressions.
    /// </summary>
    /// <seealso cref= ExpressionManager#createExpression(String) </seealso>
    /// <seealso cref= ExpressionManager#createMethodExpression(String)
    /// 
    ///  </seealso>
    public class ParsingElContext : ELContext
    {
        /// <summary>
        /// 
        /// </summary>
        public override ELResolver ELResolver
        {
            get
            {
                return null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public override FunctionMapper FunctionMapper
        {
            get
            {
                return null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public override VariableMapper VariableMapper
        {
            get
            {
                return null;
            }
        }
    }

}