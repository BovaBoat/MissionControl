using MissionControlDatabase.Models;

namespace MissionControlDatabase
{
    public class DatabaseHandler
    {
        private MissionControlDbContext _dbContext;

        public DatabaseHandler(DatabaseConfig dbConfig)
        {
            _dbContext = new MissionControlDbContext(dbConfig);
        }

        public void InsertMessage(int commandCode, byte[]? payload, string nodeName)
        {
            var node = _dbContext.Nodes.FirstOrDefault(n => n.Name == nodeName);

            if (node == null)
            {
                throw new Exception("Node not found with the provided name.");
            }

            var message = new Message
            {
                CommandCode = commandCode,
                Payload = payload,
                NodeId = node.NodeId,
                Timestamp = DateTime.Now
            };

            _dbContext.Messages.Add(message);
            _dbContext.SaveChanges();
        }
    }

    #region Structures

    public struct DatabaseConfig
    {
        public string ServerName;
        public string DatabaseName;

        public DatabaseConfig(string serverName, string databaseName)
        {
            ServerName = serverName;
            DatabaseName = databaseName;
        }
    }

    #endregion
}
