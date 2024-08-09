namespace MissionControlLib
{
    public class NavMessage
    {
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
