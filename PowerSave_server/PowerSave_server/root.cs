using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PowerSave_server
{
    class root
    {
        // this will allow us to accept new clients to join
        private ClientProtocolListener m_cProtocolListener;
        // contains all the clients connected
        private List<Client> m_clients;
        // so we can save the clients that should be removed, they cannot be removed in a foreach loop, that will cause problems
        private List<Client> m_toBeRemoved;

        // we only have 1 arduino right now
        arduinoListener m_arduino;

        public root()
        {
            m_clients = new List<Client>();
            m_toBeRemoved = new List<Client>();
            m_arduino = new arduinoListener();
            Console.WriteLine("enter an arduino comport");
            string port = Console.ReadLine();
            while (!m_arduino.addSerialPort(port)) 
            {
                Console.WriteLine("please try again, that comport does not exist");
                port = Console.ReadLine();
            }
        }

        public void init()
        {
            m_cProtocolListener = new ClientProtocolListener();
        }

        public void run()
        {
            // check if we can start the clientProtocolListener
            if (!m_cProtocolListener.init())
            {
                Console.WriteLine("failed to init clientProtocolListener!");
                return;
            }
            Console.WriteLine("running server!");
            while (true)
            {
                Client newClient = m_cProtocolListener.acceptNewClients();
                if (newClient != null)
                {
                    //  a new client connected
                    m_clients.Add(newClient);
                    Console.WriteLine("there is now {0} online clients", m_clients.Count);
                }
                foreach (Client c in m_clients)
                {
                    c.clientAcceptData();
                    if (!c.isOnline())
                    {
                        m_toBeRemoved.Add(c);
                    }
                }
                foreach (Client c in m_toBeRemoved)
                {
                    m_clients.Remove(c);
                }
                m_toBeRemoved.Clear();
                m_arduino.readData();
                //System.Threading.Thread.Sleep(10);
            }
        }

        public List<relay> getRelays()
        {
            // todo fix!
            return m_arduino.getAllRelays();
        }

        public void sendToAll(scPacket pck)
        {
            foreach (Client c in m_clients)
            {
                c.sendPacket(pck);
            }
        }

        public bool setRelayState(short id, byte state)
        {
            return m_arduino.setState((byte)id, (relay.RELAY_STATE)state);
        }
    }
}
