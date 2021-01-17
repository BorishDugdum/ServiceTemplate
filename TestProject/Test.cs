using log4net;
using Log4NetLibrary;
using System;
using System.Threading;

namespace TestProject
{
    public static class Test
    {
        private static readonly ILog _log = LogManager.GetLogger("QWERTY.SHMERTY");// System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static void Run(string name)
        {
            _log.Verbose($"Running {name} as {Environment.UserName}");

            //Mimic proc time - randomize time to finish
            //DEFAULT_TIMEOUT is 9 seconds - so this will give a 10% chance to crash
            Thread.Sleep(new Random().Next((int)TimeSpan.FromSeconds(10).TotalMilliseconds)); //randomize time to finish
        }
    }
}
