using System;
using System.Collections.Generic;
using System.Text;
using PluginInterface;
using System.IO;
using System.Xml;
using System.Threading;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.Windows.Forms;
using System.Resources;
using System.Reflection;
using System.Xml.Schema;

namespace rss1
{  


    //PluginInterface implementation
    public class Rss1 : rssInterface
    {
        XmlDocument doc;
        string fileName;
        string htmlName;
        string xslName;
        string xsdName;
        string oldtitlecolor;
        string oldbackgroundcolor;
        ToolBarButton tb;

        public void getDataBase(DataBaseEngine data)
        {
        }

        public void feedChanged(string name, string category)
        { }

        public void setOwner(System.Windows.Window window)
        { }


        //Configuration Window code
        public class config : Form,IDisposable 
        {
            string text;
            Button btnCommand=new Button();
            TextBox txt=new TextBox();
            ResourceManager rm;
            
            ~config()
            {
                try 
                { 
                    ResourceWriter writer = new ResourceWriter(Environment.CurrentDirectory + @"\" + "Plugin.Properties.resource1.resources"); 
                    writer.AddResource("TextColor",txt.Text);                    
                    MessageBox.Show("Resources saved");
                    writer.Close(); 
                }
                catch 
                { 
                    MessageBox.Show("Error. Could not open resource for writing."); 
                }
            
            }
            public config()
            {


                this.Load += new EventHandler(config_Load); 
                //adding controls cannot be done until form is loaded properly
                //so we use the Load event
            }






            private void config_Load(object sender, EventArgs e)
            {
                /*
                //add the controls to the form
                this.Controls.Add(btnCommand);
                this.Controls.Add(txt);

                //set the control properties
                btnCommand.Width = 100;
                btnCommand.Height = 50;
                btnCommand.Left = 100;
                btnCommand.Top = 50;
                btnCommand.Text = "Save as resource";
                btnCommand.Visible = true;
                btnCommand.Click+=new EventHandler(btnCommand_Click);

                txt.Width = 100;
                txt.Height = 50;
                txt.Top = 170;
                txt.Left = 100;
              */
                //add the controls to the form
                this.Controls.Add(btnCommand);
                this.Controls.Add(txt);

                //set the control properties
                btnCommand.Width = 100;
                btnCommand.Height = 50;
                btnCommand.Left = 100;
                btnCommand.Top = 50;
                btnCommand.Text = "Text";
                btnCommand.Visible = true;
                btnCommand.Click += btnCommand_Click; 

                txt.Width = 100;
                txt.Height = 50;
                txt.Top = 170;
                txt.Left = 100;
                
                Assembly asm = Assembly.GetExecutingAssembly();
                rm = ResourceManager.CreateFileBasedResourceManager("Plugin.Properties.resource1", System.Environment.CurrentDirectory, null);  
                rm.ReleaseAllResources();
                try
                {
                    txt.Text = rm.GetString("TextColor");
                }
                catch (Exception)
                {
                    txt.Text = "no resource";
                }
                rm = null;
            }

            private void btnCommand_Click(object sender,EventArgs Args )
            {
                //FileStream fs = new FileStream("Rss1plugin.Properties", FileMode.OpenOrCreate, FileAccess.Write);
                System.GC.Collect();
                
            }

        }



        public Rss1()
        {
            fileName = Environment.CurrentDirectory + "\\rss1.rss";
            htmlName = Environment.CurrentDirectory + "\\rss1.htm";
            xslName = Environment.CurrentDirectory + "\\rss1.xsl";
            xsdName = Environment.CurrentDirectory + "\\rss-1_0.xsd";
            oldtitlecolor = "<h3 style=\"color:"+"maroon"+"\">";
            oldbackgroundcolor = "<xsl:text/>background-color: " + "#efeff5";
        }


        public string whatStd(XmlDocument rssdocument)
        {
            try
            {
                string standard = rssdocument.DocumentElement.SelectSingleNode("/*").Name;
                return standard;
            }
            catch (System.NullReferenceException nullRef)
            {
                return null;
                //return nullRef.Message;
            }
        }

