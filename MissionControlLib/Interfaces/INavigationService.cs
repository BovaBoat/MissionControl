using MissionControlLib.Waypoints;

namespace MissionControl.Domain.Interfaces
{
    public interface INavigationService
    {
        /// <summary>
        /// Establishes a connection for navigation services
        /// </summary>
        /// <returns>A task representing the asynchronous operation</returns>
        Task Connect();

        /// <summary>
        /// Sends a command to request periodic location reports
        /// </summary>
        /// <returns>A task representing the asynchronous operation</returns>
        Task PeriodicReportLocationCommand();
    }
}
