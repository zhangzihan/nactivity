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
using System;
using System.IO;
using System.Text;
using Sys.Workflow.Engine;

namespace Sys.Workflow.Engine.Impl.Util
{

    /// 
    /// 
    /// 
    public class IoUtil
    {

        public static byte[] ReadInputStream(System.IO.Stream inputStream, string inputStreamName)
        {
            try
            {
                using (MemoryStream outputStream = new MemoryStream())
                {
                    inputStream.CopyTo(outputStream);
                    outputStream.Seek(0, SeekOrigin.Begin);
                    return outputStream.ToArray();
                }
            }
            catch (Exception e)
            {
                throw new ActivitiException("couldn't read input stream " + inputStreamName, e);
            }
        }

        public static string ReadFileAsString(string filePath)
        {
            try
            {
                using (FileStream fs = GetFile(filePath))
                {
                    StreamReader sr = new StreamReader(fs);

                    return sr.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                throw new ActivitiException("Couldn't read file " + filePath + ": " + e.Message);
            }
        }

        public static FileStream GetFile(string filePath)
        {
            //Uri url = typeof(IoUtil).ClassLoader.getResource(filePath);
            try
            {
                return File.Open(filePath, FileMode.Open);
            }
            catch (Exception e)
            {
                throw new ActivitiException("Couldn't get file " + filePath + ": " + e.Message);
            }
        }

        public static void WriteStringToFile(string content, string filePath)
        {
            try
            {
                var ws = new StreamWriter(filePath, false, Encoding.UTF8, 1024);
                ws.Write(content);
                ws.Flush();
            }
            catch (Exception e)
            {
                throw new ActivitiException("Couldn't write file " + filePath, e);
            }
        }

        /// <summary>
        /// Closes the given stream. The same as calling <seealso cref="InputStream#close()"/>, but errors while closing are silently ignored.
        /// </summary>
        public static void CloseSilently(System.IO.Stream stream)
        {
            try
            {
                if (stream is object)
                {
                    stream.Close();
                }
            }
            catch (IOException)
            {
                // Exception is silently ignored
            }
        }
    }

}