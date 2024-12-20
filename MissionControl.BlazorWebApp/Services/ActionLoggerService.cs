using log4net;
using log4net.Config;
using MissionControl.Infrastructure.Logger;
using System.Reflection;

namespace MissionControl.BlazorWebApp.Services
{
    public class ActionLoggerService
    {
        public ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public ActionLogger Logger;

        public IEnumerable<string> Logs => Logger.Logs;

        public ActionLoggerService()
        {
            var repository = LogManager.GetRepository();
            foreach (var appender in repository.GetAppenders())
            {
                repository.ResetConfiguration(); // Remove all existing appenders
            }

            Logger = new ActionLogger();
            BasicConfigurator.Configure(Logger); // Configuring to use ActionLogger
        }
    }
}