        public bool canParse()
        {
            return ("rdf:RDF" == whatStd(doc));
            //return true;
        }

        public void setDocument(XmlDocument doc)
        {
            this.doc = doc;
        }

        public void changeTitleColor(string color)
        {
            color = "<h3 style=\"color:" + color + "\">";
            if (File.Exists(xslName))
            {
                string s = "";
                using (StreamReader sr = File.OpenText(xslName))
                {
                    s = sr.ReadToEnd();
                    s = s.Replace(oldtitlecolor, color);
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
            color = "<xsl:text/>background-color: " + color;
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

            XmlTextWriter w = new XmlTextWriter(fileName, Encoding.UTF8);
          
            doc.Save(w);
            w.Flush();
            w.Close();
            TransformXML();
            string s = File.OpenText(htmlName).ReadToEnd();
           
            return s;
            /*
            XmlValidatingReader reader = null;
            XmlSchemaCollection myschema = new XmlSchemaCollection();
            //ValidationEventHandler eventHandler = new ValidationEventHandler();


            try
            {
                //Create the XML fragment to be parsed.

                //Create the XmlParserContext.
                XmlParserContext context = new XmlParserContext(null, null, "", XmlSpace.None);

                //Implement the reader. 
                reader = new XmlValidatingReader(doc.OuterXml, XmlNodeType.Element, context);
                //Add the schema.
                myschema.Add("", xsdName);

                //Set the schema type and add the schema to the reader.
                reader.ValidationType = ValidationType.Schema;
                reader.Schemas.Add(myschema);

                while (reader.Read())
                {
                }

                XmlTextWriter w = new XmlTextWriter(fileName, Encoding.UTF8);
                doc.Save(w);
                w.Flush();
                w.Close();
                TransformXML();
                string s = File.OpenText(htmlName).ReadToEnd();

                return s;
            }
            catch (XmlException XmlExp)
            {
                return XmlExp.Message;
            }
            catch (XmlSchemaException XmlSchExp)
            {
                return XmlSchExp.Message;
            }
            catch (Exception GenExp)
            {
                return GenExp.Message;
            }
            finally
            {

            }*/

        }

        public void showConfiguration()
        {          
            config cfg = new config();
            cfg.ShowDialog();             
        }

        public void addToToolbar(System.Windows.Controls.ToolBar ToolBar)
        {
            //System.Windows.Controls.Button btn = new System.Windows.Controls.Button();
            //btn.Content = "rss1";
            //btn.Click+=new System.Windows.RoutedEventHandler(btn_Click); 
            //ToolBar.Items.Add(btn); 
        }

        public void btn_Click(object source,System.Windows.RoutedEventArgs e)
        {
            showConfiguration(); 
        }


        public string description()
        {
            return "Default parsing mechanism for RSS 1.0 .";
        }

        public string name()
        {
            return "rss 1.0"; 
        }

        private void TransformXML()
        {

            // Create a resolver with default credentials.

            XmlUrlResolver resolver = new XmlUrlResolver();

            resolver.Credentials = System.Net.CredentialCache.DefaultCredentials;

            // transform the personnel.xml file to html

            XslTransform transform = new XslTransform();

            // load up the stylesheet
            
            transform.Load(xslName , resolver);
            
            if (File.Exists(fileName))
            {
                string s = "";
                using (StreamReader sr = File.OpenText(fileName))
                {
                    s = sr.ReadToEnd();

                    Assembly asm = Assembly.GetExecutingAssembly();
                    ResourceManager rm = new ResourceManager("Rss1plugin.Properties.Resources", asm);
                    rm.ReleaseAllResources();


                    //s = s.Replace(rm.GetString("Bla"), " ");
                }

                using (StreamWriter sw = File.CreateText(fileName))
                {
                    sw.Write(s);
                }
            }

            // perform the transformation

            transform.Transform(fileName, htmlName, resolver);

        }
    }
}
