using MissionControl.Application;
using MissionControl.Infrastructure;
using MissionControl.Infrastructure.Mqtt;

namespace MissionControl.WebAPI.DTO
{
    public class MissionConfig
    {
        public string? InstanceId { get; set; }
        public NodeConfig NodeConfig { get; set; }
        public MqttConfig MqttConfig { get; set; }
    }
}
