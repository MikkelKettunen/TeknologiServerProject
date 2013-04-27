using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PowerSave_server
{
    static class unixtime
    {
        public static int getCurrentTime()
        {
            return (int)(DateTime.Now - new DateTime(1970, 1, 1).ToLocalTime()).TotalSeconds;
        }

        // will only work up to 60 minutes 
        public static int getDifferenceMilisecond(DateTime start, DateTime end)
        {
            TimeSpan dif = end - start;
            return (dif.Milliseconds + dif.Seconds * 1000 + dif.Minutes * 60 * 1000);
        }
    }
}
