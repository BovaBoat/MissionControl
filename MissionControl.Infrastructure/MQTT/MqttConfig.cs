namespace MissionControl.Infrastructure.Mqtt
{
    public class MqttConfig
    {
        public string BrokerAddress { get; }
        public string PublishTopic { get; }
        public string SubscribeTopic { get; }

        public MqttConfig(string brokerAddress, string publishTopic, string subscribeTopic)
        {

            if (string.IsNullOrEmpty(brokerAddress)
                && string.IsNullOrEmpty(publishTopic)
                && string.IsNullOrEmpty(subscribeTopic))
            {
                throw new Exception("Invalid mqtt config");
            }

            BrokerAddress = brokerAddress;
            PublishTopic = publishTopic;
            SubscribeTopic = subscribeTopic;
        }
    }
}
