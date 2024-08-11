using MQTTnet.Client;
using MQTTnet;
using System.Text;
using System.Diagnostics;
using MissionControlLib.Exceptions;
using MissionControl.Shared.Enums;
using MissionControl.Shared.DataTransferObjects;

namespace MissionControlLib.Infrastructure
{
    internal class CommHandler
    {
        private IMqttClient? _mqttClient;
        private MqttFactory? _mqttFactory;
        private MqttCommunicationConfig _mqttCommunicationConfig;

        private AutoResetEvent _isResponseReceived = new AutoResetEvent(false);
        private object _responseLock = new object();
        private NavMessage? _responseMessage;

        private int RESPONSE_TIMEOUT = 2000;

        public delegate void MessageSentEventHandler(NavMessage message);
        public event MessageSentEventHandler MessageSent;

        public CommHandler(MqttCommunicationConfig mqttConfig)
        {
            _mqttCommunicationConfig = mqttConfig;
            _mqttFactory = new MqttFactory();
            _mqttClient = _mqttFactory.CreateMqttClient();
        }

        public async Task ConnectToBrokerAndSubscribe()
        {
            var mqttClientOptions = new MqttClientOptionsBuilder().WithTcpServer(_mqttCommunicationConfig.BrokerAddress).Build();
            _mqttClient!.ApplicationMessageReceivedAsync += OnApplicationMessageReceivedAsync;
            await _mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);
            await SubscribeToTopic(_mqttCommunicationConfig.SubscribeTopic);
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

        public async Task SendMessage(NavMessage navMessage)
        {
            _mqttFactory = new MqttFactory();
            _mqttClient = _mqttFactory.CreateMqttClient();

            var mqttClientOptions = new MqttClientOptionsBuilder().WithTcpServer(_mqttCommunicationConfig.BrokerAddress).Build();
            await _mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

            var mqttMessage = new MqttApplicationMessageBuilder()
            .WithTopic(_mqttCommunicationConfig.PublishTopic)
            .WithPayload(navMessage.Payload)
            .WithRetainFlag(false)
            .Build();

            var result = await _mqttClient.PublishAsync(mqttMessage, CancellationToken.None);

            if (!result.IsSuccess)
            {
                throw new Exception("Failed to publish message");
            }

            MessageSent?.Invoke(navMessage);
        }

        public NavMessage AwaitResponse(CommandCodeEnum commandCode)
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

            if (!IsErrorResponse((BoatResponseCodeEnum)_responseMessage.Payload[0]))
            {
                throw new ErrorResponseException("Error received from boat side");
            }

            return _responseMessage;
        }

        private bool IsErrorResponse(BoatResponseCodeEnum reponse)
        {
            return reponse == BoatResponseCodeEnum.OK;
        }

        #region Event handlers

        private async Task OnApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
        {
            lock (_responseLock)
            {
                Trace.WriteLine("Received application message.");
                var payloadString = Encoding.Default.GetString(e.ApplicationMessage.PayloadSegment);
                var payloadBytesList = Encoding.ASCII.GetBytes(payloadString).ToList();

                var commandByte = payloadBytesList[0];
                _responseMessage = new NavMessage((CommandCodeEnum)commandByte, payloadBytesList.GetRange(1, payloadBytesList.Count - 1));

                _isResponseReceived.Set();
            }
        }        

        #endregion
    }
}
