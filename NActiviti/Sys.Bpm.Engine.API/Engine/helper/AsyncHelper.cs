using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sys
{
    // /// <summary>
    // /// 异步辅助方法
    // /// </summary>
    //public static class AsyncHelper
    //{
    //    private static TaskFactory CreateTaskFactory()
    //    {
    //        return new TaskFactory(CancellationToken.None,
    //                   TaskCreationOptions.None,
    //                   TaskContinuationOptions.None,
    //                   TaskScheduler.Default);
    //    }

    //    /// <summary>
    //    /// 同步调用异步方法
    //    /// </summary>
    //    /// <typeparam name="T"></typeparam>
    //    /// <param name="func"></param>
    //    /// <returns></returns>
    //    public static T RunSync<T>(Func<Task<T>> func) =>
    //        CreateTaskFactory()
    //            .StartNew(func)
    //            .Unwrap()
    //            .GetAwaiter()
    //            .GetResult();


    //    /// <summary>
    //    /// 同步调用异步方法
    //    /// </summary>
    //    /// <param name="func"></param>
    //    public static void RunSync(Func<Task> func) =>
    //        CreateTaskFactory()
    //            .StartNew(func)
    //            .Unwrap()
    //            .GetAwaiter()
    //            .GetResult();
    //}
}
