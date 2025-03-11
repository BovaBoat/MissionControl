namespace MissionControl.Application
{
    public class NodeConfig
    {
        public string MissionControlName { get; }
        public string VesselName { get; }

        public NodeConfig(string missionControlName, string vesselName)
        {
            MissionControlName = missionControlName;
            VesselName = vesselName;
        }
    }
}
