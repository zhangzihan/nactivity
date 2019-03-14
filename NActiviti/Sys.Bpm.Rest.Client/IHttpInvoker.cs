using System.Threading.Tasks;

namespace Sys.Bpm.Rest.Client
{
    /// <summary>
    /// 
    /// </summary>
    public interface IHttpInvoker
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uri"></param>
        /// <returns></returns>
        Task<T> GetAsync<T>(string uri);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uri"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<T> PostAsync<T>(string uri, object data);
    }
}