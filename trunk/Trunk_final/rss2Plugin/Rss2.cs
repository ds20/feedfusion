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
        Label[] txt;
        TextBox hide;
        Button btn;
        ResourceManager rm;

        public void loadWindow()
        {           
            win = new System.Windows.Window();
            win.Title = "Rss2 Plugin Configuration Window";
            win.Width = 400;
            win.Height = 200;

            hide = new TextBox();
            hide.Text = "";
            hide.Visibility = Visibility.Hidden;

            txt = new Label[4];
            
            txt[0] = new Label();
            txt[0].Content = "Background";            
            txt[0].Width = 100;
            txt[0].Height = 30;         
            txt[0].MouseEnter += new System.Windows.Input.MouseEventHandler(txt_MouseEnter);
            txt[0].MouseLeave += new System.Windows.Input.MouseEventHandler(txt_MouseLeave);
            txt[0].MouseDown += new System.Windows.Input.MouseButtonEventHandler(txt_MouseDown);
            txt[0].FontWeight = FontWeights.Bold;
            txt[1] = new Label();
            txt[1].Content = "Title";
            txt[1].Width = 100;
            txt[1].Height = 30;
            txt[1].MouseEnter += new System.Windows.Input.MouseEventHandler(txt_MouseEnter);
            txt[1].MouseLeave += new System.Windows.Input.MouseEventHandler(txt_MouseLeave);
            txt[1].MouseDown += new System.Windows.Input.MouseButtonEventHandler(txt_MouseDown);
            txt[1].FontWeight = FontWeights.Bold;
            txt[2] = new Label();
            txt[2].Content = "Title item";
            txt[2].Width = 100;
            txt[2].Height = 30;            
            txt[2].MouseEnter += new System.Windows.Input.MouseEventHandler(txt_MouseEnter);
            txt[2].MouseLeave += new System.Windows.Input.MouseEventHandler(txt_MouseLeave);
            txt[2].MouseDown += new System.Windows.Input.MouseButtonEventHandler(txt_MouseDown);
            txt[2].FontWeight = FontWeights.Bold;

            txt[3] = new Label();
            txt[3].Width = 150;
            txt[3].Height = 30;
            txt[3].Visibility = Visibility.Hidden;
            
            btn = new Button();
            btn.Content = "<<Back";
            btn.Visibility = Visibility.Hidden;
            btn.Width = 100;
            btn.Height = 30;
            btn.Click += new RoutedEventHandler(btn_Click);
                      

            list = new ListBox();            
            list.Height = 150;
            list.Width = 80;
            list.Visibility = Visibility.Hidden;
            list.Items.Add("red");
            list.Items.Add("green");
            list.Items.Add("blue");
            list.Items.Add("maroon");
            list.Items.Add("#efeff5");
            list.SelectionChanged += new SelectionChangedEventHandler(OnSelection);
                        
            StackPanel rootPanel = new StackPanel();
            
            rootPanel.Children.Add(txt[0]);
            rootPanel.Children.Add(txt[1]);
            rootPanel.Children.Add(txt[2]);
            rootPanel.Children.Add(txt[3]);
            rootPanel.Children.Add(btn);
            
            DockPanel dockPanel = new DockPanel();
            dockPanel.Children.Add(rootPanel);
            dockPanel.Children.Add(list);
            win.Content = dockPanel;
            win.Show();
        }

        void txt_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Label l = new Label();
            l = (Label)sender;
            hide.Text = (string)l.Content;
            int i = 0;
            switch (hide.Text)
            {
                case "Background":
                    i = 0;
                    break;
                case "Title":
                    i = 1;
                    break;
                case "Title item":
                    i = 2;
                    break;
            }
            for (int j = 0; j < 3; j++)
                if (i != j)
                    txt[j].Visibility = Visibility.Hidden;
            txt[3].Content = "Color for:  " + (string)l.Content;
            txt[3].Visibility = Visibility.Visible;
            list.Visibility = Visibility.Visible;
            btn.Visibility = Visibility.Visible;          
        }

        void txt_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            txt[3].Visibility = Visibility.Hidden;
        }

        
         void txt_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
         {
             Label l = new Label();
             l = (Label)sender;
             hide.Text = (string)l.Content;
             txt[3].Content = "Color for:  " + (string)l.Content;
             txt[3].Visibility = Visibility.Visible;                     
         }
        
         private void btn_Click(object sender, RoutedEventArgs e)
         {
             for (int j = 0; j < 3; j++)
                 txt[j].Visibility = Visibility.Visible;
             btn.Visibility = Visibility.Hidden;
             list.Visibility = Visibility.Hidden;
         }         
         
        private void OnSelection(object sender, SelectionChangedEventArgs aArgs)
        {
            if (list.SelectedIndex != -1)
            {
                switch (hide.Text)
                 {
                     case "Background":
                         changeBackgroundColor((string)list.SelectedValue);
                         try
                         {
                             ResourceWriter writer = new ResourceWriter(Environment.CurrentDirectory + @"\" + "Plugin.Properties.resource2.resources");
                             writer.AddResource("BackgroundColor", (string)list.SelectedValue);
                             writer.AddResource("TitleItemColor", oldtitleitemcolor);
                             writer.AddResource("TitleColor", oldtitlecolor);

                             oldbackgroundcolor = (string)list.SelectedValue;                             
                             writer.Close();
                         }
                         catch
                         {
                             MessageBox.Show("Error. Check that you have sufficient privileges and that there is enough disk space.");
                         }
                         break;
                     case "Title":
                         changeTitleColor((string)list.SelectedValue);
                         try
                         {
                             ResourceWriter writer = new ResourceWriter(Environment.CurrentDirectory + @"\" + "Plugin.Properties.resource2.resources");
                             writer.AddResource("BackgroundColor", oldbackgroundcolor);
                             writer.AddResource("TitleItemColor", oldtitleitemcolor);
                             writer.AddResource("TitleColor", (string)list.SelectedValue);                             
                             
                             oldtitlecolor = (string)list.SelectedValue;                             
                             writer.Close();
                         }
                         catch
                         {
                             MessageBox.Show("Error. Could not open resource for writing.");
                         }
                         break;
                     case "Title item":
                         changeTitleItemColor((string)list.SelectedValue);
                         try
                         {
                             ResourceWriter writer = new ResourceWriter(Environment.CurrentDirectory + @"\" + "Plugin.Properties.resource2.resources");
                             writer.AddResource("BackgroundColor", oldbackgroundcolor);
                             writer.AddResource("TitleItemColor", (string)list.SelectedValue);
                             writer.AddResource("TitleColor", oldtitlecolor);

                             oldtitleitemcolor = (string)list.SelectedValue;                            
                             writer.Close();
                         }
                         catch
                         {
                             MessageBox.Show("Error. Could not open resource for writing.");
                         }                         
                         break;
                 }
                 list.Visibility = Visibility.Hidden;
                 for (int j = 0; j < 3; j++)
                     txt[j].Visibility = Visibility.Visible;
                 btn.Visibility = Visibility.Hidden; 
            }                
        }

        public Rss2()
        {
            fileName = Environment.CurrentDirectory+"\\Plugins\\rss2.rss";
            htmlName = Environment.CurrentDirectory + "\\Plugins\\rss2.htm";
            xslName = Environment.CurrentDirectory + "\\Plugins\\rss2.xsl";
            xsdName1 = Environment.CurrentDirectory + "\\Plugins\\rss-0_91.xsd";
            xsdName2 = Environment.CurrentDirectory + "\\Plugins\\rss-0_92.xsd";
            xsdName3 = Environment.CurrentDirectory + "\\Plugins\\rss-0_93.xsd";


            
            Assembly asm = Assembly.GetExecutingAssembly();
            rm = ResourceManager.CreateFileBasedResourceManager("Plugin.Properties.resource2", System.Environment.CurrentDirectory, null);
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
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri(Environment.CurrentDirectory + "\\icons\\rss.png");
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
                //return nullRef.Message;
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
                return XmlExp.Message;
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
                return XmlExp.Message;
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
            return "The version of this rss in unknown to us";

            
        }

        public void showConfiguration()
        {
            loadWindow();
        }

        public string description()
        {
            return "Default parsing mechanism for RSS 2.0 and 0.93";
        }

        public string name()
        {
            return "rss2.0 & rss0.93";
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
