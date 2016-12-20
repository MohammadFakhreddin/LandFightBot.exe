using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LandFightBotReborn.SocketIO.JSONObjects;

namespace LandFightBotReborn.SocketIO
{
    public class Ack
    {
        public int packetId;
        public DateTime time;

        private System.Action<JSONObject> action;

        public Ack(int packetId, System.Action<JSONObject> action)
        {
            this.packetId = packetId;
            this.time = DateTime.Now;
            this.action = action;
        }

        public void Invoke(JSONObject ev)
        {
            action.Invoke(ev);
        }

        public override string ToString()
        {
            return string.Format("[Ack: packetId={0}, time={1}, action={2}]", packetId, time, action);
        }
    }
}
