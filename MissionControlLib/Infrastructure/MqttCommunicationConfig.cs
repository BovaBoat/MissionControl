namespace MissionControlLib.Infrastructure
{
    public struct MqttCommunicationConfig
    {
        public string BrokerAddress;
        public string PublishTopic;
        public string SubscribeTopic;

        public MqttCommunicationConfig(string brokerAddress, string publishTopic, string subscribeTopic)
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
