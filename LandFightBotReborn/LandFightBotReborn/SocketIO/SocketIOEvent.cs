using LandFightBotReborn.SocketIO.JSONObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandFightBotReborn.SocketIO
{
    public class SocketIOEvent
    {
        public string name { get; set; }

        public JSONObject data { get; set; }

        public SocketIOEvent(string name) : this(name, null) { }

        public SocketIOEvent(string name, JSONObject data)
        {
            this.name = name;
            this.data = data;
        }

        public override string ToString()
        {
            return string.Format("[SocketIOEvent: name={0}, data={1}]", name, data);
        }
    }
}
