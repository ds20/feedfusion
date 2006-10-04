using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using PluginInterface;
using System.IO;
using System.Reflection;
using System.Runtime.Remoting;
using System.Collections.Specialized;
using System.Resources;

namespace WinFXConsumer
{
    public class pluginManager
    {
        ToolBar bar;
        public List<rssInterface> plugins;
        public StringCollection pluginCollection;
        DataBaseEngine data;
        Window wnd;

        public pluginManager(ToolBar tb,DataBaseEngine ndata,Window window)
        {
            wnd = window;
            data = ndata;
            bar = tb;
            Type myType = Type.GetType("PluginInterface.rssInterface,PluginInterface");
            if (myType == null) MessageBox.Show("Critical error. Cannot find plugin Interface.");
            loadPlugins(Environment.CurrentDirectory, myType);
        }


        private void loadPlugins(string folder, Type myDataType)
        {
            DirectoryInfo myDir = new DirectoryInfo(folder);
            FileInfo[] myFiles = myDir.GetFiles("*.dll");

            plugins = new List<rssInterface>();
            StringCollection pluginCollection2 = Properties.Settings.Default.PluginCollection;
            pluginCollection = new StringCollection();

            if (pluginCollection2 == null)
                pluginCollection2 = new StringCollection();
            else
            {
                for (int i = 0; i < pluginCollection2.Count; i++)
                    pluginCollection.Add(null);     //some of these nulls will be replaced. helps keep saved order
                foreach (FileInfo f in myFiles)
                {
                    String fileName = f.FullName;
                    if (pluginCollection2.Contains(fileName))
                    {
                        int index = pluginCollection2.IndexOf(fileName);
                        pluginCollection.Insert(index, fileName);   //the corresponding null moves down 1 position
                        pluginCollection.RemoveAt(index + 1);       //delete the corresponding null
                    }
                }

                for (int i = 0; i < pluginCollection.Count; i++)    //delete the remaining nulls
                    if (pluginCollection[i] == null)
                        pluginCollection.RemoveAt(i);
            }

            for (int i = 0; i < pluginCollection.Count; i++)
                plugins.Add(null);

            foreach (FileInfo f in myFiles)
            {
                String crtFileName = f.FullName;
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
                                if (!plugins.Contains((rssInterface)(myObj.Unwrap())))
                                {
                                    rssInterface myUnwrappedObj=(rssInterface)(myObj.Unwrap());

                                    if (pluginCollection.Contains(crtFileName))
                                    {
                                        int index = pluginCollection.IndexOf(crtFileName);
                                        plugins.Insert(index, myUnwrappedObj);   //this moves the null 1 position
                                        plugins.RemoveAt(index + 1);              //deletes the null
                                    }
                                    else
                                    {
                                        plugins.Add(myUnwrappedObj);       //add to end
                                        pluginCollection.Add(crtFileName);  //and in the FileNameList
                                    }
                                    myUnwrappedObj.setOwner(wnd); 
                                    myUnwrappedObj.getDataBase(data);  
                                }
                                
                            }
                        }
                    }
                }


                catch (Exception exc)
                {
                    System.Diagnostics.Trace.WriteLine(string.Format("{0}: {1}", this, exc.Message));
                }

            }

            //clean the remaining nulls in plugins
            for (int i = 0; i < plugins.Count; i++)
                if (plugins[i] == null)
                {
                    plugins.RemoveAt(i);
                    pluginCollection.RemoveAt(i);
                }

            

            //MessageBox.Show( plugins.Count + " plugins loaded successfully!");
        }

        public int number
        {
            get { return plugins.Count; }
        }


    }
}
