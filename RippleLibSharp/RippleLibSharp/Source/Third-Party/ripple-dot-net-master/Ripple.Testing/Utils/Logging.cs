using System.Diagnostics;
using System.Net;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;

namespace Ripple.Testing.Utils
{
    public class Logging
    {
        // Easier to use than typeof()
        public static ILog ForContext()
        {
            // TODO: is this portable ?
            var stackTrace = new StackTrace();
            var methodBase = stackTrace.GetFrame(1).GetMethod();
            var type = methodBase.DeclaringType;
            return LogManager.GetLogger(type);
        }

        // Just send everything to some dash and setup the filtering
        // there.
        public static void ConfigureUdpLogging(string ip = "127.0.0.1",
                                               int port = 8080,
                                               Level level = null)
        {
            var appender = CreateUdpAppender(ip, port);
            ConfigureRoot(level, appender);
        }

        private static void ConfigureRoot(Level level, IAppender appender)
        {
            var hierarchy = (Hierarchy) LogManager.GetRepository();
            hierarchy.Root.AddAppender(appender);
            hierarchy.Root.Level = level ?? Level.All;
            hierarchy.Configured = true;
        }

        private static UdpAppender CreateUdpAppender(string ip, int port)
        {
            var appender = new UdpAppender
            {
                RemotePort = port,
                RemoteAddress = IPAddress.Parse(ip),
                Layout = new XmlLayout(true)
            };
            appender.ActivateOptions();
            return appender;
        }

        public static void ConfigureConsoleLogging(Level level = null, string patternFormat=null)
        {
            const string defaultFormat =
                "%date [%thread] %-5level %logger -" +
                " %message %newline";
            var pattern = new PatternLayout(patternFormat ?? defaultFormat);
            var consoleAppender = new ConsoleAppender
            {
                Layout = pattern
            };
            consoleAppender.ActivateOptions();
            ConfigureRoot(Level.All, consoleAppender);
        }

    }
}
