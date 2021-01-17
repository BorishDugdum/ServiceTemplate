using log4net;
using Log4NetLibrary;
using ServiceTemplate.Services;
using System;
using System.Collections.Generic;
using System.Reflection;
using Topshelf;

namespace ServiceTemplate
{
    internal static class ConfigureService
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        internal static void Configure(string[] args)
        {
            _log.Debug("Configure!");

            try
            {
                //only run test service if installing
                var result = Array.FindIndex(args, t => t.StartsWith("install", StringComparison.InvariantCultureIgnoreCase)); 
                if (result > -1)
                    RunHost(typeof(TestService), true);
                RunHost(typeof(MyService), false);
            }
            catch(Exception ex)
            {
                _log.Fatal(ex.Message, ex);
            }
        }
        private static void RunHost(Type type, bool runAsTest)
        {
            // query all flags for constructors of this type (so you can invoke any protection modifer that's accessible)
            var ctors = type.GetConstructors(
                BindingFlags.Public | BindingFlags.Instance | 
                BindingFlags.Static | BindingFlags.NonPublic | 
                BindingFlags.Default | BindingFlags.InvokeMethod | 
                BindingFlags.DeclaredOnly);

            // invoke the first constructor with no parameters.
            dynamic obj = ctors[0].Invoke(new object[] { });

            var exitCode = HostFactory.Run(x =>
            {
                x.Service<dynamic> (sc =>
                {
                    sc.ConstructUsing(s => obj);
                    sc.WhenStarted((s, hostControl) => s.Start(hostControl));
                    sc.WhenStopped((s, hostControl) => s.Stop(hostControl));
                });
                //x.StartAutomatically();
                x.SetServiceName("1. Service Template"); //this will rename your service (default is assembly name - 'ServiceTemplate')
                x.SetDisplayName("1. Service Template Display"); 
                x.SetDescription("Service Template Description");
                if (runAsTest)
                {
                    x.UseTestHost();
                }
                x.OnException(ex =>
                {
                    _log.Fatal($"Terminating service: {ex.Message}", ex);
                });
                x.EnableServiceRecovery(rc =>
                {
                    /* rc.OnCrashOnly();    // restart the service only after crash */
                    rc.RestartService(1);   // first failure
                    //rc.RestartService(5);   // second failure
                    //rc.RestartComputer(15, "");   // third failure
                    rc.SetResetPeriod(-1); //0 will only use first failure
                });
            });

            int exitCodeValue = (int)Convert.ChangeType(exitCode, exitCode.GetTypeCode());
            Environment.ExitCode = exitCodeValue;
            _log.Info($"{type.Name} finished with exit code: [" + exitCode.ToString() + "]");
        }
    }

    //Still working on ServiceManager
#if DEBUG
    //TODO:
    //when one service crashes, the rest continue (host will restart entire service.. what happens to running individuals? prob GC?)
    //when one services stops, it stops the host
    //...create a handler inside ServiceManager to handle those individual Services ->
    //options to restart individual services, or stop all services and stop host, or let individual service end
    internal class ServiceManager
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        internal class ServiceData
        {
            internal string Name { get; }
            internal TimeSpan? Interval { get; }
            internal bool? IntervalBuffer { get; }
            internal TimeSpan? Timeout { get; }

            /// <summary>
            /// Create a new service to run using all DEFAULT values
            /// </summary>
            internal ServiceData()
            {
                Name = null;
                Interval = null;
                IntervalBuffer = null;
                Timeout = null;
            }

            /// <summary>
            /// Create a new service to run
            /// </summary>
            /// <param name="interval">Time between runs. Null = default set in class.</param>
            /// <param name="name">Custom Service name (log). Null = Class name.</param>
            /// <param name="intervalBuffer">If true, will set at least {interval} buffer time between finish and start of new call.</param>
            /// <param name="timeout">Time allowed for call to run before throwing timeout exception.</param>
            internal ServiceData(TimeSpan? interval, string name = null, bool? intervalBuffer = null, TimeSpan? timeout = null)
            {
                Name = name;
                Interval = interval;
                IntervalBuffer = intervalBuffer;
                Timeout = timeout;
            }
        }

        //Name Services or pass null to just use ClassName
        internal static readonly List<ServiceData> service_list = new List<ServiceData>()
        {
            new ServiceData(), //default values
            new ServiceData(TimeSpan.FromSeconds(30), "Service_2"),
            new ServiceData(TimeSpan.FromMinutes(2.5), "LongService.Service_3", false, TimeSpan.FromMinutes(1)),
            new ServiceData(TimeSpan.FromSeconds(5), "ShortService.Service_4", true, TimeSpan.FromMinutes(1))
        };
        internal List<MyService> services;

        internal ServiceManager()
        {
            _log.Verbose("Constructor!");
            services = new List<MyService>();
            foreach(var s in service_list)
            {
                services.Add(new MyService(s.Interval, s.Name, s.IntervalBuffer, s.Timeout));
            }
        }
        internal bool Start(HostControl hostControl)
        {
            _log.Verbose("Starting!");
            try
            {
                foreach (var s in services)
                {
                    if (!s.Start(hostControl))
                        throw new Exception($"{s.NAME} failed to start!");
                }
            }
            catch(Exception ex) 
            {
                _log.Fatal("Error starting services", ex);
                return false; 
            }

            return true;
        }
        internal bool Stop(HostControl hostControl)
        {
            _log.Verbose("Stopping!");
            try
            {
                foreach (var s in services)
                {
                    if (!s.Stop(hostControl))
                        throw new Exception($"{s.NAME} failed to stop!");
                }
            }
            catch (Exception ex)
            {
                _log.Fatal("Error stopping services", ex);
                return false;
            }

            return true;
        }
    }
#endif
}
