using CitizenFX.Core;
using System;
using System.IO;

namespace Logging 
{
    /// <summary>
    /// Static logger class that allows direct logging of anything to a text file
    /// </summary>
    public static class Logger
    {
        public static LogLevel Level = LogLevel.Information;

        public static void Log(object message, LogLevel logLevel = LogLevel.Information)
        {
            if(logLevel >= Level)
            {
                Debug.WriteLine(DateTime.Now + " : " + message + Environment.NewLine);
            }            
        }
    }

    public enum LogLevel
    {
        Trace,
        Debug,
        Information,
        Warning,
        Error,
        Critical,
        None
    }
}