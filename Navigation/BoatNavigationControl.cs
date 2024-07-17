using MQTTnet;
using MQTTnet.Client;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text;
using Waypoint;

namespace Navigation
{
    public class BoatNavigationControl
    {
        private int START_MISSION_PAYLOAD_SIZE = 11;
        private int RESPONSE_TIMEOUT = 2000;

        private IMqttClient? _mqttClient;
        private MqttFactory? _mqttFactory;
        private MqttCommunicationConfig _mqttCommunicationConfig;
        private bool _isConfigured = false;
        private AutoResetEvent _isResponseReceived;
        private NavMessage _responseMessage;
        private object _responseLock = new object();

        #region Public methods

        public void Configure(MqttCommunicationConfig mqttConfig)
        {
            _mqttCommunicationConfig = mqttConfig;
            _mqttFactory = new MqttFactory();
            _mqttClient = _mqttFactory.CreateMqttClient();
            _isResponseReceived = new AutoResetEvent(false);

            _isConfigured = true;
        }

        public async Task StartCommunication()
        {
            if (!_isConfigured)
            {
                throw new Exception("Communication parameters are not configured");
            }

            var mqttClientOptions = new MqttClientOptionsBuilder().WithTcpServer(_mqttCommunicationConfig.BrokerAddress).Build();
            _mqttClient!.ApplicationMessageReceivedAsync += OnApplicationMessageReceivedAsync;
            await _mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);
            await SubscribeToTopic(_mqttCommunicationConfig.BoatResponseTopic);
        }

        private async Task OnApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
        {
            lock (_responseLock)
            {
                Trace.WriteLine("Received application message.");
                var payloadString = System.Text.Encoding.Default.GetString(e.ApplicationMessage.PayloadSegment);
                var payloadBytesList = Encoding.ASCII.GetBytes(payloadString).ToList();
                _responseMessage = new NavMessage(payloadBytesList);

                _isResponseReceived.Set();
            }
        }

        public async Task StartMission(Coordinates destinationCoordinates)
        {
            if (!_isConfigured)
            {
                throw new Exception("Communication parameters are not configured");
            }

            try
            {
                await StartMissionCommand(destinationCoordinates);
                var responsePayload = AwaitResponse(CommandsCodeEnum.START_MISSION);
                if (!IsErrorResponse((BoatResponseCodeEnum)responsePayload[0]))
                {
                    throw new Exception("Error received from boat side");
                }

                await MissionStartConfirmationCommand();

                responsePayload = AwaitResponse(CommandsCodeEnum.GREEN_LIGTH);
                if (!IsErrorResponse((BoatResponseCodeEnum)responsePayload[0]))
                {
                    throw new Exception("Error received from boat side");
                }
            }
            catch (ResponseTimeoutException ex)
            {
                Console.WriteLine($"{ex.Message}");
            }
        }

        private async Task StartMissionCommand(Coordinates destinationCoordinates)
        {
            var payload = new List<byte>
            {
                    (byte)CommandsCodeEnum.START_MISSION
            };

            payload.AddRange(destinationCoordinates.ToByteList());

            await SendMessage(payload, _mqttCommunicationConfig.NavControlTopic);
        }

        private bool IsErrorResponse(BoatResponseCodeEnum reponse)
        {
            return reponse == BoatResponseCodeEnum.OK;
        }

        private async Task MissionStartConfirmationCommand()
        {
            var payload = new List<byte>
            {
                (byte)CommandsCodeEnum.GREEN_LIGTH
            };

            await SendMessage(payload, _mqttCommunicationConfig.NavControlTopic);
        }

        private List<byte> AwaitResponse(CommandsCodeEnum commandCode)
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

            if (_responseMessage.CommandCode != commandCode)
            {
                throw new Exception("Wrong command code received in response");
            }

            return _responseMessage.Payload;
        }

        public async Task SendMessage(List<byte> payload, string topic)
        {
            _mqttFactory = new MqttFactory();
            _mqttClient = _mqttFactory.CreateMqttClient();

            var mqttClientOptions = new MqttClientOptionsBuilder().WithTcpServer(_mqttCommunicationConfig.BrokerAddress).Build();
            await _mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

            var message = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(payload)
            .WithRetainFlag(false)
            .Build();

            await _mqttClient.PublishAsync(message, CancellationToken.None);
        }

        #endregion

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



        public void WaypointTransfer()
        {

        }

        public void GetBoatLocation()
        {

        }

        public void EndCommunication()
        {

        }
    }
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
}
