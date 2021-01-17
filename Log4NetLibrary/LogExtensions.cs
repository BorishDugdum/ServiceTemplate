using log4net;
using System;
using System.Collections.Generic;
using System.Text;

namespace Log4NetLibrary
{
    public static class ILogLevelExtensions
    {
        public static readonly log4net.Core.Level Auth = new log4net.Core.Level(50000, "AUTH");
    }

    public static class ILogExtentions
    {
        public static void Trace(this ILog log, string message, Exception exception)
        {
            log.Logger.Log(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType,
                log4net.Core.Level.Trace, message, exception);
        }

        public static void Trace(this ILog log, string message)
        {
            log.Trace(message, null);
        }

        public static void Notice(this ILog log, string message, Exception exception)
        {
            log.Logger.Log(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType,
                log4net.Core.Level.Notice, message, exception);
        }

        public static void Notice(this ILog log, string message)
        {
            log.Notice(message, null);
        }

        public static void Verbose(this ILog log, string message, Exception exception)
        {
            log.Logger.Log(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType,
                log4net.Core.Level.Verbose, message, exception);
        }

        public static void Verbose(this ILog log, string message)
        {
            log.Verbose(message, null);
        }
    }

    public static class SecurityExtensions
    {
        public static void Auth(this ILog log, string message)
        {
            log.Logger.Log(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType,
                ILogLevelExtensions.Auth, message, null);
        }

        public static void AuthFormat(this ILog log, string message, params object[] args)
        {
            string formattedMessage = string.Format(message, args);
            log.Logger.Log(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType,
                ILogLevelExtensions.Auth, formattedMessage, null);
        }
    }
}
