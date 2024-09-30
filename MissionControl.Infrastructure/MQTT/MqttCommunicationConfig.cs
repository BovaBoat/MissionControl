namespace MissionControl.Infrastructure
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
}
