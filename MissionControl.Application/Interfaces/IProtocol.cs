using MissionControlLib.Waypoints;
using System;
namespace MissionControl.Application.Interfaces
{
    public interface IProtocolService
    {
        /// <summary>
        /// Sends a command to start a mission with the specified destination coordinates
        /// </summary>
        /// <param name="destinationCoordinates">The coordinates of the destination</param>
        /// <returns>True if the command was successfully sent and acknowledged</returns>
        Task<bool> StartMissionCommand(Coordinates destinationCoordinates);
        
        /// <summary>
        /// Sends a mission start confirmation command
        /// </summary>
        /// <returns>True if the command was successfully sent and acknowledged</returns>
        Task<bool> MissionStartConfirmationCommand();
        
        Task<bool> PeriodicReportLocationCommand(int timeoutSeconds = 30);
    }
}
