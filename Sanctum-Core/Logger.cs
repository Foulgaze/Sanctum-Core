using System;
using System.IO;

namespace Sanctum_Core
{
    internal class Logger
    {
        public static event Action<string> log = delegate { };
        public static event Action<string> errorLog = delegate { };

        private static readonly TextWriter errorLogger = Console.Error;
        public static void Log(string message)
        {
            log(message);
            Console.WriteLine(message);
        }

        public static void LogError(string message)
        {
            errorLog(message);
            errorLogger.WriteLine(message);
        }
    }
}
