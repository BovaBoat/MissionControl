using MissionControlDatabase.Models;
using MissionControl.Domain.Entities;

namespace MissionControl.Database.Repository
{
    public class DatabaseRepository
    {
        private MissionControlDbContext _dbContext;

        public DatabaseRepository(DatabaseConfig dbConfig)
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

        public void StoreMessageEventHandler(NavMessage message, string senderName)
        {
            InsertMessage((int)message.CommandCode, message.Payload.ToArray(), senderName);
        }
    }
}
