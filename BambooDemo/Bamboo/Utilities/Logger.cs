using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Bamboo.Utilities
{
    public enum LogLevel
    {
        Off = 0,
        Error,
        Warning,
        Info,
        All
    }

    public class Logger
    {
        static private Logger loggerInstance;
        private LogLevel currentLevel;

        private Logger()
        {
            this.currentLevel = LogLevel.All;
        }

        static public Logger GetInstance()
        {
            if (null == Logger.loggerInstance)
            {
                Logger.loggerInstance = new Logger();
            }

            return Logger.loggerInstance;
        }

        public void LogLine(string message, LogLevel level = LogLevel.Info, [CallerMemberName]string caller = "")
        {
            if (level <= this.currentLevel)
            {
                Debug.WriteLine(caller + " : " + message);
            }
        }
    }
}
