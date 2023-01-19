using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Lactoscope_Interface
{
    public class XML_Parser
    {
        XmlDocument doc;

        public void ReadFile(string path)
        {
            doc = new XmlDocument();
            doc.Load(path);
        }

        public string GetAttributeValue(string element, string attribute)
        {
            XmlNode node = doc.DocumentElement.SelectSingleNode(element);
            string value = node.Attributes.GetNamedItem(attribute).Value;
            return value;
        }

        public string GetElementValue(string element)
        {
            XmlNode node = doc.DocumentElement.SelectSingleNode(element);
            string value = node.InnerText;
            return value;
        }
    }
}
