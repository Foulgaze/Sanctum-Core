using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Sanctum_Core
{
    public class Logger
    {
        private static readonly TextWriter errorLogger = Console.Error;
        public static void Log(string message)
        {
            Console.WriteLine(message);
        }

        public static void LogError(string message)
        {
            errorLogger.WriteLine(message);
        }
    }
}
