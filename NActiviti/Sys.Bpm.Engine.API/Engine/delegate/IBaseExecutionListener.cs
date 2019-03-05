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
namespace org.activiti.engine.@delegate
{

    /// <summary>
    /// Callback interface to be notified of execution events like starting a process instance, ending an activity instance or taking a transition.
    /// 
    /// 
    /// </summary>
    public interface IBaseExecutionListener
    {
    }

    public static class BaseExecutionListener_Fields
    {
        public const string EVENTNAME_START = "start";
        public const string EVENTNAME_END = "end";
        public const string EVENTNAME_TAKE = "take";
    }

}