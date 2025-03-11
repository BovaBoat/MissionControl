using MQTTnet.Client;
using MQTTnet;
using MissionControl.Infrastructure.Exceptions;
using MissionControl.Infrastructure.Interfaces;
using MissionControl.Infrastructure.Mqtt;
using MissionControl.Domain.Entities;
using MissionControl.Shared.Enums;
using MissionControlLib.Waypoints;
using MissionControl.Domain.Exceptions;
using System.Diagnostics;

namespace MissionControl.Infrastructure
{
    public class MqttComService : IComService
    {
        #region Constants

        private int RESPONSE_TIMEOUT = 6000;

        #endregion

        private IMqttClient? _mqttClient;
        private MqttFactory? _mqttFactory;
        private MqttConfig _mqttCommunicationConfig;
        private object _responseLock = new object();

        public delegate void MessageSentEventHandler(List<byte> messagePayload);
        public event MessageSentEventHandler? MessageSent;

        public delegate void MessageReceivedEventHandler(List<byte> messagePayload);
        public event MessageReceivedEventHandler? MessageReceived;

        private bool _isConfigured = false;
        private SeaBusMessage _responseMessage;
        private AutoResetEvent _isResponseReceived = new AutoResetEvent(false);
        private bool _isExpectingResponse = false;
        private CommandCodeEnum _expectedResponseCmdCode;

        public MqttComService(MqttConfig mqttConfig)
        {
            _mqttCommunicationConfig = mqttConfig;
            _mqttFactory = new MqttFactory();
            _mqttClient = _mqttFactory.CreateMqttClient();
        }

        public async Task Connect()
        {
            MessageSent += MessageSentHandler;
            MessageReceived += MessageReceivedHandler;

            await ConnectToBrokerAndSubscribe();
        }

        public async Task ConnectToBrokerAndSubscribe()
        {
            var mqttClientOptions = new MqttClientOptionsBuilder().WithWebSocketServer(_mqttCommunicationConfig.BrokerAddress).Build();
            _mqttClient!.ApplicationMessageReceivedAsync += OnApplicationMessageReceivedAsync;
            var result = await _mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

            if (result.ResultCode != MqttClientConnectResultCode.Success)
            {
                throw new Exception("Failed to connect");
            }

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

            var result = await _mqttClient!.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);

            if (result.Items.Any(item => item.ResultCode != MqttClientSubscribeResultCode.GrantedQoS0))
            {
                throw new Exception($"Failed to subscribe to topic {topic}");
            }
        }

        public async Task SendBytes(List<byte> payload)
        {
            var mqttMessage = new MqttApplicationMessageBuilder()
            .WithTopic(_mqttCommunicationConfig.PublishTopic)
            .WithPayload(payload)
            .WithRetainFlag(false)
            .Build();

            var result = await _mqttClient!.PublishAsync(mqttMessage, CancellationToken.None);

            if (!result.IsSuccess)
            {
                throw new FailedToPublishMessageException("Error occured while publishing message.", payload);
            }

            MessageSent?.Invoke(payload);
        }

        private async Task SendMessage(List<byte> payload, bool isResponseExpected = false)
        {
            await SendBytes(payload);

            if (isResponseExpected)
            {
                _isExpectingResponse = true;
            }
        }

        #region Event Handlers

        public void MessageSentHandler(List<byte> messageContent)
        {
            var seaBusMessage = new SeaBusMessage((CommandCodeEnum)messageContent[0], messageContent);
            MessageSent?.Invoke(seaBusMessage);
        }

        public void MessageReceivedHandler(List<byte> messageContent)
        {
            var seaBusMessage = new SeaBusMessage((CommandCodeEnum)messageContent[0], messageContent);
            if (seaBusMessage.CommandCode == CommandCodeEnum.GET_LOCATION)
            {
                var coordinates = Coordinates.FromByteList(seaBusMessage.Payload.GetRange(1, 8));
                LocationUpdateReceived?.Invoke(coordinates);
            }

            if (_isMissionInProgress && seaBusMessage.CommandCode == CommandCodeEnum.MISSION_END)
            {
                _isMissionInProgress = false;
            }

            if (_isExpectingResponse && seaBusMessage.CommandCode == _expectedResponseCmdCode)
            {
                _isExpectingResponse = false;
                _responseMessage = seaBusMessage;
                _isResponseReceived.Set();
            }

            MessageReceived?.Invoke(seaBusMessage);
        }

        #region Private Methods

        private SeaBusMessage AwaitResponse(CommandCodeEnum commandCode)
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

            MessageReceived?.Invoke(_responseMessage);

            return _responseMessage;
        }

        #endregion

        #region Events

        public delegate void LocationUpdateRxEvenetHandler(Coordinates locationCoordinates);

        public event LocationUpdateRxEvenetHandler? LocationUpdateReceived;

        #endregion

        #region Event handlers

        private async Task OnApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
        {

                var payload = e.ApplicationMessage.PayloadSegment.ToList<byte>();

                MessageReceived?.Invoke(payload);
        }        

        #endregion
    }
}
