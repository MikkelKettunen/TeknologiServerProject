using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace PowerSave_server
{
    // handles saving and writing XML files
    class FileHandler
    {
        /* 
         * save all pictures in memory
         * so we don't have to fetch them all the time
         */
        Dictionary<string, byte[]> m_pictures;
        string m_xml;
        string m_dir;
        public FileHandler()
        {
            m_pictures = new Dictionary<string, byte[]>();
            m_dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            try
            {
                readXML();
            }
            catch (Exception)
            {
                m_xml = "";
            }
        }

        public string getXML()
        {
            return m_xml;
        }

        public void setXML(string xml)
        {
            m_xml = xml;
            saveXmlToHDD(xml);
        }

        void saveXmlToHDD(string xml)
        {
            File.WriteAllText(m_dir + "\\xmldata.xml", xml);
        }

        void readXML()
        {
            m_xml = File.ReadAllText(m_dir + "\\xmldata.xml");
        }

        void savePictureToHDD(byte[] picData, string filename)
        {
            FileStream fs = new FileStream(m_dir + "\\" + filename, FileMode.Create);
            fs.Write(picData, 0, picData.Length);
            fs.Close();
        }

        public void addPicture(string picname, byte[] data)
        {  
            m_pictures[picname] = data;
            FileStream fs = new FileStream(m_dir + "\\" + picname, FileMode.Create);
            fs.Write(data, 0, data.Length);
            fs.Close();
        }


        /* return null on error*/
        public byte[] getPicture(string picname)
        {
            if (m_pictures.ContainsKey(picname))
                return m_pictures[picname];
            // try to load from dics
            FileStream fs;
            try
            {
                fs = new FileStream(m_dir + "\\" + picname, FileMode.Open);
            }
            catch (Exception)
            {
                return null;
            }

            try
            {
                byte[] data = new byte[fs.Length];
                fs.Read(data, 0, (int)fs.Length);
                m_pictures[picname] = data;
                fs.Close();
                return data;
            }
            catch (Exception)
            {
                fs.Close();
            }
            return null;
        }
    }
}
