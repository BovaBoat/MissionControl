using MQTTnet.Client;
using MQTTnet;
using MissionControl.Infrastructure.Exceptions;

namespace MissionControlLib.Infrastructure
{
    public class MqttCommHandler
    {
        private IMqttClient? _mqttClient;
        private MqttFactory? _mqttFactory;
        private MqttCommunicationConfig _mqttCommunicationConfig;
        private object _responseLock = new object();

        public delegate void MessageSentEventHandler(List<byte> messagePayload);
        public event MessageSentEventHandler? MessageSent;

        public delegate void MessageReceivedEventHandler(List<byte> messagePayload);
        public event MessageReceivedEventHandler? MessageReceived;

        public MqttCommHandler(MqttCommunicationConfig mqttConfig)
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

        public async Task SendMessage(List<byte> payload)
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

        #region Event handlers

        private async Task OnApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
        {
            lock (_responseLock)
            {
                var payload = e.ApplicationMessage.PayloadSegment.ToList<byte>();

                MessageReceived?.Invoke(payload);
            }
        }        

        #endregion
    }
}
