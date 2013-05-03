using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;

namespace PowerSave_server
{
    class arduinoListener
    {
        private class internalDataStructure
        {
            public List<relay> relays;
            public SerialPort sp;
            // when we send a packet, we add it to here, so we can check the reply
            public List<byte> packetData;

            public internalDataStructure()
            {
                relays = new List<relay>();
                packetData = new List<byte>();
                sp = null;
            }
        };

        List<internalDataStructure> m_connected;

        public arduinoListener()
        {
            m_connected = new List<internalDataStructure>();
        }

        public bool addSerialPort(string port)
        {
            internalDataStructure data;
            try
            {
                data = new internalDataStructure();
                data.sp = new SerialPort(port, 9600, Parity.None, 8, 
                                        StopBits.One);
                data.sp.Open();
            }
            catch (Exception e)
            {
                e.ToString();
                return false;
            }
            if (data.sp == null)
            {
                return false;
            }
            data.sp.ReadTimeout = 0;
            // there is a 1 relay
            relay nRelay = new relay(relay.RELAY_STATE.ON, 1);
            data.relays.Add(nRelay);
            m_connected.Add(data);
            sendSetState(nRelay, relay.RELAY_STATE.ON, data);

            nRelay = new relay(relay.RELAY_STATE.ON, 2);
            data.relays.Add(nRelay);
            m_connected.Add(data);
            sendSetState(nRelay, relay.RELAY_STATE.ON, data);

            return true;
        }

        public void readData()
        {
            foreach (internalDataStructure DS in m_connected)
            {
                try
                {
                    // read all the data until we get an exception from no data
                    for (; ; )
                    {
                        if (DS.sp.BytesToRead == 0)
                            break;
                        byte data = (byte)DS.sp.ReadByte();
                        parseByte(data, DS);
                    }
                }
                catch (TimeoutException e)
                {
                    e.GetType(); // fix warning
                    // nothing this is what should happen
                }
                catch (Exception e)
                {
                    Console.WriteLine("arduinoListener::readData() exception: {0}", e.Message);
                }
            }
        }

        void parseByte(byte data, internalDataStructure DS)
        {
            if (!isFromArduino(data))
            {
                Console.WriteLine("got wrong packet, it doesn't appear to be from arduino but it is!!");
                return;
            }
            Console.WriteLine("got packet {0}", data);
            parseArduinoData(data, DS);
        }

        void parseArduinoData(byte data, internalDataStructure DS)
        {
            // extract the id
            byte id = (byte)(data & 31);
            Console.WriteLine("server got id {0} from arduino", id);
            // this should be the first
            if (DS.packetData.Count == 0)
            {
                Console.WriteLine("Got data from Arduino but didn't expect any");
                return;
            }
            for(int i = 0; i < DS.packetData.Count; i++)
            {
                if (DS.packetData[i] == id)
                {
                    DS.packetData.RemoveAt(i);
                    Console.WriteLine("Got arduino data as expected with id {0}", id);
                    break;
                }
            }
        }

        bool isFromArduino(byte data)
        {
            return (data & (1 << 7)) > 0;
        }

        public bool setState(byte relayID, relay.RELAY_STATE state)
        {
            // we only got 1 in m_connected right now, so we're allowede to do this
            internalDataStructure ds = m_connected[0];
            for (int i = 0; i < ds.relays.Count; i++)
            {
                if (ds.relays[i].getID() == relayID)
                {
                    sendSetState(ds.relays[i], state, ds);
                    ds.relays[i].setState(state);
                    return true;
                }
            }
            return false;
        }

        void sendSetState(relay to, relay.RELAY_STATE state, internalDataStructure DS)
        {
            byte data = 0;
            data |= (byte)to.getID(); // write the id
            data |= (byte)(state == relay.RELAY_STATE.ON ? 0x20 : 0);
            sendData(data, DS.sp);
            // adding the data to the queue, so we know what we should recieve later
            DS.packetData.Add((byte)to.getID());
        }

        void sendData(byte data, SerialPort sp)
        {
            Console.WriteLine("sent {0}", data);
            sp.Write(new byte[1] { data }, 0, 1); 
        }

        public List<relay> getAllRelays()
        {
            List<relay> all = new List<relay>();
            List<int> added = new List<int>();
            foreach (internalDataStructure DS in m_connected)
            {
                foreach (relay r in DS.relays)
                {
                    if (!added.Contains(r.getID()))
                    {
                        all.Add(r);
                        added.Add(r.getID());
                    }
                }
            }
            
            return all;
        }
    }
}
