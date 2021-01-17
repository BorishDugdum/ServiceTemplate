using log4net;
using Topshelf;
using Log4NetLibrary;

namespace ServiceTemplate.Services
{
    internal class TestService : ServiceControl
    {
        private readonly ILog _log;

        public TestService()
        {
            var name = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.FullName;
            _log = LogManager.GetLogger(name);
            _log.Info("Test Service Created!");

            Log4NetHelper.ListLogLevels();
        }

        public bool Start(HostControl hostControl)
        {
            _log.Info("Test Service Started!");
            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            _log.Info("Test Service Stopped!");
            return true;
        }
    }
}
