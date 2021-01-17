using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;

namespace Log4NetLibrary
{
#if DEBUG
    internal class DebugHelper
    {
        /// <summary>
        /// Old Method to read config xml from embeded resource
        /// </summary>
        internal static void SetLog4NetConfiguration()
        {
            var test = ReadResource();
            XmlDocument log4netConfig = new XmlDocument();
            log4netConfig.LoadXml(test);//.Load(File.OpenRead(LOG_CONFIG_FILE));

            var repo = LogManager.CreateRepository(
                Assembly.GetEntryAssembly(), typeof(log4net.Repository.Hierarchy.Hierarchy));

            log4net.Config.XmlConfigurator.Configure(repo, log4netConfig["log4net"]);
        }
        
        /// <summary>
        /// Reads log4net.config in Startup project embedded resource. 
        /// Assumes it's the only embedded resource so only grabs first resource from manifest.
        /// </summary>
        /// <returns></returns>
        private static string ReadResource()
        {
            var assembly = Assembly.GetExecutingAssembly();
            //string resourcePath = "log4net.config";
            var names = assembly.GetManifestResourceNames();

            // Format: "{Namespace}.{Folder}.{filename}.{Extension}"
            var mrs = assembly.GetManifestResourceStream(names.First()); //if we have multiple - make sure we find it and not just grab first


            using (Stream stream = mrs)
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// See what's locking file
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        internal static string GetFileProcessName(string filePath)
        {
            Process[] procs = Process.GetProcesses();
            string fileName = Path.GetFileName(filePath);

            foreach (Process proc in procs)
            {
                if (proc.MainWindowHandle != new IntPtr(0) && !proc.HasExited)
                {
                    ProcessModule[] arr = new ProcessModule[proc.Modules.Count];

                    foreach (ProcessModule pm in proc.Modules)
                    {
                        if (pm.ModuleName == fileName)
                            return proc.ProcessName;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Quick test to see if logs are working
        /// </summary>
        internal static void LogTest()
        {
            var methodbase = MethodBase.GetCurrentMethod().DeclaringType;
            var log = LogManager.GetLogger(methodbase);

            log.Debug("Debug!");
            log.Info("Info!");
            log.Warn("Warn!");
            log.Error("Error!", new Exception("Error Exception"));
            log.Fatal("Fatal!", new Exception("Fatal Exception"));
            log.Verbose("Verbose!");
            log.Trace("Trace!");
            log.Notice("Notice!");
            log.Auth("Auth!");

            log.Info($"[{Environment.CurrentManagedThreadId}] [{Environment.UserDomainName}] [{Environment.UserName}]");
            log.Error($"[{Environment.CurrentManagedThreadId}] [{Environment.UserDomainName}] [{Environment.UserName}]");
        }
    }
#endif
}
