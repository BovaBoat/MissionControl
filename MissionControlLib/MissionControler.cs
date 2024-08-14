using MissionControlLib.Waypoints;
using MissionControlLib.Infrastructure;
using MissionControl.Shared.Enums;
using MissionControl.Shared.DataTransferObjects;
using System.Diagnostics;
using MissionControlLib.Exceptions;

namespace MissionControl.Domain
{
    public class MissionControler
    {
        #region Constants

        private int RESPONSE_TIMEOUT = 6000;

        #endregion

        private bool _isConfigured = false;
        public bool IsMissionInProgress = false;
        private AutoResetEvent _isMissionEnded = new AutoResetEvent(false);
        private CommHandler _commHandler;
        private NodeConfig _nodeConfig;
        private CommandCodeEnum _expectedResponseCmdCode;
        private AutoResetEvent _isResponseReceived = new AutoResetEvent(false);
        private bool _isExpectingResponse = false;
        private NavMessage _responseMessage; 

        #region Events

        public delegate void MessageSentEventHandler(NavMessage message, string messageSenderName);
        public delegate void MessageReceivedEventHandler(NavMessage message, string messageSenderName);
        public delegate void LocationUpdateRxEvenetHandler(Coordinates locationCoordinates);

        public event MessageSentEventHandler MessageSent;
        public event MessageReceivedEventHandler MessageReceived;

        public event LocationUpdateRxEvenetHandler LocationUpdateReceived;

        #endregion

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
            _commHandler.MessageReceived += MessageReceivedHandler;

            await _commHandler.ConnectToBrokerAndSubscribe();
        }

        public async Task<NavMessage> AwaitResponse(CommandCodeEnum commandCode)
        {
            var timeoutStopwatch = Stopwatch.StartNew();

            while (true)
            {
                if (timeoutStopwatch.ElapsedMilliseconds > RESPONSE_TIMEOUT)
                {
                    throw new ResponseTimeoutException("Timeout occured while awaiting response");
                }

                if (_isResponseReceived.WaitOne(10))
                {
                    break;
                }
            }

            if (_responseMessage!.CommandCode != commandCode)
            {
                throw new Exception("Wrong command code received in response");
            }

            //MessageReceived.Invoke(_responseMessage, _nodeConfig.VesselName);

            return _responseMessage;
        }

        public async Task StartMission(Coordinates destinationCoordinates)
        {
            if (!_isConfigured)
            {
                throw new InvalidOperationException("Mission control is not configured");
            }

            await StartMissionCommand(destinationCoordinates);
            var response = await AwaitResponse(CommandCodeEnum.START_MISSION);

            await MissionStartConfirmationCommand();
            response = await AwaitResponse(CommandCodeEnum.GREEN_LIGTH);

            IsMissionInProgress = true;
        }

        #endregion Public Methods

        #region Private Methods

        public async Task PeriodicReportLocationCommand()
        {
            var reportLocationTimeoutSeconds = 30;
            var timeoutInBytes = BitConverter.GetBytes(reportLocationTimeoutSeconds);

            var message = new NavMessage(CommandCodeEnum.PERIODIC_REPORT_LOCATION, timeoutInBytes.ToList<byte>());

            await SendCommand(message, isResponseExpected: true);

            var response = await AwaitResponse(CommandCodeEnum.PERIODIC_REPORT_LOCATION);
        }

        private Coordinates LocationReceived(List<byte> coordinatesInBytes)
        {
            var locationCoordinates = Coordinates.FromByteList(coordinatesInBytes);

            return locationCoordinates;
        }

        private async Task StartMissionCommand(Coordinates destinationCoordinates)
        {
            var navMessage = new NavMessage(CommandCodeEnum.START_MISSION, destinationCoordinates.ToByteList());

            await SendCommand(navMessage, isResponseExpected: true);
        }

        private async Task MissionStartConfirmationCommand()
        {
            var navMessage = new NavMessage(CommandCodeEnum.GREEN_LIGTH);

            await SendCommand(navMessage, isResponseExpected: true);
        }

        private async Task SendCommand(NavMessage navMessage, bool isResponseExpected = false)
        {
            var result = await _commHandler.SendMessage(navMessage);

            if (result && isResponseExpected)
            {
                _isExpectingResponse = true;
                _expectedResponseCmdCode = navMessage.CommandCode;
            }
        }

        #endregion Private Methods

        #region Event Handlers

        public void MessageSentHandler(NavMessage message)
        {
            MessageSent?.Invoke(message, _nodeConfig.MissionControlName);
        }

        public void MessageReceivedHandler(NavMessage message)
        {
            Console.WriteLine(message.CommandCode);
            if (message.CommandCode == CommandCodeEnum.GET_LOCATION)
            {
                var coordinates = Coordinates.FromByteList(message.Payload.ToList<byte>());
                LocationUpdateReceived.Invoke(coordinates);
            }

            if (_isExpectingResponse && message.CommandCode == _expectedResponseCmdCode)
            {
                _isExpectingResponse = false;
                _responseMessage = message;
                _isResponseReceived.Set();
            }

            //MessageReceived?.Invoke(message, _nodeConfig.VesselName);
        }

        #endregion
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
