using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Filter;
using log4net.Layout;
using log4net.Repository.Hierarchy;

namespace Log4NetLibrary
{
    public static class Log4NetHelper
    {
        public static void Configure(string logFilePath)
        {
            //Register Encoding for Core (ColoredConsoleAppender usage)
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();
            hierarchy.Root.RemoveAllAppenders(); /*Remove any other appenders to avoid duplicates*/
            hierarchy.Root.Level = new Level(Level.Verbose.Value, Level.Verbose.Name);
            GlobalContext.Properties["tab"] = '\t'; //not needed, but property example used in "info" appender (i could just do \t in string)

            #region List of Appenders
            var debug = Helper<RollingFileAppender>.Init(x =>
            {
                x.Name = "debug";
                x.File = $@"{logFilePath}\debug.log";
                x.AppendToFile = true;
                x.MaximumFileSize = "5KB";
                x.MaxSizeRollBackups = 10;
                x.StaticLogFileName = true;
                x.PreserveLogFileNameExtension = true;
                x.RollingStyle = RollingFileAppender.RollingMode.Size;
                x.Layout = Helper<CustomPatternLayout>.Init(o =>
                {
                    o.IgnoresException = false;
                    o.ConversionPattern = "[%utcdate{yyyy-MM-dd HH:mm.ss}]  %lvlL  %editCl  \"%message\"%newline";
                    o.ActivateOptions();
                });
                x.AddFilter(Helper<LevelRangeFilter>.Init(o =>
                {
                    o.LevelMin = Level.All;
                    o.ActivateOptions();
                }));
                x.ActivateOptions();
                hierarchy.Root.AddAppender(x);
            });

            var info = Helper<RollingFileAppender>.Init(x =>
            {
                x.Name = "info";
                x.File = $@"{logFilePath}\info.log";
                x.AppendToFile = true;
                x.MaximumFileSize = "5KB";
                x.MaxSizeRollBackups = 10;
                x.StaticLogFileName = true;
                x.PreserveLogFileNameExtension = true;
                x.RollingStyle = RollingFileAppender.RollingMode.Size;
                x.Layout = Helper<CustomPatternLayout>.Init(o =>
                {
                    o.ConversionPattern = "[%utcdate{yyyy-MM-dd HH:mm.ss}]  %lvlC  %editCs  \"%message\"%newline";
                    o.ActivateOptions();
                });
                foreach (var level in new Level[] { ILogLevelExtensions.Auth, Level.Info, Level.Verbose, Level.Notice })
                {
                    x.AddFilter(Helper<LevelMatchFilter>.Init(o =>
                    {
                        o.AcceptOnMatch = true;
                        o.LevelToMatch = level;
                        o.ActivateOptions();
                    }));
                }
                x.AddFilter(Helper<DenyAllFilter>.Init(o =>
                {
                    o.ActivateOptions();
                }));
                x.ActivateOptions();
                hierarchy.Root.AddAppender(x);
            });

            var console = Helper<ColoredConsoleAppender>.Init(c =>
            {
                c.Name = "console";
                c.Layout = Helper<CustomPatternLayout>.Init(o =>
                {
                    o.ConversionPattern = "[%utcdate{yyyy-MM-dd HH:mm.ss}]  %lvlC  %editCs  \"%m\"%n%tabex";//"%utcdate{yyyy-MM-dd HH:mm.ss} %lvlL (%c{1}) - \"%m\"%n%tabex";
                    o.IgnoresException = false;
                    o.ActivateOptions();
                });
                foreach (var map in ColoredConsoleValues.Colors)
                    c.AddMapping(map);
                c.ActivateOptions();
                hierarchy.Root.AddAppender(c);
            });

            var warn = Helper<RollingFileAppender>.Init(x =>
            {
                x.Name = "warn";
                x.File = $@"{logFilePath}\Warnings\";
                x.DatePattern = "yyyy-MM-dd.'txt'";
                x.StaticLogFileName = false;
                x.AppendToFile = true;
                x.MaximumFileSize = "15MB";
                x.MaxSizeRollBackups = 60;
                x.RollingStyle = RollingFileAppender.RollingMode.Date;
                x.LockingModel = Helper<FileAppender.MinimalLock>.Init(o =>
                {
                    if (o.CurrentAppender != null)
                    {
                        //Check to see if file is locked
                        //log.Debug($"Process: '{GetFileProcessName($@"{logFilePath}\Warnings\{DateTime.Now:yyyy-MM-dd}.txt")}' locking err file '{$@"{logFilePath}\Warnings\{DateTime.Now:yyyy-MM-dd}.txt"}'");
                        //o.ReleaseLock();
                    }
                    o.ActivateOptions();
                });
                x.Layout = Helper<CustomPatternLayout>.Init(o =>
                {
                    o.ConversionPattern = "[%utcdate{yyyy-MM-dd HH:mm.ss}]  [%level]  %editCl  \"%message\"%newline";//"[%utcdate{yyyy-MM-dd HH:mm.ss}] [%level] (%c) - \"%message\"%newline";
                    o.ActivateOptions();
                });
                x.AddFilter(Helper<LevelMatchFilter>.Init(o =>
                {
                    o.AcceptOnMatch = true;
                    o.LevelToMatch = Level.Warn;
                    o.ActivateOptions();
                }));
                x.AddFilter(Helper<DenyAllFilter>.Init(o =>
                {
                    o.ActivateOptions();
                }));
                x.ActivateOptions();
                hierarchy.Root.AddAppender(x);
            });

            var err = Helper<RollingFileAppender>.Init(x =>
            {
                x.Name = "error";
                x.File = $@"{logFilePath}\Errors\";
                x.DatePattern = "yyyy-MM-dd.'txt'";
                x.StaticLogFileName = false;
                x.AppendToFile = true;
                x.MaximumFileSize = "15MB";
                x.MaxSizeRollBackups = 60;
                x.RollingStyle = RollingFileAppender.RollingMode.Date;
                x.LockingModel = Helper<FileAppender.MinimalLock>.Init(o =>
                {
                    if (o.CurrentAppender != null)
                    {
                        //Check to see if file is locked
                        //log.Debug($"Process: '{GetFileProcessName($@"{logFilePath}\Errors\{DateTime.Now:yyyy-MM-dd}.txt")}' locking err file '{$@"{logFilePath}\Errors\{DateTime.Now:yyyy-MM-dd}.txt"}'");
                        //o.ReleaseLock();
                    }
                    o.ActivateOptions();
                });
                x.Layout = Helper<CustomPatternLayout>.Init(o =>
                {
                    o.IgnoresException = false;
                    o.ConversionPattern = "[%utcdate{yyyy-MM-dd HH:mm.ss}] [%level]%n-  (%c)%n-  \"%message\"%newline%tabex%n";
                    o.ActivateOptions();
                });
                x.AddFilter(Helper<LevelRangeFilter>.Init(o =>
                {
                    o.LevelMin = Level.Error;
                    o.ActivateOptions();
                }));
                x.ActivateOptions(); 
                hierarchy.Root.AddAppender(x);
            });
            #endregion

            //Configure log4net
            BasicConfigurator.Configure(debug, info, console, warn, err);
        }

