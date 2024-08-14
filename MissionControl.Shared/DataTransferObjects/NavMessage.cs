using MissionControl.Shared.Enums;

namespace MissionControl.Shared.DataTransferObjects
{
    public class NavMessage
    {
        public CommandCodeEnum CommandCode { get; }
        public List<byte>? Payload { get; }

        public NavMessage(CommandCodeEnum commandCode, List<byte>? payload = null)
        {
            if (!IsComandCodeValid(commandCode))
            {
                throw new Exception($"The command code is not valid.");
            }

            CommandCode = commandCode;
            Payload = payload;
        }

        private bool IsComandCodeValid(CommandCodeEnum commandCode)
        {
            if (!Enum.IsDefined(commandCode))
            {
                return false;
            }

            return true;
        }

        public List<byte> GetMessageContentByteArray()
        {
            var messageContent = new List<byte>();
            messageContent.Add((byte)CommandCode);

            if (Payload != null)
            {
                messageContent.AddRange(Payload);
            }
            
            return messageContent;
        }
    }
}
