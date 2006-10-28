using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Threading;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.Resources;
using System.Reflection;

using PluginInterface;

namespace Plugin
{
    public class configWindow
    {
        configWindow()
        {
            //MessageBox.Show("Aceasta e fereastra de configurare.");
        }
    }
    public class Atom1 : rssInterface
    {
        XmlDocument doc;
        string fileName;
        string htmlName;
        string xslName;
        string oldtitlecolor;
        string oldbackgroundcolor;
        //configWindow conf = new configWindow();

        public void getDataBase(DataBaseEngine data)
        {
        }


        public Atom1()
        {
            fileName = Environment.CurrentDirectory + @"\Plugins\www.rss";
            htmlName = Environment.CurrentDirectory + @"\Plugins\www.htm";
            xslName = Environment.CurrentDirectory + @"\Plugins\atom1.xsl";

            oldtitlecolor = "maroon";
            oldbackgroundcolor = "#efeff5";
        }
        
        public string whatStd(XmlDocument rssdoc)
        {
            try
            {
                string standard = rssdoc.DocumentElement.SelectSingleNode("/*").Name;
                return standard;
            }
            catch (System.NullReferenceException nullRef)
            {
                return null;
                //return nullRef.Message;
            }

        }

        public void feedChanged(string name, string category)
        { }

        public void addToToolbar(System.Windows.Controls.ToolBar ToolBar)
        { 
        
        }

        public void setOwner(System.Windows.Window window)
        { }

        public bool canParse()
        {
            return ("feed" == whatStd(doc)); 
            //return true;
        }

        public void setDocument(XmlDocument doc)
        {
            this.doc = doc;
        }

        public void changeTitleColor(string color)
        {
            if (File.Exists(xslName))
            {
                string s = "";
                using(StreamReader sr = File.OpenText(xslName))
                {                    
                    s = sr.ReadToEnd();
                    s=s.Replace(oldtitlecolor, color);
                    oldtitlecolor = color;
                }

                using (StreamWriter sw = File.CreateText(xslName))
                {
                    sw.Write(s);
                }
            } 
        }

        public void changeBackgroundColor(string color)
        {
            if (File.Exists(xslName))
            {
                string s = "";
                using (StreamReader sr = File.OpenText(xslName))
                {
                    s = sr.ReadToEnd();
                    s = s.Replace(oldbackgroundcolor, color);
                    oldbackgroundcolor = color;
                }

                using (StreamWriter sw = File.CreateText(xslName))
                {
                    sw.Write(s);
                }
            }
        }

        public string parsedHTML()
        {

            XmlTextWriter w = new XmlTextWriter( fileName,Encoding.UTF8); 
            doc.Save(w);
            w.Flush();
            w.Close();
            TransformXML();
            string s=File.OpenText(htmlName).ReadToEnd();
         
            return s;
        }

        public void showConfiguration()
        {
            
        }

        public void addToToolbar()
        { }

        public string description()
        {
            return "Default parsing mechanism for ATOM 1.0 feeds";
        }

        public string name()
        {
            return "ATOM 1.0";
        }

        private  void TransformXML()
        {

            // Create a resolver with default credentials.

            XmlUrlResolver resolver = new XmlUrlResolver();

            resolver.Credentials = System.Net.CredentialCache.DefaultCredentials;

            // transform the personnel.xml file to html

            XslTransform transform = new XslTransform();

            // load up the stylesheet

            transform.Load(xslName, resolver);

            if (File.Exists(fileName))
            {
                string s = "";
                using (StreamReader sr = File.OpenText(fileName))
                {
                    s = sr.ReadToEnd();

                    Assembly asm = Assembly.GetExecutingAssembly();
                    ResourceManager rm = new ResourceManager("Plugin.Properties.Resource1", asm);
                    rm.ReleaseAllResources();


                    s = s.Replace(rm.GetString("Bla"), " ");
                }

                using (StreamWriter sw = File.CreateText(fileName))
                {
                    sw.Write(s);
                }
            }
            // perform the transformation

            transform.Transform(fileName,htmlName, resolver);
        }
    }
}
