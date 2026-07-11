using System;
using System.Reflection;
using NLog;

namespace Sys.Rules.DmnEngine.Utils
{
    /// <summary>
    /// Extension methods for NLog.Logger providing exception-throwing logging patterns
    /// and correlation-based logging, replacing the RadCommons.core ILogger interface.
    /// </summary>
    internal static class LoggerExt
    {
        /// <summary>
        /// Logs at Error level and creates/returns an exception of type <typeparamref name="TException"/>
        /// </summary>
        public static TException Error<TException>(this Logger logger, string message) where TException : Exception
        {
            logger.Error(message);
            return CreateException<TException>(message);
        }

        /// <summary>
        /// Logs at Fatal level and creates/returns an exception of type <typeparamref name="TException"/>
        /// </summary>
        public static TException Fatal<TException>(this Logger logger, string message) where TException : Exception
        {
            logger.Fatal(message);
            return CreateException<TException>(message);
        }

        /// <summary>
        /// Logs at Fatal level and creates/returns an exception of type <typeparamref name="TException"/> with inner exception
        /// </summary>
        public static TException Fatal<TException>(this Logger logger, string message, Exception innerException) where TException : Exception
        {
            logger.Fatal(innerException, message);
            return CreateException<TException>(message, innerException: innerException);
        }

        /// <summary>
        /// Logs at Info level with correlation ID
        /// </summary>
        public static void InfoCorr(this Logger logger, string correlationId, string message)
        {
            logger.Info($"[{correlationId}] {message}");
        }

        /// <summary>
        /// Logs at Trace level with correlation ID
        /// </summary>
        public static void TraceCorr(this Logger logger, string correlationId, string message)
        {
            logger.Trace($"[{correlationId}] {message}");
        }

        /// <summary>
        /// Logs at Warn level with correlation ID
        /// </summary>
        public static void WarnCorr(this Logger logger, string correlationId, string message)
        {
            logger.Warn($"[{correlationId}] {message}");
        }

        /// <summary>
        /// Logs at Error level with correlation ID and creates/returns an exception of type <typeparamref name="TException"/>
        /// </summary>
        public static TException ErrorCorr<TException>(this Logger logger, string correlationId, string message) where TException : Exception
        {
            logger.Error($"[{correlationId}] {message}");
            return CreateException<TException>(message);
        }

        /// <summary>
        /// Logs at Fatal level with correlation ID and creates/returns an exception of type <typeparamref name="TException"/>
        /// </summary>
        public static TException FatalCorr<TException>(this Logger logger, string correlationId, string message) where TException : Exception
        {
            logger.Fatal($"[{correlationId}] {message}");
            return CreateException<TException>(message);
        }

        /// <summary>
        /// Exception filter that logs the exception at Fatal level and returns false
        /// (so the exception is not caught but is logged)
        /// </summary>
        public static bool FatalFltr(this Logger logger, Exception exception)
        {
            logger.Fatal(exception, exception.Message);
            return false;
        }

        /// <summary>
        /// Creates an exception of type <typeparamref name="TException"/> with the given message.
        /// Handles constructors with optional parameters (e.g. string message, Exception innerException = null).
        /// </summary>
        private static TException CreateException<TException>(string message, Exception innerException = null) where TException : Exception
        {
            var type = typeof(TException);

            // Try (string, Exception) constructor first - covers both explicit and optional inner exception
            try
            {
                var ctor = type.GetConstructor(new[] { typeof(string), typeof(Exception) });
                if (ctor != null)
                {
                    return (TException)ctor.Invoke(new object[] { message, innerException });
                }
            }
            catch { /* fall through */ }

            // Try (string) constructor
            try
            {
                var ctor = type.GetConstructor(new[] { typeof(string) });
                if (ctor != null)
                {
                    return (TException)ctor.Invoke(new object[] { message });
                }
            }
            catch { /* fall through */ }

            // Fallback: use Activator with all args
            try
            {
                if (innerException != null)
                    return (TException)Activator.CreateInstance(type, message, innerException);
                return (TException)Activator.CreateInstance(type, message);
            }
            catch
            {
                return (TException)Activator.CreateInstance(type);
            }
        }
    }
}

