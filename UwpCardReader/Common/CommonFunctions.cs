using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace UwpCardReader.Common
{
    public static class CommonFunctions
    {
        public static string GetConnectionStringFromConfigFile()
        {
            XmlDocument configurationsFile = new XmlDocument();
            configurationsFile.Load("Configurations.xml");
            XmlNode node = configurationsFile.SelectSingleNode("/AppSettings/ConnectionString");
            return node.InnerText.Trim();
        }
    }
}
