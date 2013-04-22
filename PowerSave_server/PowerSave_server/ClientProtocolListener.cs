using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
namespace PowerSave_server
{
    class ClientProtocolListener
    {
        // this listen for new clients
        Socket m_listener;
        // ip and port of the server
        IPEndPoint m_portAndIP;

        public ClientProtocolListener()
        {
            m_listener = new Socket(AddressFamily.InterNetwork, 
                                    SocketType.Stream, 
                                    ProtocolType.Tcp);
            m_portAndIP = new IPEndPoint(IPAddress.Any, 7648);
        }

        public bool init()
        {
            try
            {
                m_listener.Bind(m_portAndIP);
                m_listener.Listen(5);
                m_listener.Blocking = false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            return true;
        }

        public Client acceptNewClients()
        {
            Socket clientSocket;
            Client client = null;
            try
            {
                clientSocket = m_listener.Accept();
                client = new Client(clientSocket);
            }
            catch (SocketException sockexception)
            {
                switch (sockexception.ErrorCode)
                {
                    case (int)SocketError.WouldBlock:
                        break;
                    default:
                        Console.WriteLine(sockexception.Message);
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.GetType());
                return null;
            }
            return client;
        }
    }
}
