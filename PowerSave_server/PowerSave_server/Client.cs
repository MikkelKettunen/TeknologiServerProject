using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace PowerSave_server
{
    // the states a client can be in
    enum CLIENT_STATE
    {
        // we're waiting for handshake
        HANDHAKE,
        // we're online
        ONLINE,
    };

    class Client
    {
        private Socket m_socket;
        // so we know if a client is online or 
        // if we are waiting to confirm it's a client
        CLIENT_STATE m_state;

        // a place we will save the data from the client
        List<byte> m_buffer;
        // so we know if a client connected right
        bool m_isOnline;

        // called when we make a new client
        public Client(Socket sock)
        {
            m_socket = sock;
            m_socket.Blocking = false;
            m_state = CLIENT_STATE.HANDHAKE;
            m_buffer = new List<byte>();
            m_isOnline = true;
        }

        // called when the client is deleted from the system
        ~Client()
        {
            // close the socket, so we can accept more clients
            m_socket.Close();
        }

        public void clientAcceptData()
        {
            Byte []buffer = new Byte[1024];
            try
            {
                int bytesReceived = 1;
                while (bytesReceived > 0)
                {
                    // receive packets
                    // if we get 0 bytes, kick this client
                    bytesReceived = m_socket.Receive(buffer);
                    if (bytesReceived == 0)
                    {
                        kick("revieved 0 bytes, client disconnected");
                        continue;
                    }
                    Console.WriteLine("recieved {0} bytes", bytesReceived);
                    for (int i = 0; i < bytesReceived; i++)
                    {
                        // save the data
                        byte data = buffer[i];
                        m_buffer.Add(data);
                    }
                }
            }
            catch (SocketException se)
            {
                if (se.ErrorCode != (int)SocketError.WouldBlock)
                {
                    Console.WriteLine(se.Message);
                    m_isOnline = false;
                    return;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                m_isOnline = false;
                return;
            }
            parseBuffer();
        }

        void parseBuffer()
        {
            scPacket pck = new scPacket();
            if (pck.parseRawData(m_buffer))
            {
                m_buffer.RemoveRange(0, pck.getSize());
                handlePacket(pck);
            }
            else
            {
                // check pck error
                if ((PACKET_TYPE)pck.getPacketType() == PACKET_TYPE.ERR_INVALID_PACKET)
                {
                    kick("got an invalid packet!");
                }
            }
        }

        void handlePacket(scPacket pck)
        {
            switch ((PACKET_TYPE)pck.getPacketType())
            {
            case PACKET_TYPE.C_HANDSHAKE:
                if (m_state == CLIENT_STATE.HANDHAKE)
                {
                    Console.WriteLine("got handshake");
                    sendAcceptPacket();
                    m_state = CLIENT_STATE.ONLINE;
                }
                else
                {
                    // kick the client for sending wrong packets!
                    kick("Handshake recieved from client, but it is already online!");
                }
                break;
            case PACKET_TYPE.C_GET_ONLINE_CLIENTS:
                if (m_state == CLIENT_STATE.ONLINE)
                {
                    Console.WriteLine("got GET_ONLINE_CLIENTS packet!");
                    sendOnlineSockets();
                }
                else
                {
                    kick("got packet GET_ONLINE_CLIENTS, but client is not online!!");
                }
                break;
            case PACKET_TYPE.C_UPDATE_SOCKET_STATE:
                if (m_state == CLIENT_STATE.ONLINE)
                {
                    Console.WriteLine("got UPDATE_SOCKET_STATE packet!");
                    short sockId = pck.readShort();
                    byte state = pck.readByte();
                    if (Program.getRoot().setRelayState(sockId, state))
                    {
                        Console.WriteLine("updated socket {0} to state {1}", sockId, state);
                        updateSocket(sockId, state);
                        sendSockUpdateToAll(sockId, state);
                    }
                    else
                    {
                        Console.WriteLine("failed updated socket {0} to state {1}", sockId, state);
                    }
                }
                else
                {
                    kick("got packet UPDATE_SOCKET_STATE, but client is not online!!");
                }
                break;
            }
        }

        public bool isOnline()
        {
            return m_isOnline;
        }

        public void kick(string reason)
        {
            Console.WriteLine("client got kicked: {0}", reason);
            m_isOnline = false;
        }

        void sendAcceptPacket()
        {
            scPacket pck = new scPacket(PACKET_TYPE.S_ACCEPT);
            m_state = CLIENT_STATE.ONLINE;
            sendPacket(pck);
        }

        void sendOnlineSockets()
        {
            scPacket pck = new scPacket(PACKET_TYPE.S_SEND_ONLINE_CLIENTS);
            // now write all the data needed to the packet
            List<relay> relays = Program.getRoot().getRelays();
            pck.writeShort((short)relays.Count);
            for (int i = 0; i < relays.Count; i++)
            {
                pck.writeShort(relays[i].getID());                  //2
                pck.writeByte((byte)relays[i].getCurrentState());   //3
                pck.writeLong(relays[i].getTotalUptime());          //7
                pck.writeLong(relays[i].getDailyUptime());          //11
                pck.writeLong(relays[i].getDailyDowntime());        //15
                pck.writeLong(relays[i].getWatt());                 //19
            }
            sendPacket(pck);
        }

        public void sendPacket(scPacket pck)
        {
            try
            {
                m_socket.Send(pck.getRawData().ToArray());
            }
            catch (Exception e)
            {
            }
        }

        void updateSocket(short sockid, byte state)
        {
            List<relay> relays = Program.getRoot().getRelays();
            for (int i = 0; i < relays.Count; i++)
            {
                if (relays[i].getID() == sockid)
                {
                    relays[i].setState((relay.RELAY_STATE)state);
                    break;
                }
            }
        }

        void sendSockUpdateToAll(short sockid, byte state)
        {
            scPacket pck = new scPacket(PACKET_TYPE.S_SOCKET_UPDATE);
            pck.writeShort(sockid);
            pck.writeByte(state);
            Program.getRoot().sendToAll(pck);
        }
    } 
}
