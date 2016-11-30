using LandFightBotReborn.SocketIO.JSONObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandFightBotReborn.SocketIO
{
    public class Packet
    {
        public EnginePacketType enginePacketType;
        public SocketPacketType socketPacketType;

        public int attachments;
        public string nsp;
        public int id;
        public JSONObject json;

        public Packet() : this(EnginePacketType.UNKNOWN, SocketPacketType.UNKNOWN, -1, "/", -1, null) { }
        public Packet(EnginePacketType enginePacketType) : this(enginePacketType, SocketPacketType.UNKNOWN, -1, "/", -1, null) { }

        public Packet(EnginePacketType enginePacketType, SocketPacketType socketPacketType, int attachments, string nsp, int id, JSONObject json)
        {
            this.enginePacketType = enginePacketType;
            this.socketPacketType = socketPacketType;
            this.attachments = attachments;
            this.nsp = nsp;
            this.id = id;
            this.json = json;
        }

        public override string ToString()
        {
            return string.Format("[Packet: enginePacketType={0}, socketPacketType={1}, attachments={2}, nsp={3}, id={4}, json={5}]", enginePacketType, socketPacketType, attachments, nsp, id, json);
        }
    }
}
