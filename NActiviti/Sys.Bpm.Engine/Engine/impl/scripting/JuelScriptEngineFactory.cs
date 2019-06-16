using System.Collections.Generic;
using System.Text;

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

namespace Sys.Workflow.engine.impl.scripting
{


	/// <summary>
	/// Factory to create <seealso cref="JuelScriptEngine"/>s.
	/// 
	/// 
	/// </summary>
	public class JuelScriptEngineFactory : ScriptEngineFactory
	{

	  private static IList<string> names;
	  private static IList<string> extensions;
	  private static IList<string> mimeTypes;

	  static JuelScriptEngineFactory()
	  {
		names = Collections.unmodifiableList(Collections.singletonList("juel"));
		extensions = names;
		mimeTypes = Collections.unmodifiableList(new List<string>(0));
	  }

	  public virtual string EngineName
	  {
		  get
		  {
			return "juel";
		  }
	  }

	  public virtual string EngineVersion
	  {
		  get
		  {
			return "1.0";
		  }
	  }

	  public virtual IList<string> Extensions
	  {
		  get
		  {
			return extensions;
		  }
	  }

	  public virtual string LanguageName
	  {
		  get
		  {
			return "JSP 2.1 EL";
		  }
	  }

	  public virtual string LanguageVersion
	  {
		  get
		  {
			return "2.1";
		  }
	  }

	  public virtual string getMethodCallSyntax(string obj, string method, params string[] arguments)
	  {
		throw new System.NotSupportedException("Method getMethodCallSyntax is not supported");
	  }

	  public virtual IList<string> MimeTypes
	  {
		  get
		  {
			return mimeTypes;
		  }
	  }

	  public virtual IList<string> Names
	  {
		  get
		  {
			return names;
		  }
	  }

	  public virtual string getOutputStatement(string toDisplay)
	  {
		// We will use out:print function to output statements
		StringBuilder stringBuffer = new StringBuilder();
		stringBuffer.Append("out:print(\"");

		int length = toDisplay.Length;
		for (int i = 0; i < length; i++)
		{
		  char c = toDisplay[i];
		  switch (c)
		  {
		  case '"':
			stringBuffer.Append("\\\"");
			break;
		  case '\\':
			stringBuffer.Append("\\\\");
			break;
		  default:
			stringBuffer.Append(c);
			break;
		  }
		}
		stringBuffer.Append("\")");
		return stringBuffer.ToString();
	  }

	  public virtual string getParameter(string key)
	  {
		if (key.Equals(ScriptEngine.NAME))
		{
		  return LanguageName;
		}
		else if (key.Equals(ScriptEngine.ENGINE))
		{
		  return EngineName;
		}
		else if (key.Equals(ScriptEngine.ENGINE_VERSION))
		{
		  return EngineVersion;
		}
		else if (key.Equals(ScriptEngine.LANGUAGE))
		{
		  return LanguageName;
		}
		else if (key.Equals(ScriptEngine.LANGUAGE_VERSION))
		{
		  return LanguageVersion;
		}
		else if (key.Equals("THREADING"))
		{
		  return "MULTITHREADED";
		}
		else
		{
		  return null;
		}
	  }

	  public virtual string getProgram(params string[] statements)
	  {
		// Each statement is wrapped in '${}' to comply with EL
		StringBuilder buf = new StringBuilder();
		if (statements.Length != 0)
		{
		  for (int i = 0; i < statements.Length; i++)
		  {
			buf.Append("${");
			buf.Append(statements[i]);
			buf.Append("} ");
		  }
		}
		return buf.ToString();
	  }

	  public virtual ScriptEngine ScriptEngine
	  {
		  get
		  {
			return new JuelScriptEngine(this);
		  }
	  }

	}

}