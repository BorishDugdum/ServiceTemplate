/*Created by Warren Smith 2021
 * 
 * This project was created to be a solid template for running
 * a new Windows Service. This should allow you to focus more on 
 * what application you want to run instead of building the
 * service, error handling, and logging.
 * 
 * Created with help of various StackOverflow responses.
 * 
 */

/* NOTES
 * 
 * Using System.Threading.Tasks.Timer instead of System.Timers.Timer so TopShelf can handle unhandled exceptions (will hang otherwise)
 * Make sure to put .\ in front of username when installing with local user
 * EX: ServiceTemplate.exe install -username ".\yourUserName"
 * Don't run EnableServiceRecovery with RestartComputer when you're testing crashes >.<
 * 
 */

using System;
using System.Diagnostics;
using System.Reflection;

namespace ServiceTemplate
{
    class Program
    {
        static void Main(string[] args)
        {
            //Create log folder location
            var methodbase = MethodBase.GetCurrentMethod().DeclaringType;
            var assemblyName = methodbase.Assembly.GetName();
            var baseFilePath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\logs\{assemblyName.Name}";
            Log4NetLibrary.Log4NetHelper.Configure(baseFilePath);

            //Configure Host Services (TopShelf)
            ConfigureService.Configure(args);

            PressKeyExit(args);
        }

        [Conditional("DEBUG")]
        private static void PressKeyExit(string[] args)
        {
            //Putting this here as well because I'm lazy and running service off my debug build
            if (args.Length == 0)
            {
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey();
            }
        }
    }
}
