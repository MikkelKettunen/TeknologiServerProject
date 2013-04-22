using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PowerSave_server
{
    class Program
    {
        static root m_cRoot;
        static void Main(string[] args)
        {
            m_cRoot = new root();
            m_cRoot.init();
            m_cRoot.run();
            return;
        }

        public static root getRoot()
        {
            return m_cRoot;
        }
    }
}
