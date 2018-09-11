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
namespace org.activiti.bpmn.converter.util
{

	public class CommaSplitter
	{

		// split the given spring using commas if they are not inside an expression
		public static IList<string> splitCommas(string st)
		{
			IList<string> result = new List<string>();
			int offset = 0;

			bool inExpression = false;
			for (int i = 0; i < st.Length; i++)
			{
				if (!inExpression && st[i] == ',')
				{
					if ((i - offset) > 1)
					{
						result.Add(st.Substring(offset, i - offset));
					}
					offset = i + 1;
				}
				else if ((st[i] == '$' || st[i] == '#') && st[i + 1] == '{')
				{
					inExpression = true;
				}
				else if (st[i] == '}')
				{
					inExpression = false;
				}
			}

			if ((st.Length - offset) > 1)
			{
				result.Add(st.Substring(offset));
			}
			return result;
		}
	}

}