using MissionControlLib;
using MissionControlLib.Waypoints;
using NUnit.Framework;

namespace MissionControlLib.Test.Integration
{
    [TestFixture]
    public class MissionControlIntegrationTests
    {
        const string BROKER_ADDRESS = "test.mosquitto.org";
        const string MISSION_CONTROL_COMMAND_TOPIC = "brod/navControl";
        const string RESPONSE_TOPIC = "brod/boatResponse";

        MqttCommunicationConfig communicationConfig = new MqttCommunicationConfig
        {
            BrokerAddress = BROKER_ADDRESS,
            NavControlTopic = MISSION_CONTROL_COMMAND_TOPIC,
            BoatResponseTopic = RESPONSE_TOPIC
        };


        [Test]
        public async Task SendMissionStartCommand_WhenMessagePayloadIsValid_ShouldNotThrow()
        {
            var missionControl = new MissionControl();
            var mockCoordinates = new Coordinates(0, 0);

            missionControl.Configure(communicationConfig);

            await missionControl.StartMission(mockCoordinates);

            Assert.DoesNotThrowAsync(async () => await missionControl.StartMission(mockCoordinates));
        }
    }

    public class MockBoatComm
    {
        const string BROKER_ADDRESS = "test.mosquitto.org";
        const string MISSION_CONTROL_COMMAND_TOPIC = "brod/navControl";
        const string RESPONSE_TOPIC = "brod/boatResponse";

        MqttCommunicationConfig CommunicationConfig = new MqttCommunicationConfig
        {
            BrokerAddress = BROKER_ADDRESS,
            NavControlTopic = MISSION_CONTROL_COMMAND_TOPIC,
            BoatResponseTopic = RESPONSE_TOPIC
        };
    }
}