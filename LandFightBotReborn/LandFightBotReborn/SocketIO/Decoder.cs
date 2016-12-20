using LandFightBotReborn.SocketIO.JSONObjects;
using System;
using System.Collections;
using System.Text;
using WebSocketSharp;
using myLogger = LandFightBotReborn.Utils.Logger;

namespace LandFightBotReborn.SocketIO
{
    public class Decoder
    {
        public Packet Decode(MessageEventArgs e)
        {
            try
            {
#if SOCKET_IO_DEBUG
				Debug.Log("[SocketIO] Decoding: " + e.Data);
#endif

                string data = e.Data;
                Packet packet = new Packet();
                int offset = 0;

                // look up packet type
                int enginePacketType = int.Parse(data.Substring(offset, 1));
                packet.enginePacketType = (EnginePacketType)enginePacketType;

                if (enginePacketType == (int)EnginePacketType.MESSAGE)
                {
                    int socketPacketType = int.Parse(data.Substring(++offset, 1));
                    packet.socketPacketType = (SocketPacketType)socketPacketType;
                }

                // connect message properly parsed
                if (data.Length <= 2)
                {
#if SOCKET_IO_DEBUG
					Debug.Log("[SocketIO] Decoded: " + packet);
#endif
                    return packet;
                }

                // look up namespace (if any)
                if ('/' == data[offset + 1])
                {
                    StringBuilder builder = new StringBuilder();
                    while (offset < data.Length - 1 && data[++offset] != ',')
                    {
                        builder.Append(data[offset]);
                    }
                    packet.nsp = builder.ToString();
                }
                else
                {
                    packet.nsp = "/";
                }

                // look up id
                char next = data[offset + 1];
                if (next != ' ' && char.IsNumber(next))
                {
                    StringBuilder builder = new StringBuilder();
                    while (offset < data.Length - 1)
                    {
                        char c = data[++offset];
                        if (char.IsNumber(c))
                        {
                            builder.Append(c);
                        }
                        else
                        {
                            --offset;
                            break;
                        }
                    }
                    packet.id = int.Parse(builder.ToString());
                }

                // look up json data
                if (++offset < data.Length - 1)
                {
                    try
                    {
#if SOCKET_IO_DEBUG
						Debug.Log("[SocketIO] Parsing JSON: " + data.Substring(offset));
#endif
                        packet.json = new JSONObject(data.Substring(offset));
                    }
                    catch (Exception ex)
                    {
                        myLogger.info(ex.ToString());
                    }
                }

#if SOCKET_IO_DEBUG
				Debug.Log("[SocketIO] Decoded: " + packet);
#endif

                return packet;

            }
            catch (Exception ex)
            {
                throw new SocketIOException("Packet decoding failed: " + e.Data, ex);
            }
        }
    }
}
