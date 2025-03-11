using MissionControlLib.Waypoints;

namespace MissionControl.Application.Interfaces
{
    public interface IMissionControlService
    {
        Task<bool> StartMission(Coordinates destinationCoordinates);
    }
}
