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
            string value;
            XmlNode node = doc.DocumentElement.SelectSingleNode(element);
            if(node != null)
            {
                value = node.Attributes.GetNamedItem(attribute).Value;
            } else
            {
                value = "";
            }
            return value;
        }

        public string GetElementValue(string element)
        {
            string value;
            XmlNode node = doc.DocumentElement.SelectSingleNode(element);
            if (node != null)
            {
                value = node.InnerText;
            } else
            {
                value = "";
            }

            return value;
        }
    }
}
