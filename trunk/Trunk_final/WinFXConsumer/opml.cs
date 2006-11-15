using System;
using System.Collections.Generic;
using System.Text;
using Xml.Opml;
using System.Xml;
using System.Xml.Schema;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Indexer;
namespace WinFXConsumer
{
    public class opml:PluginInterface.Opml
    {
        public TreeViewItem Parse(string xmlfilename)
        {
            Parser p = new Parser(xmlfilename);

            string fileName =  Path.GetTempPath() + "\\opml.htm";

            Document doc = new Document();
            doc = p.RetrieveDocument();

            using (StreamWriter sw = File.CreateText(fileName))
            {
                sw.Write(doc.ToString());
            }

            return doc.Root;
        }

        public TreeViewItem Parse(string xmlfilename,ref int nrFeeds)
        {
            Parser p = new Parser(xmlfilename);

            string fileName = Path.GetTempPath() + "\\opml.htm";

            Document doc = new Document();
            doc = p.RetrieveDocument();

            using (StreamWriter sw = File.CreateText(fileName))
            {
                sw.Write(doc.ToString());
            }

            nrFeeds = doc.NrFeeds;
            return doc.Root;
        }

        public TreeViewItem getRootDataBase(FeedDB database)
        {
            TreeViewItem root = new TreeViewItem();
            root.Header = ("My Feeds");

            Feed f = new Feed();
            f.IsLeaf = false;
            f.Text = "My Feeds";
            f.HtmlUrl = f.XmlUrl = "";

            root.Tag = f;
            /*
            Feed g = new Feed();
            g.IsLeaf = true;
            g.HtmlUrl = g.XmlUrl = "";*/

            foreach (string s in database.getCategories())
            {
                TreeViewItem catNode = new TreeViewItem();
                catNode.Header = s;

                Feed h = new Feed();
                h.IsLeaf = false;
                h.HtmlUrl = h.XmlUrl = "";
                h.Text = s;
                catNode.Tag = h;
                foreach (XmlFeed feed in database.getFeeds(s))
                {
                    TreeViewItem rssItem = new TreeViewItem();
                    rssItem.Header = feed.url;

                    Feed g = new Feed();
                    g.IsLeaf = true;
                    g.XmlUrl = feed.url;
                    g.HtmlUrl = "";
                    if (feed.feedName != "" && feed.feedName != null)
                    {
                        
                        g.Text = feed.feedName.Replace("&"," ");
                    }
                    else
                    {
                        g.Text = feed.url;
                    }
                    rssItem.Tag = g;

                    catNode.Items.Add(rssItem);
                }
                root.Items.Add(catNode);
            }
            return root;
        }

        public string makeOpml(PluginInterface.DataBaseEngine   database)
        {
            Document doc = new Document();
            doc.Title = "Database";
            doc.AuthorName = "FeedFusion 0.9x";
            doc.AuthorEmail = "sbarlead@yahoo.com";
            doc.DateCreated = DateTime.Today.ToShortDateString();
            doc.DateModified = DateTime.Today.ToShortDateString();//;DateTime.Now.Date.ToString();

           

            TreeViewItem root = new TreeViewItem();
            root = getRootDataBase((FeedDB)database);

            doc.Root = doc.Now = root;

            return doc.ToOpml();
        }

        public void import(Object url_o, PluginInterface.DataBaseEngine database)
        {
            TreeViewItem root = new TreeViewItem();
            String enc;
            string fileName = Path.GetTempPath() + "\\opml.xml";
            XmlDocument doc = new XmlDocument();
            try { doc.Load((String)url_o); }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, (String)url_o);
            }

            string s = OpmlValidation(doc);
            if (s == "ok")
            {
                XmlNode xn = doc.FirstChild;
                while (xn != null && xn.NodeType != XmlNodeType.XmlDeclaration)
                    xn = xn.NextSibling;
                if (xn == null || xn.NodeType != XmlNodeType.XmlDeclaration) enc = "UTF-8";     //default encoding
                else
                {
                    enc = ((XmlDeclaration)xn).Encoding;
                    if (enc == null || enc == "") enc = "UTF-8";   //default encoding
                }

                XmlTextWriter w = new XmlTextWriter(fileName, Encoding.GetEncoding(enc));
                doc.Save(w);
                w.Flush();
                w.Close();

                int nrFeeds = 0;
                root = Parse(fileName, ref nrFeeds);

                /* string Name =  System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\blablabla.txt";
                 StreamWriter sw = File.CreateText(Name);
                 */
                //add(root, root, sw);
                XmlFeed[] feeds = new XmlFeed[nrFeeds];
                int i = 0;
                TreeToVector(root, root, feeds, ref i);
                //MessageBox.Show(nrFeeds.ToString());
                ((FeedDB)database).addFeeds(feeds);
                //MessageBox.Show("gata add...");
                //sw.Close(); 
            }
            else
                MessageBox.Show(s);
        }

        public string OpmlValidation(XmlDocument doc)
        {
            try
            {
                //http://hosting.opml.org/dave/spec/states.opml    
                //http://hosting.opml.org/dave/spec/subscriptionList.opml
                string fileName = Path.GetTempPath() + "\\opml.xml";
                XmlValidatingReader reader = null;
                XmlSchemaCollection myschema = new XmlSchemaCollection();
                //Create the XML fragment to be parsed.

                //Create the XmlParserContext.
                XmlParserContext context = new XmlParserContext(null, null, "", XmlSpace.None);

                //Implement the reader. 
                reader = new XmlValidatingReader(doc.OuterXml, XmlNodeType.Element, context);
                //Add the schema.
                string xsdName = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\contents.xsd";
                myschema.Add("", xsdName);

                //Set the schema type and add the schema to the reader.
                reader.ValidationType = ValidationType.Schema;
                reader.Schemas.Add(myschema);
                while (reader.Read())
                {
                }
            }
            catch (XmlSchemaException XmlSchExp)
            {
                if (XmlSchExp.Message != "The 'version' attribute has an invalid value according to its data type.")
                    return XmlSchExp.Message;
            }
            catch (XmlException XmlExp)
            {
                return "<b>Opml Error. Parsing xml failed due XML parsing error.</b> " + XmlExp.Message;
            }

            catch (Exception GenExp)
            {
                return GenExp.Message;
            }
            finally
            {

            }
            return "ok";
        }

        public void TreeToVector(TreeViewItem node, TreeViewItem parent, XmlFeed[] feeds, ref int i)
        {
            Feed f = new Feed();
            f = (Feed)node.Tag;
            if (f.IsLeaf == true)
            {
                if (f.XmlUrl == "" || f.XmlUrl == null) { }
                //MessageBox.Show("Nu am ce adresa sa adaug");
                else
                {
                    feeds[i] = new XmlFeed();
                    feeds[i].catName = (string)parent.Header;
                    feeds[i].feedName = f.ToString();
                    feeds[i].url = f.XmlUrl;
                    i++;
                }
            }
            else
            {
                foreach (TreeViewItem it in node.Items)
                {
                    TreeToVector(it, node, feeds, ref i);
                    /*
                    string Name =  System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\blablabla.txt";
                    StreamWriter sw = File.CreateText(Name);
                    sw.Write(i.Header);
                    sw.Close();*/
                }

            }

        }
            
    }
}
