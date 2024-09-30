namespace MissionControl.BlazorWebApp.Services
{
    public class ConfigDataService
    {
        public bool IsConfigured { get; set; } = false;
        public string VesselName { get; set; } = "";
        public string BrokerAddress { get; set; } = "";
        public string PublishTopic { get; set; } = "";
        public string SubscribeTopic { get; set; } = "";
        public string MissionControlName { get; set; } = "";

        public void Configure()
        {
            if (string.IsNullOrEmpty(VesselName))
            {
                IsConfigured = false;

                return;
            }

            IsConfigured = true;
        }
    }
}
