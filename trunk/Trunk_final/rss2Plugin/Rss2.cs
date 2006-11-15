using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Threading;
using System.Xml.XPath;
using System.Xml.Xsl;
using PluginInterface;
using System.Resources;
using System.Reflection;
using System.Xml.Schema;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace rss2
{
    public class Rss2: rssInterface
    {
        XmlDocument doc;
        string fileName;
        string htmlName;
        string xslName;
        string xsdName1;
        string xsdName2;
        string xsdName3;
        string oldtitlecolor;
        string oldtitleitemcolor;
        string oldbackgroundcolor;

        Window win;
        ListBox list;
        ComboBox combo;
        Button btn;
        ResourceManager rm;
        string clr,act;

        public void loadWindow()
        {
            clr = "";
            act = "";
            win = new System.Windows.Window();
            win.ResizeMode = ResizeMode.NoResize;   
            win.Title = "Rss2 Plugin Configuration Window";
            win.Width = 450;
            win.Height = 120;
            
            list = new ListBox();
            list.Margin = new Thickness(3); 
            list.Height = 60;
            list.Width = 92;
            list.VerticalAlignment = VerticalAlignment.Stretch;
            list.Items.Add("Background");
            list.Items.Add("Title");
            list.Items.Add("Title item");
            DockPanel.SetDock(list, Dock.Left);
            list.SelectionChanged += new SelectionChangedEventHandler(OnSelection);

            combo = new ComboBox();
            combo.Margin = new Thickness(3,8,3,3);  
            combo.Height = 20;
            combo.VerticalAlignment = VerticalAlignment.Top;
            combo.HorizontalAlignment = HorizontalAlignment.Stretch;
            combo.Items.Add("Red");
            combo.Items.Add("Green");
            combo.Items.Add("Blue");
            combo.Items.Add("Maroon");
            combo.Items.Add("Light gray");
            DockPanel.SetDock(combo, Dock.Top);
            combo.SelectionChanged += new SelectionChangedEventHandler(combo_SelectionChanged);

            btn = new Button();
            btn.Margin = new Thickness(3);  
            btn.Content = "Set";            
            btn.Width = 100;
            //btn.Height = 30;            
            DockPanel.SetDock(btn, Dock.Bottom);
            btn.Click += new RoutedEventHandler(btn_Click);

            DockPanel rootPanel = new DockPanel();
            
            rootPanel.Children.Add(btn);
            rootPanel.Children.Add(combo);            
            

            DockPanel dockPanel = new DockPanel();
            dockPanel.Children.Add(list);
            dockPanel.Children.Add(rootPanel);
            StackPanel sp = new StackPanel();
            Label lblInfo = new Label();
            lblInfo.Content = "Please select an element to change its color";
            sp.Children.Add(lblInfo);
            sp.Children.Add(dockPanel);
            win.Content = sp;
            win.Show();
        }
        public void setOpml(Opml opml) { }  
        private void btn_Click(object sender, RoutedEventArgs e)
        {
            switch (act)
            {
                case "Background":
                    changeBackgroundColor(clr);
                    try
                    {
                        ResourceWriter writer = new ResourceWriter(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\" + "Plugin.Properties.resource2.resources");
                        writer.AddResource("BackgroundColor", clr);
                        writer.AddResource("TitleItemColor", oldtitleitemcolor);
                        writer.AddResource("TitleColor", oldtitlecolor);

                        oldbackgroundcolor = clr;
                        writer.Close();
                    }
                    catch
                    {
                        MessageBox.Show("Error. Check that you have sufficient privileges and that there is enough disk space.");
                    }
                    break;
                case "Title":
                    changeTitleColor(clr);
                    try
                    {
                        ResourceWriter writer = new ResourceWriter(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\" + "Plugin.Properties.resource2.resources");
                        writer.AddResource("BackgroundColor", oldbackgroundcolor);
                        writer.AddResource("TitleItemColor", oldtitleitemcolor);
                        writer.AddResource("TitleColor", clr);

                        oldtitlecolor = clr;
                        writer.Close();
                    }
                    catch
                    {
                        MessageBox.Show("Error. Could not open resource for writing.");
                    }
                    break;
                case "Title item":
                    changeTitleItemColor(clr);
                    try
                    {
                        ResourceWriter writer = new ResourceWriter(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\" + "Plugin.Properties.resource2.resources");
                        writer.AddResource("BackgroundColor", oldbackgroundcolor);
                        writer.AddResource("TitleItemColor", clr);
                        writer.AddResource("TitleColor", oldtitlecolor);

                        oldtitleitemcolor = clr;
                        writer.Close();
                    }
                    catch
                    {
                        MessageBox.Show("Error. Could not open resource for writing.");
                    }
                    break;
            }
        }

        private void combo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (combo.SelectedIndex != -1)
            {
                clr=((string)combo.SelectedValue).ToLower() ;
                if (clr == "light gray")
                    clr = "#efeff5";
            }
        }
        private void OnSelection(object sender, SelectionChangedEventArgs aArgs)
        {
            if (list.SelectedIndex != -1)
            {
                act=(string)list.SelectedValue;
            }
        }
       
        public Rss2()
        {
            fileName =  Path.GetTempPath()+"\\rss2.rss";
            htmlName =  Path.GetTempPath()+  "\\rss2.htm";
            xslName =  System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\rss2.xsl";
            xsdName1 =  System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\rss-0_91.xsd";
            xsdName2 =  System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\rss-0_92.xsd";
            xsdName3 =  System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\rss-0_93.xsd";


            
            Assembly asm = Assembly.GetExecutingAssembly();
            rm = ResourceManager.CreateFileBasedResourceManager("Plugin.Properties.resource2", System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), null);
            rm.ReleaseAllResources();
            oldtitleitemcolor = rm.GetString("TitleItemColor");
            oldtitlecolor = rm.GetString("TitleColor");
            oldbackgroundcolor = rm.GetString("BackgroundColor");
            rm = null;            
        }


        public void setOwner(System.Windows.Window window)
        { }

        public void addToToolbar(System.Windows.Controls.ToolBar ToolBar)
        {
            System.Windows.Controls.Button toolbtn = new System.Windows.Controls.Button();
            toolbtn.ToolTip  = "Shows the Rss2 Plugin Configuration Window";
            
            Image im = new Image();
            im.Width = 30;
            im.Height = 30;
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.DecodePixelWidth = 30; 
            bi.UriSource = new Uri( System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\icons\\rss.png");
            bi.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            bi.CacheOption = BitmapCacheOption.OnLoad;
            bi.EndInit();
            im.Source = bi;
            toolbtn.Content = im;
            toolbtn.Click += new System.Windows.RoutedEventHandler(toolbtn_Click);
            ToolBar.Items.Add(toolbtn);
   
        }

        public void toolbtn_Click(object source, System.Windows.RoutedEventArgs e)
        {
            showConfiguration();
        }

        public void getDataBase(DataBaseEngine data)
        {
        }


        public string whatStd(XmlDocument rssdocument)
        {
            try
            {
                string standard = rssdocument.DocumentElement.SelectSingleNode("/*").Name;
                return standard;
            }
            catch (System.NullReferenceException)
            {
                return null;
            }
        }

        public void feedChanged(string name, string category)
        { }

        public bool canParse()
        {
            return ("rss" == whatStd(doc)); 
            //return true;
        }

        public void setDocument(XmlDocument doc)
        {
            this.doc = doc;
        }

        public void changeTitleItemColor(string color)
        {           
            color = "<p><h3 style=\"color:" + color;
            string xcolor = "<p><h3 style=\"color:" + oldtitleitemcolor;
            if (File.Exists(xslName))
            {
                string s = "";
                using (StreamReader sr = File.OpenText(xslName))
                {
                    s = sr.ReadToEnd();
                    s = s.Replace(xcolor, color);
                    xcolor = color;
                }

                using (StreamWriter sw = File.CreateText(xslName))
                {
                    sw.Write(s);
                }
            }
        }

        public void changeTitleColor(string color)
        {           
            color = "<p><h2 style=\"color:" + color;            
            string xcolor = "<p><h2 style=\"color:" + oldtitlecolor;
            if (File.Exists(xslName))
            {
                string s = "";
                using (StreamReader sr = File.OpenText(xslName))
                {
                    s = sr.ReadToEnd();
                    s = s.Replace(xcolor, color);
                    xcolor = color;
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
            string xcolor = "<xsl:text/>background-color: " + oldbackgroundcolor;
            if (File.Exists(xslName))
            {
                string s = "";
                using (StreamReader sr = File.OpenText(xslName))
                {
                    s = sr.ReadToEnd();
                    s = s.Replace(xcolor, color);
                    xcolor = color;
                }

                using (StreamWriter sw = File.CreateText(xslName))
                {
                    sw.Write(s);
                }
            }
        }

        public string parsedHTML()
        {
            //////////////////////////////////////////////////////////////
            //////Validation, man! I'm telling you it's important!!! /////
            //////////////////////////////////////////////////////////////
            /////   http://www.thearchitect.co.uk/schemas/rss-2_0.xsd ////
            /////   http://support.microsoft.com/kb/318504/           ////
            //////////////////////////////////////////////////////////////

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
                myschema.Add("", xsdName1);
                
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
            catch (XmlSchemaException XmlSchExp)
            {
                if (XmlSchExp.Message != "The 'version' attribute has an invalid value according to its data type.")
                    return XmlSchExp.Message;
            }
            catch (XmlException XmlExp)
            {
                return "<b>Plugin Error. Parsing xml failed due XML parsing error.</b> "+ XmlExp.Message;
            }
            
            catch (Exception GenExp)
            {
                return GenExp.Message;
            }
            finally
            {
                
            }


            try
            {
                //Create the XML fragment to be parsed.

                //Create the XmlParserContext.
                XmlParserContext context = new XmlParserContext(null, null, "", XmlSpace.None);

                //Implement the reader. 
                reader = new XmlValidatingReader(doc.OuterXml, XmlNodeType.Element, context);
                //Add the schema.
                myschema.Add("", xsdName2);
                
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
            catch (XmlSchemaException XmlSchExp)
            {
                if (XmlSchExp.Message != "The 'version' attribute has an invalid value according to its data type.")
                    return XmlSchExp.Message;
            }
            catch (XmlException XmlExp)
            {
                return "<h2>Parsing error: </h2> "+XmlExp.Message;
            }

            catch (Exception GenExp)
            {
                return GenExp.Message;
            }
            finally
            {

            }


            try
            {
                //Create the XML fragment to be parsed.

                //Create the XmlParserContext.
                XmlParserContext context = new XmlParserContext(null, null, "", XmlSpace.None);

                //Implement the reader. 
                reader = new XmlValidatingReader(doc.OuterXml, XmlNodeType.Element, context);
                //Add the schema.
                myschema.Add("", xsdName3);

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
            catch (XmlSchemaException XmlSchExp)
            {
                if (XmlSchExp.Message != "The 'version' attribute has an invalid value according to its data type.")
                    return XmlSchExp.Message;
            }
            catch (XmlException XmlExp)
            {
                return XmlExp.Message;
            }

            catch (Exception GenExp)
            {
                return GenExp.Message;
            }
            finally
            {

            }
            return "<h2><b>Plugin error.<b> RSS2 plugin: Parsing failed. This feed does not meet any standard specifications.</h2>";            
        }

        public void showConfiguration()
        {
            loadWindow();
        }

        public string description()
        {
            return "Default parsing mechanism for RSS 2.0 ,0.91, 0.92 and 0.93";
        }

        public string name()
        {
            return "rss0.93 & rss2.0";
        }

        private  void TransformXML()
        {

            // Create a resolver with default credentials.

            XmlUrlResolver resolver = new XmlUrlResolver();

            resolver.Credentials = System.Net.CredentialCache.DefaultCredentials;

            // transform the *.xml file to html

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
                    ResourceManager rm = new ResourceManager("rss2.Properties.Resource1", asm);
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
