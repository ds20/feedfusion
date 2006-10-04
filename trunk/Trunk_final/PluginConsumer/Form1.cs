using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.Reflection;
using System.IO;
using System.Runtime.Remoting;
using PluginInterface;
using System.Xml;
using System.Threading;

using System.Xml.XPath;
using System.Xml.Xsl ;

namespace PluginConsumer
{
    public partial class Form1 : Form
    {
        rssInterface[] plugins=new rssInterface[30];
        int nr = 0;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            Type myType = Type.GetType("PluginInterface.rssInterface,PluginInterface");
            if (myType==null) MessageBox.Show("NU E BINE");
            loadPlugins(Environment.CurrentDirectory, myType); 
        }

        //functia de incarcare a pluginurilor
        //e BUNA :D
        private void loadPlugins(string folder, Type myDataType)
        {
            DirectoryInfo myDir = new DirectoryInfo(folder);
            FileInfo[] myFiles = myDir.GetFiles("*.dll");
            
            foreach (FileInfo f in myFiles)
            {
                try
                {
                    Assembly myAssembly = Assembly.LoadFile(f.FullName);
                    Type[] myClasses = myAssembly.GetTypes();
                    foreach (Type t in myClasses)
                    {
                        Type[] interfaces = t.GetInterfaces();
                        foreach (Type myInterfaceType in interfaces)
                        {
                            if (myInterfaceType.Equals(myDataType))
                            {
                                ObjectHandle myObj = Activator.CreateInstanceFrom(f.FullName, t.ToString());
                                plugins.SetValue( (rssInterface)(myObj.Unwrap()),nr++ );
                                MessageBox.Show("S-a incarcat un plugin!"); 
                            }
                        }
                    }
                }

                catch (Exception exc)
                {
                    System.Diagnostics.Trace.WriteLine(string.Format("{0}: {1}", this, exc.Message));
                    
                }
            }
        }

        //procesare feed
        private void startDld()
        {
            //nu se face asa... tre sa facem altfel...
            findPluginFor(dldFeed(textBox1.Text)); 
        }


        //dld feed
        private XmlDocument dldFeed(string url)
        {
            
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(url);
            return xDoc;
        }

        //gaseste plugin pt feed
        public void findPluginFor(XmlDocument rssDocument)
        {
            int i=0;
            bool gasit = false;
            while ( (i < nr)&& (!gasit) )
            {
                plugins[i].setDocument(rssDocument);
                if (plugins[i].canParse())
                {
                    gasit = true;
                    MessageBox.Show("Pluginul care parseaza este "+plugins[i].description());
                    webBrowser1.DocumentText = plugins[i].parsedHTML(); 
                }
                i++;
            }
            if (!gasit) MessageBox.Show("Nu exista plugin instalat pentru acest tip de RSS.");
        }


        //gaseste tipul de feed
        


        private void button1_Click(object sender, EventArgs e)
        {
            Thread t1 = new Thread(new ThreadStart(startDld));
            t1.Start();


        }


        private void button2_Click(object sender, EventArgs e)
        {
            //rss 0.9   
            findPluginFor(dldFeed("http://www.antisource.com/backend/sitenews.xml"));
        }


        private void button3_Click(object sender, EventArgs e)
        {
            //rss 2.0
            findPluginFor(dldFeed("http://msdn.microsoft.com/rss.xml"));
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //atom1.0
            findPluginFor(dldFeed("http://www.atomenabled.org/atom.xml"));
        }

    }
}