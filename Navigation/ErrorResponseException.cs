using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waypoint
{
    public class ErrorResponseException : Exception
    {
        public ErrorResponseException(string message)
        : base(message)
        {

        }
    }
}
