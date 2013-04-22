using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;

namespace PowerSave_server
{
    class relay
    {
        public enum RELAY_STATE : byte
        {
            OFFLINE = 2,
            OFF = 0,
            ON = 1,
        };

        RELAY_STATE m_state;
        short m_ID;

        public relay(RELAY_STATE state, short id)
        { 
            m_state = state;
            m_ID = id;
        }

        ~relay()
        { 
        }

        public void setState(RELAY_STATE state)
        {
            m_state = state;
        }

        public RELAY_STATE getCurrentState()
        {
            return m_state;
        }

        public short getID()
        {
            return m_ID;
        }
    }
}
