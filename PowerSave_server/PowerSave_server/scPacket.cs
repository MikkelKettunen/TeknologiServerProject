using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PowerSave_server
{
    /* 
     *  "SC_" is a server / client packet, so it can be sent by both
     *  "C_" is a client packet
     *  "S_" is a server packet
     */
    enum PACKET_TYPE : byte
    {
        SC_PING = 0x00,
        C_HANDSHAKE = 0x01,
        S_ACCEPT = 0x02,
        C_GET_ONLINE_CLIENTS = 0x03,
        S_SEND_ONLINE_CLIENTS = 0x04,
        C_UPDATE_SOCKET_STATE = 0x05,
        S_SOCKET_UPDATE = 0x06,
        C_SOCKET_POWER_UPDATE = 0x07,
        S_SOCKET_POWER_UPDATE = 0x08,
        C_REQUEST_SOCKET_INFO = 0x09,
        S_SOCKET_POWER_INFO = 0x0A,
        // invalid states
        ERR_NOT_DONE = 0xFC,
        ERR_INVALID_PACKET = 0xFE,
        ERR_NONE = 0xFF,
    };
    // server client packet
    class scPacket
    {
        private byte m_packetID;
        List<byte> m_packetData;
        UInt16 m_pos; // used so we can read and write data
        public scPacket(PACKET_TYPE type)
        {
            m_packetID = (byte)type;
            m_pos = 1;
            m_packetData = new List<byte>();
        }

        public scPacket()
        {
            m_packetID = (byte)PACKET_TYPE.ERR_NONE;
            m_pos = 0;
            m_packetData = new List<byte>();
        }

        public byte getPacketType()
        {
            return m_packetID;
        }
        /*
         * returns true if we got a valid packet
         * returns false if we couldn't parse the packet
         * m_packetID is always set to a value
         */
        public bool parseRawData(List<byte> raw)
        {
            if (raw.Count == 0)
            {
                m_packetID = (byte)PACKET_TYPE.ERR_NOT_DONE;
                return false;
            }
            switch((PACKET_TYPE)raw[0])
            {
                case PACKET_TYPE.C_GET_ONLINE_CLIENTS:
                    // it's only 1 byte long
                    m_packetID = raw[0];
                    m_pos = 0;
                    return true;
                case PACKET_TYPE.C_HANDSHAKE:
                    // it's only 1 byte long anyways
                    m_packetID = raw[0];
                    m_pos = 0;
                    return true;
                case PACKET_TYPE.C_UPDATE_SOCKET_STATE:
                    if (raw.Count >= 4)
                    {
                        m_packetID = raw[0];
                        for (int i = 1; i < 4; i++)
                            m_packetData.Add(raw[i]);
                        m_pos = 0;
                        return true;
                    }
                    m_packetID = (byte)PACKET_TYPE.ERR_NOT_DONE;
                    return false;
                case PACKET_TYPE.C_SOCKET_POWER_UPDATE:
                    if (raw.Count >= 7)
                    {
                        m_packetID = raw[0];
                        for (int i = 1; i < 7; i++)
                            m_packetData.Add(raw[i]);
                        m_pos = 0;
                        return true;
                    }
                    break;
            }
            m_packetID = (byte)PACKET_TYPE.ERR_INVALID_PACKET;
            return false;
        }

        public int getSize()
        {
            return m_packetData.Count + 1;
        }

        public List<byte> getRawData()
        {
            List<byte> tmp = new List<byte>();
            tmp.Add(m_packetID);
            for (int i = 0; i < m_packetData.Count; i++)
                tmp.Add(m_packetData[i]);
            return tmp;
        }

        public void writeShort(short data)
        {
            byte first = (byte)(data >> 8),
                 second = (byte)(data);
            m_packetData.Add(first);
            m_packetData.Add(second);
        }

        public void writeByte(byte data)
        {
            m_packetData.Add(data);
        }

        public void writeLong(int data)
        {
            writeShort((short)(data >> 16));
            writeShort((short)data);
        }

        public byte readByte()
        {
            return m_packetData[m_pos++];
        }

        public short readShort()
        {
            short tmp = (short)(readByte());
            tmp <<= 8;
            tmp |= (short)readByte();
            return tmp;
        }

        public int readLong()
        {
            int tmp = (int)readShort();
            tmp <<= 16;
            //tmp = 
            return tmp;
        }
    }
}
