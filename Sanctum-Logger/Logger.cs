﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Sanctum_Core_Logger
{

    public class Logger
    {
        private static readonly TextWriter errorLogger = Console.Error;
        public static void Log(string message)
        {
            Console.WriteLine(message);
        }

        /// <summary>
        /// Logs an error message
        /// </summary>
        /// <param name="message"></param>
        public static void LogError(string message)
        {
            errorLogger.WriteLine(message);
        }
    }
}
