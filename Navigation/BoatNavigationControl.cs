using MQTTnet;
using MQTTnet.Client;
using System.Diagnostics;
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
        private ManualResetEvent _isResponseReceived;
        private NavMessage _responseMessage;
        private object _responseLock = new object();

        #region Public methods

        public void Configure(MqttCommunicationConfig mqttConfig)
        {
            _mqttCommunicationConfig = mqttConfig;
            _mqttFactory = new MqttFactory();
            _mqttClient = _mqttFactory.CreateMqttClient();
            _isResponseReceived = new ManualResetEvent(false);

            _isConfigured = true;
        }

        public async Task StartCommunication()
        {
            if (!_isConfigured)
            {
                throw new Exception("Communication parameters are not configured");
            }

            var mqttClientOptions = new MqttClientOptionsBuilder().WithTcpServer(_mqttCommunicationConfig.BrokerAddress).Build();

            _mqttClient!.ApplicationMessageReceivedAsync += async e =>
            {
                lock (_responseLock)
                {
                    Trace.WriteLine("Received application message.");
                    var payloadString = System.Text.Encoding.Default.GetString(e.ApplicationMessage.PayloadSegment);

                    var payloadBytesList = Encoding.ASCII.GetBytes(payloadString).ToList();
                    _responseMessage = new NavMessage(payloadBytesList);

                    _isResponseReceived.Set();
                }
            };

            await _mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

            await SubscribeToTopic(_mqttCommunicationConfig.BoatResponseTopic);

            Console.WriteLine("MQTT client subscribed to topic.");
        }

        public async Task StartMission(Coordinates destinationCoordinates)
        {
            if (!_isConfigured)
            {
                throw new Exception("Communication parameters are not configured");
            }

            var payload = new List<byte>
            {
                (byte)CommandsCodeEnum.START_MISSION
            };

            payload.AddRange(CoordinatesToByteArray(destinationCoordinates));

            await SendMessage(payload, _mqttCommunicationConfig.NavControlTopic);

            try
            {
                AwaitResponse(_mqttCommunicationConfig.BoatResponseTopic);
            }
            catch (ResponseTimeoutException ex)
            {
                Console.WriteLine($"{ex.Message}");
            }           
        }

        private void AwaitResponse(string topicName)
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

            Console.WriteLine("Response received");
        }

        class ResponseTimeoutException : Exception
        {
            public ResponseTimeoutException(string message)
            : base(message)
            { }
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

            // Publish the message
            await _mqttClient.PublishAsync(message, CancellationToken.None);
            Console.WriteLine("Message published.");
        }

        #endregion

        private List<byte> CoordinatesToByteArray(Coordinates coordinates)
        {
            var lattitudeBytes = BitConverter.GetBytes(coordinates.GetLatitude());
            var lattitudeBytesList = new List<byte>(lattitudeBytes);

            var longtiudeBytes = BitConverter.GetBytes(coordinates.GetLongitude());
            var longtiudeBytesList = new List<byte>(longtiudeBytes);

            longtiudeBytesList.AddRange(longtiudeBytesList);

            return longtiudeBytesList;
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
        GET_LOCATION = 0x02
    }
}
