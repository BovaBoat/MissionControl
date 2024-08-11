using MissionControlLib.Waypoints;
using MissionControlLib.Infrastructure;
using MissionControl.Shared.Enums;
using MissionControl.Shared.DataTransferObjects;

namespace MissionControl.Domain
{
    public class MissionControler
    {
        private bool _isConfigured = false;
        private AutoResetEvent _isMissionEnded = new AutoResetEvent(false);
        private bool _isMissionInProgress = false;
        private CommHandler _commHandler;
        private NodeConfig _nodeConfig;

        public delegate void MessageSentEventHandler(NavMessage message, string messageSenderName);
        public event MessageSentEventHandler MessageSent;

        public void MessageSentHandler(NavMessage message)
        {
            MessageSent!.Invoke(message, _nodeConfig.MissionControlName);
        }

        #region Public methods

        public void Configure(MqttCommunicationConfig mqttConfig, NodeConfig nodeConfig)
        {
            _commHandler = new CommHandler(mqttConfig);
            _nodeConfig = nodeConfig;
            _isConfigured = true;
        }

        public async Task Connect()
        {
            if (!_isConfigured)
            {
                throw new Exception("Mission control instance is not configured.");
            }

            _commHandler.MessageSent += MessageSentHandler;

            await _commHandler.ConnectToBrokerAndSubscribe();
        }

        public async Task StartMission(Coordinates destinationCoordinates)
        {
            if (!_isConfigured)
            {
                throw new InvalidOperationException("Mission control is not configured");
            }

            await StartMissionCommand(destinationCoordinates);
            var response = _commHandler.AwaitResponse(CommandCodeEnum.START_MISSION);

            await MissionStartConfirmationCommand();
            response = _commHandler.AwaitResponse(CommandCodeEnum.GREEN_LIGTH);

            _isMissionInProgress = true;
        }

        #endregion Public Methods

        #region Private Methods

        private async Task MonitorMission()
        {
            while (true)
            {
                if (_isMissionEnded.WaitOne(10))
                {
                    break;
                }

                var response = _commHandler.AwaitResponse(CommandCodeEnum.GET_LOCATION);
            }
        }

        private Coordinates LocationReceived(List<byte> coordinatesInBytes)
        {
            var locationCoordinates = Coordinates.FromByteList(coordinatesInBytes);

            return locationCoordinates;
        }

        private async Task StartMissionCommand(Coordinates destinationCoordinates)
        {
            var navMessage = new NavMessage(CommandCodeEnum.START_MISSION, destinationCoordinates.ToByteList());

            await _commHandler.SendMessage(navMessage);
        }

        private async Task MissionStartConfirmationCommand()
        {
            var navMessage = new NavMessage(CommandCodeEnum.GREEN_LIGTH);

            await _commHandler.SendMessage(navMessage);
        }

        #endregion Private Methods
    }

    #region Structures

    public struct NodeConfig
    {
        public string MissionControlName;
        public string VesselName;

        public NodeConfig(string missionControlName, string vesselName)
        {
            MissionControlName = missionControlName;
            VesselName = vesselName;
        }
    }

    #endregion
}
