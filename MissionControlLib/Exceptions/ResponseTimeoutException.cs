using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionControlLib.Exceptions
{
    class ResponseTimeoutException : Exception
    {
        public ResponseTimeoutException(string message)
        : base(message)
        {

        }
    }
}
