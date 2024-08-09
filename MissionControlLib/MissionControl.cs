using MissionControlLib.Waypoints;
using MQTTnet;
using MQTTnet.Client;
using System.Diagnostics;
using System.Text;
using MissionControlDatabase;
using MissionControlLib.Exceptions;

namespace MissionControlLib
{
    public class MissionControl
    {
        private int RESPONSE_TIMEOUT = 2000;

        private IMqttClient? _mqttClient;
        private MqttFactory? _mqttFactory;
        private MqttCommunicationConfig _mqttCommunicationConfig;
        private bool _isConfigured = false;
        private AutoResetEvent _isResponseReceived = new AutoResetEvent(false);
        private AutoResetEvent _isMissionEnded = new AutoResetEvent(false);
        private NavMessage? _responseMessage;
        private object _responseLock = new object();
        private bool _isMissionInProgress = false;
        private DatabaseHandler _dbHandler;
        private NodeConfig _nodeNameConfig;

        #region Public methods

        public void Configure(MqttCommunicationConfig mqttConfig, DatabaseConfig dbConfig, NodeConfig nodeConfig)
        {
            _mqttCommunicationConfig = mqttConfig;
            _mqttFactory = new MqttFactory();
            _mqttClient = _mqttFactory.CreateMqttClient();

            _dbHandler = new DatabaseHandler(dbConfig);
            _nodeNameConfig = nodeConfig;

            _isConfigured = true;
        }

        public async Task StartCommunication()
        {
            if (!_isConfigured)
            {
                throw new InvalidOperationException("Communication parameters are not configured");
            }
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  
            var mqttClientOptions = new MqttClientOptionsBuilder().WithTcpServer(_mqttCommunicationConfig.BrokerAddress).Build();
            _mqttClient!.ApplicationMessageReceivedAsync += OnApplicationMessageReceivedAsync;
            await _mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);
            await SubscribeToTopic(_mqttCommunicationConfig.BoatResponseTopic);
        }

        public async Task StartMission(Coordinates destinationCoordinates)
        {
            if (!_isConfigured)
            {
                throw new InvalidOperationException("Mission control is not configured");
            }

            await SendStartMissionCommand(destinationCoordinates);
            var response = AwaitResponse(CommandsCodeEnum.START_MISSION);

            if (!IsErrorResponse((BoatResponseCodeEnum)response.Payload[0]))
            {
                throw new ErrorResponseException("Error received from boat side");
            }

            await MissionStartConfirmationCommand();

            response = AwaitResponse(CommandsCodeEnum.GREEN_LIGTH);

            if (!IsErrorResponse((BoatResponseCodeEnum)response.Payload[0]))
            {
                throw new ErrorResponseException("Error received from boat side");
            }

            _isMissionInProgress = true;

            await MonitorMission();
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

                var response = AwaitResponse(CommandsCodeEnum.GET_LOCATION);
            }
        }

        private Coordinates LocationReceived(List<byte> coordinatesInBytes)
        {
            var locationCoordinates = Coordinates.FromByteList(coordinatesInBytes);

            return locationCoordinates;
        }

        private async Task SendStartMissionCommand(Coordinates destinationCoordinates)
        {
            var navMessage = new NavMessage(CommandsCodeEnum.START_MISSION, destinationCoordinates.ToByteList());

            await SendMessage(navMessage, _mqttCommunicationConfig.NavControlTopic);
        }

        private bool IsErrorResponse(BoatResponseCodeEnum reponse)
        {
            return reponse == BoatResponseCodeEnum.OK;
        }

        private async Task MissionStartConfirmationCommand()
        {
            var navMessage = new NavMessage(CommandsCodeEnum.GREEN_LIGTH);

            await SendMessage(navMessage, _mqttCommunicationConfig.NavControlTopic);
        }

        private NavMessage AwaitResponse(CommandsCodeEnum commandCode)
        {
            if (!_isConfigured)
            {
                throw new Exception("Not configured");
            }

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

            _dbHandler.InsertMessage((int)_responseMessage.CommandCode, _responseMessage.Payload!.ToArray(), _nodeNameConfig.VesselName);

            return _responseMessage;
        }

        private async Task SendMessage(NavMessage navMessage, string topic)
        {
            _mqttFactory = new MqttFactory();
            _mqttClient = _mqttFactory.CreateMqttClient();

            var mqttClientOptions = new MqttClientOptionsBuilder().WithTcpServer(_mqttCommunicationConfig.BrokerAddress).Build();
            await _mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

            var mqttMessage = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(navMessage.Payload)
            .WithRetainFlag(false)
            .Build();

            var result = await _mqttClient.PublishAsync(mqttMessage, CancellationToken.None);

            if (!result.IsSuccess)
            {
                throw new Exception("Failed to publish message");
            }

            _dbHandler.InsertMessage((int)navMessage.CommandCode, navMessage.Payload?.ToArray(), _nodeNameConfig.MissionControlName);
        }

        private async Task SubscribeToTopic(string topic)
        {
            var mqttSubscribeOptions = _mqttFactory!.CreateSubscribeOptionsBuilder()
                .WithTopicFilter(
                    f =>
                    {
                        _ = f.WithTopic(topic);
                    })
                .Build();

            await _mqttClient!.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);
        }

        #endregion Private Methods

        #region Event handlers

        private async Task OnApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
        {
            lock (_responseLock)
            {
                Trace.WriteLine("Received application message.");
                var payloadString = System.Text.Encoding.Default.GetString(e.ApplicationMessage.PayloadSegment);
                var payloadBytesList = Encoding.ASCII.GetBytes(payloadString).ToList();

                var commandByte = payloadBytesList[0];
                _responseMessage = new NavMessage((CommandsCodeEnum)commandByte, payloadBytesList.GetRange(1, payloadBytesList.Count - 1));

                _isResponseReceived.Set();
            }
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

    #region Enums
    public enum CommandsCodeEnum : byte
    {
        None = 0,
        START_MISSION = 0x01,
        GET_LOCATION = 0x02,
        GREEN_LIGTH = 0x03,
    }

    public enum BoatResponseCodeEnum
    {
        OK,
    }

    #endregion
}
