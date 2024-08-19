using CommandLine;
using MissionControl.Database.Repository;
using MissionControl.Domain;
using MissionControlLib.Waypoints;
using MissionControlLib.Infrastructure;
using Microsoft.Identity.Client;

namespace DockControl
{
    internal class Program
    {
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
            var navigationControl = new MissionControler();

            var communicationConfig = new MqttCommunicationConfig
            {
                BrokerAddress = dockControlCmdOptions.BrokerAddress,
                PublishTopic = dockControlCmdOptions.NavControlTopic,
                SubscribeTopic = dockControlCmdOptions.BoatResponseTopic           
            };

            var nodeConfig = new NodeConfig(MISSION_CONTROL_NODE_NAME, VESSEL_NODE_NAME);
            var databaseConfig = new DatabaseConfig(DB_SERVER_NAME, DATABASE_NAME);
            var dbHandler = new DatabaseRepository(databaseConfig);
            //navigationControl.MessageSent += dbHandler.StoreMessageEventHandler;
            //navigationControl.MessageReceived += dbHandler.StoreMessageEventHandler;
            
            try
            {
                navigationControl.Configure(communicationConfig, nodeConfig);
                await navigationControl.Connect();
                await navigationControl.StartMission(destinationCoordinates);

                navigationControl.LocationUpdateReceived += PrintLatestLocation;
                await navigationControl.PeriodicReportLocationCommand();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
           
            Console.ReadKey();
        }

        public static void PrintLatestLocation(Coordinates latestLocationCoordinates)
        {
            Console.WriteLine($"Latitude: {latestLocationCoordinates.GetLatitude()}, Longitude: {latestLocationCoordinates.GetLongitude()}");
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
