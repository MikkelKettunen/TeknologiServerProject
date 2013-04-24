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
        int m_totalUpdate;
        int m_dailyUptime;
        int m_dailyDowntime;
        int m_watt;
        int m_startTime;
        DateTime m_startDateTime;
        DateTime m_today;
        DateTime m_lastUpdate;


        public relay(RELAY_STATE state, short id, int watt = 0)
        { 
            m_state = state;
            m_ID = id;
            m_totalUpdate = 0;
            m_dailyDowntime = 0;
            m_dailyUptime = 0;
            m_startTime = getUnixTimestamp(DateTime.Now);
            m_today = DateTime.Now;
            m_startDateTime = DateTime.Now;
            m_lastUpdate = DateTime.Now;
        }

        ~relay()
        {
        }

        private void updateTime()
        {
            DateTime now = DateTime.Now;
            int dif = getUnixTimestampDifference(now, m_lastUpdate);
            if (dif > 0)
            {
                if (m_state == RELAY_STATE.ON)
                {
                    m_dailyUptime += dif;
                }
                else if (m_state == RELAY_STATE.OFF)
                {
                    m_dailyDowntime += dif;
                }
                m_lastUpdate = now;
            }

            if (now.Day != m_today.Day)
            {
                m_dailyDowntime = 0;
                m_dailyUptime = 0;
                m_today = DateTime.Now;
            }
        }

        public void setState(RELAY_STATE state)
        {
            updateTime();
            m_state = state;
        }

        public void setWatt(int watt)
        {
            m_watt = watt;
        }

        public RELAY_STATE getCurrentState()
        {
            return m_state;
        }

        public short getID()
        {
            return m_ID;
        }

        public int getTotalUptime()
        {
            updateTime();
            return getUnixTimestamp(DateTime.Now) - m_startTime;
        }

        public int getDailyUptime()
        {
            updateTime();
            return m_dailyUptime;
        }

        public int getDailyDowntime()
        {
            updateTime();
            return m_dailyDowntime;
        }

        public int getWatt()
        {
            return m_watt;
        }

        private int getUnixTimestamp(DateTime dateTime)
        {
            return (int)(dateTime - new DateTime(1970, 1, 1).ToLocalTime()).TotalSeconds;
        }

        private int getUnixTimestampDifference(DateTime start, DateTime now)
        {
            return (int)(start - now).TotalSeconds;
        }
    }
}
