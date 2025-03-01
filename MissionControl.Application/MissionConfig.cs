using MissionControl.Database;
using MissionControl.Infrastructure;

namespace MissionControl.Application
{
    public class MissionConfig
    {
        public NodeConfig NodeConfig { get; }
        public MqttConfig MqttConfig { get; }
        public DatabaseConfig? DatabaseConfig { get; }

        public MissionConfig(NodeConfig nodeConfig, MqttConfig mqttConfig, DatabaseConfig? databaseConfig)
        {
            NodeConfig = nodeConfig;
            MqttConfig = mqttConfig;
            DatabaseConfig = databaseConfig;
        }
    }
}
