namespace MissionControl.Database
{
    public class DatabaseConfig
    {
        public string ServerName { get; }
        public string DatabaseName { get; }

        public DatabaseConfig(string serverName, string databaseName)
        {
            ServerName = serverName;
            DatabaseName = databaseName;
        }
    }
}
