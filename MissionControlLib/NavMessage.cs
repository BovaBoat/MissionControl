using System;

namespace MissionControlLib
{
    public class NavMessage
    {
        private const int COMMAND_CODE_INDEX = 0;
        public CommandsCodeEnum CommandCode { get; }
        public List<byte>? Payload { get; }

        public NavMessage(CommandsCodeEnum commandCode, List<byte>? payload = null)
        {
            if (!IsComandCodeValid(commandCode))
            {
                throw new Exception($"The command code is not valid.");
            }

            CommandCode = commandCode;
            Payload = payload;
        }

        private bool IsComandCodeValid(CommandsCodeEnum commandCode)
        {
            if (!Enum.IsDefined<CommandsCodeEnum>(commandCode))
            {
                return false;
            }

            return true;
        }
    }
}
