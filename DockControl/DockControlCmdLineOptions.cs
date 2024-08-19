using CommandLine;


namespace DockControl
{
    public class DockControlCmdLineOptions
    {
        [Option("destLat", Required = true)]
        public double DestinationLatitude { get; set; }

        [Option("destLong", Required = true)]
        public double DestinationLongitude { get; set; }

        [Option("broker", Required = true)]
        public string BrokerAddress { get; set; }

        [Option("nav", Required = true)]
        public string NavControlTopic { get; set; }

        [Option("boat", Required = true)]
        public string BoatResponseTopic { get; set; }

        [Option("dbServer", Required = false)]
        public string DatabaseServerName { get; set; }

        [Option("dbName", Required = false)]
        public string DatabaseName { get; set; }

        [Option("mcName", Required = false)]
        public string MissionControlNodeName { get; set; }

        [Option("vesselName", Required = false)]
        public string VesselName { get; set; }

    }
}
