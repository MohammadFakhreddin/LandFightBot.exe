using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandFightBotReborn.SocketIO
{
    public class SocketIOException : Exception
    {
        public SocketIOException() { }
        public SocketIOException(string message) : base(message) { }
        public SocketIOException(string message, Exception innerException) : base(message, innerException) { }
    }
}
