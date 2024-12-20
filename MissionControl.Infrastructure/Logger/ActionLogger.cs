using log4net.Core;
using log4net.Appender;
using log4net.Layout;

namespace MissionControl.Infrastructure.Logger
{
    public class ActionLogger : AppenderSkeleton
    {
        private static readonly List<string> _logs = new List<string>();

        public IEnumerable<string> Logs => _logs;

        public ActionLogger()
        {
            this.Layout = new SimpleLayout();
        }

        protected override void Append(LoggingEvent loggingEvent)
        {
            var message = RenderLoggingEvent(loggingEvent);
            _logs.Add(message);
        }
    }
}
