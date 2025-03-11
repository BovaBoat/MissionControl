using MissionControlLib.Waypoints;
using MissionControl.Domain.Interfaces;
using MissionControl.Application.Interfaces;

namespace MissionControl.Application
{
    public class MissionControlService : IMissionControlService
    {
        #region Fields

        private INavigationService _navigation;
        private IProtocolService _protocol;

        private bool _isMissionInProgress = false;

        #endregion

        #region Public methods

        public MissionControlService(INavigationService navigationService, IProtocolService protocolService)
        {
            _navigation = navigationService;
            _protocol = protocolService;
        }

        public async Task<bool> StartMission(Coordinates destination)
        {
            return await _protocol.StartMissionCommand(destination);
        }

        #endregion Public Methods 
    }
}
