
using System;

namespace Navigation
{
    internal class NavMessage
    {
        private const int COMMAND_CODE_INDEX = 0;
        public CommandsCodeEnum CommandCode { get; }
        public List<byte> Payload { get; }

        public NavMessage(List<byte>? payload)
        {
            if (payload == null)
            {
                throw new ArgumentNullException(nameof(Payload));
            }

            if (payload!.Any() != true)
            {
                throw new Exception("No data received in response payload");
            }

            CommandCode = (CommandsCodeEnum)payload[COMMAND_CODE_INDEX];

            if (!Enum.IsDefined<CommandsCodeEnum>(CommandCode))
            {
                throw new UnknownCommandException(CommandCode, "Unknown command code received");
            }

            if (payload.Count > 1)
            {
                Payload = payload.GetRange(COMMAND_CODE_INDEX + 1, payload.Count - 1);
            }
            else
            {
                Payload = new List<byte>();
            }         
        }
    }
}
