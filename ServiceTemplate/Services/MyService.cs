using log4net;
using Log4NetLibrary;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
//using System.Timers;
using System.Threading;
using Topshelf;

namespace ServiceTemplate.Services
{
    internal class MyService
    {
        //DEFAULTS for manual service and null ServiceManager params
        private static readonly TimeSpan DEFAULT_INTERVAL = TimeSpan.FromSeconds(5);
        private static readonly TimeSpan DEFAULT_TIMEOUT = TimeSpan.FromSeconds(9); //random run time up to 10s in TestProject -> 10% percent chance to crash
        private static readonly bool DEFAULT_INTERVAL_BUFFER = false;
        private static readonly string DEFAULT_NAME = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.FullName;

        private HostControl _hostControl;
        private readonly string _name;
        private readonly ILog _log;
        private readonly Timer _timer;
        private bool _kill_timer = false; //need a flag to stop resetting timer when service is stopped
        private readonly TimeSpan _interval;
        private readonly TimeSpan _timeout;
        private readonly bool _interval_buffer;

        /// <summary>
        /// Gets the name of the service
        /// </summary>
        internal string NAME { get { return _name; } }

        /// <summary>
        /// Manual Constructor
        /// Use this if you don't want to use the ServiceManager
        /// </summary>
        internal MyService()
        {
            _name = DEFAULT_NAME;
            _log = LogManager.GetLogger(_name);
            _interval = DEFAULT_INTERVAL;
            _timeout = DEFAULT_TIMEOUT;
            _interval_buffer = DEFAULT_INTERVAL_BUFFER;
            _timer = new Timer(OnTimedEvent, null, -1, -1);
            _log.Debug($"Constructor!");
        }

        /// <summary>
        /// Constructer called by the ServiceManager
        /// </summary>
        /// <param name="interval">time between calls</param>
        /// <param name="name">Custom Service name (for logging purposes). Null = Class name.</param>
        /// <param name="interval_buffer">if true, will set at least {interval} buffer time between finish and start of new call</param>
        /// <param name="timeout">time allowed for call to run before throwing timeout exception</param>
        internal MyService(TimeSpan? interval, string name = null, bool? interval_buffer = null, TimeSpan? timeout = null)
        {
            _name = name ?? DEFAULT_NAME;
            _log = LogManager.GetLogger(_name);
            _interval = interval?? DEFAULT_INTERVAL;
            _timeout = timeout ?? DEFAULT_TIMEOUT;
            _interval_buffer = interval_buffer ?? DEFAULT_INTERVAL_BUFFER;
            _timer = new Timer(OnTimedEvent, null, -1, -1);
            _log.Debug($"Constructor!");
        }

        internal bool Start(HostControl hostControl)
        {
            try
            {
                _log.Info($"Started!");
                _hostControl = hostControl;
                var time_left = (_interval.TotalHours < 1) ?
                    (_interval.TotalMinutes < 1) ?
                    $"{_interval.TotalSeconds:0.#} second" :
                    $"{_interval.TotalMinutes:0.#} minute" :
                    $"{_interval.TotalHours:0.#} hour";
                _log.Verbose($"Starting with {time_left} intervals.");
                StartTimer();
                return true;
            }
            catch(Exception ex)
            {
                _log.Error("Failed to start", ex);
                return false; 
            }
        }

        internal bool Stop(HostControl hostControl)
        {
            try
            {
                _log.Info($"Stopped!");
                _timer.Change(-1, -1);
                _kill_timer = true;
                return true;
            }
            catch (Exception ex)
            {
                _log.Error("Failed to stop", ex);
                return false;
            }
        }

        private void OnTimedEvent(object source)
        {
            var sw = Log4NetHelper.Helper<Stopwatch>.Init(x => x.Start());

            try
            {
                var task = Task.Run(() =>
                {
                    TestProject.Test.Run(_name);
                });
                if (!task.Wait(_timeout))
                    throw new Exception($"Task Timeout");

                _log.Notice($"Finished in {TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds).TotalSeconds} seconds");
                StartTimer();
            }
            catch (Exception ex)
            {
                _log.Error($"Crashed after {TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds).TotalSeconds} seconds", ex);
                ForceCrash();
            }
        }

        /// <summary>
        /// Force service to end - testing purposes
        /// </summary>
        private void ForceCrash()
        {
            // 50/50 chance to exit abnormally or throw exception
            if (new Random().Next(2) % 2 == 0)
            {
                _log.Debug($"[{Environment.CurrentManagedThreadId}] Abnormal Exit");
                _hostControl.Stop(TopshelfExitCode.AbnormalExit);
            }
            else
            {
                _log.Debug($"[{Environment.CurrentManagedThreadId}] Stop Exception Thrown");
                throw new Exception("Stop Exception");
            }
        }

        private void StartTimer()
        {
            if (_kill_timer)
                return;

            var ts = GetNewInterval(_interval.TotalSeconds, _interval_buffer);
            var time_left = (ts.TotalHours < 1) ?
                (ts.TotalMinutes < 1) ?
                $"{Math.Ceiling(ts.TotalSeconds)} seconds" :
                $"{ts.TotalMinutes:0.0} minutes" :
                $"{ts.TotalHours:0.0} hours";
            _log.Debug($"Next run in {time_left}.");
            
            _timer.Change(ts, TimeSpan.FromMilliseconds(-1));
        }

        private static TimeSpan GetNewInterval(double seconds_per_interval, bool intervalBuffer)
        {
            //find the remaining time between now and next interval based off seconds
            var now = DateTime.UtcNow;
            var now_plus = now.AddSeconds(seconds_per_interval);
            var mod = seconds_per_interval - now_plus.Second % seconds_per_interval;

            //if we're using interval buffer -> add the extra interval time (if needed)
            if (intervalBuffer && mod < seconds_per_interval)
                mod += seconds_per_interval;

            //get difference between now_plus + mod - now (ZERO out ms)
            var diff = new TimeSpan(0, 0, 0, (int)(mod), -now_plus.Millisecond);

            //return the next interval time
            return diff;
        }
    }
}
