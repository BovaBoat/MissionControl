using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Navigation
{
    internal class UnknownCommandException : Exception
    {
        CommandsCodeEnum CommandCode { get; }
        public UnknownCommandException(CommandsCodeEnum commandCode, string message)
            : base(message)
        {
            CommandCode = commandCode;
        }

        public UnknownCommandException(string message)
        : base(message)
        { }
    }
}
