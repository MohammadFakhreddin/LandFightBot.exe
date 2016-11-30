using LandFightBotReborn.SocketIO.JSONObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandFightBotReborn.SocketIO
{
    public class Parser
    {
        public SocketIOEvent Parse(JSONObject json)
        {
            if (json.Count < 1 || json.Count > 2)
            {
                throw new SocketIOException("Invalid number of parameters received: " + json.Count);
            }

            if (json[0].type != JSONObject.Type.STRING)
            {
                throw new SocketIOException("Invalid parameter type. " + json[0].type + " received while expecting " + JSONObject.Type.STRING);
            }

            if (json.Count == 1)
            {
                return new SocketIOEvent(json[0].str);
            }

            if (json[1].type != JSONObject.Type.OBJECT)
            {
                throw new SocketIOException("Invalid argument type. " + json[1].type + " received while expecting " + JSONObject.Type.OBJECT);
            }

            return new SocketIOEvent(json[0].str, json[1]);
        }
    }
}
