using CommandLine;
using Navigation;
using Waypoint;

namespace DockControl
{
    internal class Program
    {
        const string BROKER_ADDRESS = "test.mosquitto.org";
        const string TOPIC = "brod/test";

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

            navigationControl.Configure(communicationConfig);

            await navigationControl.StartCommunication();

            await navigationControl.StartMission(destinationCoordinates);

            Console.ReadKey();
        }

        private static void PrintDistanceBetweenWaypoints(Waypoint.Waypoint point1, Waypoint.Waypoint point2)
        {
            Console.WriteLine($"Distance between start and destination: {point1.GetDistanceTo(point2)}");
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
