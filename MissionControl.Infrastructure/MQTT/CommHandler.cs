using MQTTnet.Client;
using MQTTnet;
using System.Text;
using System.Diagnostics;
using MissionControl.Shared.Enums;
using MissionControl.Shared.DataTransferObjects;
using MissionControlLib.Exceptions;

namespace MissionControlLib.Infrastructure
{
    public class CommHandler
    {

        private IMqttClient? _mqttClient;
        private MqttFactory? _mqttFactory;
        private MqttCommunicationConfig _mqttCommunicationConfig;
        private object _responseLock = new object();

        public delegate void MessageSentEventHandler(NavMessage message);
        public event MessageSentEventHandler MessageSent;

        public delegate void MessageReceivedEventHandler(NavMessage message);
        public event MessageReceivedEventHandler MessageReceived;

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

        public async Task<bool> SendMessage(NavMessage navMessage)
        {
            var mqttMessage = new MqttApplicationMessageBuilder()
            .WithTopic(_mqttCommunicationConfig.PublishTopic)
            .WithPayload(navMessage.GetMessageContentByteArray())
            .WithRetainFlag(false)
            .Build();

            var result = await _mqttClient.PublishAsync(mqttMessage, CancellationToken.None);

            if (!result.IsSuccess)
            {
                return false;
            }

            MessageSent?.Invoke(navMessage);

            return true;
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
                var responseMessage = new NavMessage((CommandCodeEnum)commandByte, payloadBytesList.GetRange(1, payloadBytesList.Count - 1));

                //_isResponseReceived.Set();

                MessageReceived.Invoke(responseMessage);
            }
        }        

        #endregion
    }
}
