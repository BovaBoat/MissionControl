using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionControlLib
{
    public struct MqttCommunicationConfig
    {
        public string BrokerAddress;
        public string NavControlTopic;
        public string BoatResponseTopic;

        public MqttCommunicationConfig(string brokerAddress, string navControlTopic, string boatResponseTopic)
        {

            if(string.IsNullOrEmpty(brokerAddress)
                && string.IsNullOrEmpty(navControlTopic)
                && string.IsNullOrEmpty(boatResponseTopic))
            {
                throw new Exception("Invalid mqtt config");
            }

            BrokerAddress = brokerAddress;
            NavControlTopic = navControlTopic;
            BoatResponseTopic = boatResponseTopic;
        }        
    }
}
