using System.ComponentModel;

namespace MissionControl.Infrastructure.Exceptions
{
    public class FailedToPublishMessageException : Exception
    {
        public List<byte> MessagePayload;

        public FailedToPublishMessageException(string message, List<byte> messagePayload)
        : base(message)
        {
            MessagePayload = messagePayload;
        }
    }
}
