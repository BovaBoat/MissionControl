namespace MissionControl.WebApp.Services
{
    public class ConfigDataService
    {
        public bool IsConfigured { get; set; } = false;
        public string VesselName { get; set; } = "";
        public string MqttTopic { get; set; } = "";

        public void Configure()
        {
            if (string.IsNullOrEmpty(VesselName))
            {
                IsConfigured = false;

                MqttTopic = string.Empty;

                return;
            }

            MqttTopic = $"brod/{VesselName}";

            IsConfigured = true;
        }
    }
}