        /// <summary>
        /// Logs/Displays list of Levels and their value (order of importance). 
        /// Includes custom logs (you have to manually add the typelist below if you didn't add it in those classes)
        /// Helps with filtering by range.
        /// </summary>
        public static void ListLogLevels()
        {
            var methodbase = MethodBase.GetCurrentMethod().DeclaringType;
            var log = LogManager.GetLogger(methodbase);

            log.Notice($"Checking for enabled logs...");

            var levelInfo = new List<FieldInfo>();
            foreach(var t in new Type[] { typeof(Level), typeof(ILogLevelExtensions) }) //pull all Levels from different classes
            {
                var newFields = t.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
                levelInfo.AddRange(newFields);
            }

            var logInfo = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach(var t in new Type[] { typeof(ILog), typeof(ILogExtentions), typeof(SecurityExtensions) }) //pull enabled log methods
            {
                var newFields = t.GetMethods().Where(x => x.IsPublic && x.ReturnType.Equals(typeof(void)) && x.GetParameters().Count() == 2).Where(x => !x.Name.Contains("Format")).Select(x => x.Name)?.ToHashSet();
                if(newFields != null)
                    logInfo.UnionWith(newFields);
            }

            levelInfo = levelInfo.OrderBy(x => (x.GetValue(null) as Level).Value).ToList(); //order by value (importance)
            foreach(var f in levelInfo)
            {
                var oValue = f.GetValue(null) as Level;
                var s = oValue.DisplayName;
                string centered_name = string.Format("{0,-13}", string.Format("{0," + ((13 + s.Length) / 2).ToString() + "}", s));

                var enabled = logInfo.Contains(s) ? "ENABLED" : "";

                log.Notice($"[ {string.Format("{0,-7}", enabled)} ] [ {centered_name} ]   {string.Format("{0,-12}", oValue.Value)}");
            }
        }

        /// <summary>
        /// Used to manually initialize an object and pass it back.
        /// I love this thing.
        /// </summary>
        /// <typeparam name="T">Your new Object</typeparam>
        public class Helper<T> where T : new()
        {
            public static T Init(Action<T> body)
            {
                T obj = new T();
                body(obj);
                return obj;
            }
        }

        
    }
}
