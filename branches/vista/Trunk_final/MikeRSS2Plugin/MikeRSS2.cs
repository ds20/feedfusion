using System;
using System.Reflection; 
using System.Collections.Generic;
using System.Text;
using PluginInterface;
using System.Xml;
using System.IO;
using System.Xml.Xsl;

namespace MikeRSS2Plugin
{
    [Serializable]
    public class MikeRSS2 : rssInterface
    {
        XmlDocument xmlDoc;
        string fileName;
        string htmlName;
        string xslName;

        public MikeRSS2()
        {
            fileName = Path.GetTempPath() + @"\MikeRSS2.rss";
            htmlName = Path.GetTempPath() + @"\MikeRSS2.html";
            xslName =  System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\MikeRSS2.xsl";
        }

        public void getDataBase(DataBaseEngine data)
        {
        }
        public void setOpml(Opml opml) { }  

        public void setOwner(System.Windows.Window window)
        { }

        public void addToToolbar(System.Windows.Controls.ToolBar ToolBar) { }

        public String whatStd(XmlDocument rssdocument)
        {
            XmlNode myNode = xmlDoc.DocumentElement.SelectSingleNode("/rss");
            if (myNode == null) return "not rss";
            System.Xml.XmlAttributeCollection col = myNode.Attributes;
            for (int i = 0; i < col.Count; i++)
            {
                XmlNode n = col.Item(i);
                if (n.Name == "version")
                    return n.Value;
            }
            return null;
        }

        private String whatEncoding()
        {
            XmlNode xn = xmlDoc.FirstChild;
            while (xn != null && xn.NodeType != XmlNodeType.XmlDeclaration)
                xn = xn.NextSibling;
            if (xn == null) return "UTF-8";     //default encoding
            String enc = ((XmlDeclaration)xn).Encoding;
            if (enc == null || enc == "") return "UTF-8";   //default encoding
            return enc;
        }

        public bool canParse()
        {
            return ("2.0" == whatStd(xmlDoc));
        }

        public void setDocument(XmlDocument doc)
        {
            xmlDoc = doc;
        }

        public string parsedHTML()
        {
            //Check if the document already has a style sheet associated:
            /*XmlNode myNode = xmlDoc.DocumentElement.SelectSingleNode("/");
            XmlNode sibling = myNode.FirstChild;
            while (sibling != null)
            {
                if (sibling.LocalName == "xml-stylesheet")
                    return xmlDoc.OuterXml;
                sibling = sibling.NextSibling;
            }*/

            try
            {
                XmlTextWriter w = new XmlTextWriter(fileName, Encoding.GetEncoding(whatEncoding()));
                xmlDoc.Save(w);
                //w.Flush();
                w.Close();
                TransformXML();
                String s = File.OpenText(htmlName).ReadToEnd();
                return s;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        private void TransformXML()
        {
            // Create a resolver with default credentials.

            XmlUrlResolver resolver = new XmlUrlResolver();

            resolver.Credentials = System.Net.CredentialCache.DefaultCredentials;

            // transform the *.xml file to html

            XslTransform transform = new XslTransform();

            // load up the stylesheet

            transform.Load(xslName, resolver);

            // perform the transformation
            transform.Transform(fileName, htmlName, resolver);
        }

        public void changeTitleColor(string color) { }

        public void changeBackgroundColor(string color) { }

        public void showConfiguration()
        { }

        public void addToToolbar()
        { }

        public void feedChanged(string name, string category)
        { }

        public string description()
        {
            return "Mike's RSS 2.0 Plugin";
        }

        public string name()
        {
            return "Mike's RSS 2.0";
        }
    }
}
