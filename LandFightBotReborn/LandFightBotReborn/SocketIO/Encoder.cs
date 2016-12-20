using System;
using System.Collections;
using System.Text;

namespace LandFightBotReborn.SocketIO
{
    public class Encoder
    {
        public string Encode(Packet packet)
        {
            try
            {
#if SOCKET_IO_DEBUG
				Debug.Log("[SocketIO] Encoding: " + packet.json);
#endif

                StringBuilder builder = new StringBuilder();

                // first is type
                builder.Append((int)packet.enginePacketType);
                if (!packet.enginePacketType.Equals(EnginePacketType.MESSAGE))
                {
                    return builder.ToString();
                }

                builder.Append((int)packet.socketPacketType);

                // attachments if we have them
                if (packet.socketPacketType == SocketPacketType.BINARY_EVENT || packet.socketPacketType == SocketPacketType.BINARY_ACK)
                {
                    builder.Append(packet.attachments);
                    builder.Append('-');
                }

                // if we have a namespace other than '/'
                // we append it followed by a comma ','
                if (!string.IsNullOrEmpty(packet.nsp) && !packet.nsp.Equals("/"))
                {
                    builder.Append(packet.nsp);
                    builder.Append(',');
                }

                // immediately followed by the id
                if (packet.id > -1)
                {
                    builder.Append(packet.id);
                }

                if (packet.json != null && !packet.json.ToString().Equals("null"))
                {
                    builder.Append(packet.json.ToString());
                }

#if SOCKET_IO_DEBUG
				Debug.Log("[SocketIO] Encoded: " + builder);
#endif

                return builder.ToString();

            }
            catch (Exception ex)
            {
                throw new SocketIOException("Packet encoding failed: " + packet, ex);
            }
        }
    }
}
