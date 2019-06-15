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
namespace Sys.Workflow.Options
{
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;

    /// <summary>
    /// 流程引擎配置
    /// </summary>
    public class ProcessEngineOption
    {
        private static string s_configFileName;
        private static byte[] hashByte;

        internal static string ConfigFileName
        {
            get => s_configFileName;
            set
            {
                s_configFileName = value;

                hashByte = ComputeHash();
            }
        }

        private static byte[] ComputeHash()
        {
            using (var fs = File.OpenRead(s_configFileName))
            {
                return SHA1.Create().ComputeHash(fs);
            }
        }

        /// <summary>
        /// 配置数据发生变化
        /// </summary>
        /// <returns></returns>
        public static bool HasChanged()
        {
            var curr = ComputeHash();

            bool changed = !hashByte.SequenceEqual(curr);

            if (changed)
            {
                hashByte = curr;
            }

            return changed;
        }

        /// <summary>
        /// 
        /// </summary>
        public ProcessEngineOption()
        {

        }
    }
}