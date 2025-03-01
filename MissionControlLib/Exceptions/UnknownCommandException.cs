using MissionControl.Shared.Enums;

namespace MissionControl.Domain.Exceptions
{
    internal class UnknownCommandException : Exception
    {
        CommandCodeEnum CommandCode { get; }
        public UnknownCommandException(CommandCodeEnum commandCode, string message)
            : base(message)
        {
            CommandCode = commandCode;
        }

        public UnknownCommandException(string message)
        : base(message)
        { }
    }
}
