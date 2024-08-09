using CommandLine;
using MissionControlLib.Waypoints;
using MissionControlLib;
using MissionControlDatabase;

namespace DockControl
{
    internal class Program
    {
        const string BROKER_ADDRESS = "test.mosquitto.org";
        const string TOPIC = "brod/test";
        const string DB_SERVER_NAME = "LAPTOP-HA1AJVLV";
        const string DATABASE_NAME = "MissionControl";

        const string MISSION_CONTROL_NODE_NAME = "Andrija MC";
        const string VESSEL_NODE_NAME = "Andrija Mock Boat";

        static async Task Main(string[] args)
        {
            if (args.Length <= 0)
            {
                throw new Exception("No arguments specified");
            }

            var dockControlCmdOptions = new DockControlCmdLineOptions();

            var result = Parser.Default.ParseArguments<DockControlCmdLineOptions>(args)
                .WithNotParsed(HandleParseError)
                .WithParsed(parsedOptions => dockControlCmdOptions = parsedOptions);

            var destinationCoordinates = new Coordinates(dockControlCmdOptions.DestinationLatitude, dockControlCmdOptions.DestinationLongitude);
            var navigationControl = new MissionControl();

            var communicationConfig = new MqttCommunicationConfig
            {
                BrokerAddress = dockControlCmdOptions.BrokerAddress,
                NavControlTopic = dockControlCmdOptions.NavControlTopic,
                BoatResponseTopic = dockControlCmdOptions.BoatResponseTopic           
            };

            var nodeConfig = new NodeConfig(MISSION_CONTROL_NODE_NAME, VESSEL_NODE_NAME);

            var databaseConfig = new DatabaseConfig(DB_SERVER_NAME, DATABASE_NAME);

            navigationControl.Configure(communicationConfig, databaseConfig, nodeConfig);

            await navigationControl.StartCommunication();

            await navigationControl.StartMission(destinationCoordinates);

            Console.ReadKey();
        }

        static void HandleParseError(IEnumerable<Error> errs)
        {
            ExitCode result;
            if (errs.Any(x => x is HelpRequestedError || x is VersionRequestedError))
            {
                result = ExitCode.Success;
            }
            else
            {
                result = ExitCode.Success;
                Console.WriteLine("Errors: {0}", errs.Count());
            }

            Console.WriteLine("\nExit code {0}", result);
        }

        private enum ExitCode : int
        {
            Success = 0,        
        }
    }

}
