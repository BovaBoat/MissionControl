﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Navigation
{
    class ResponseTimeoutException : Exception
    {
        public ResponseTimeoutException(string message)
        : base(message)
        { 
            
        }
    }
}
