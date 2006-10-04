using System;
using System.Collections.Generic;
using System.Text;
using Xml.Opml;
using System.Xml;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;
using Indexer;
namespace WinFXConsumer
{
    public class opml
    {
        public TreeViewItem Parse(string xmlfilename)
        {
            Parser p = new Parser(xmlfilename);

            string fileName = Environment.CurrentDirectory + "\\opml.htm";

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

            string fileName = Environment.CurrentDirectory + "\\opml.htm";

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
            root.Header = ("Baza de date");

            Feed f = new Feed();
            f.IsLeaf = false;
            f.Text = "Baza de date";
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
                        g.Text = feed.feedName;
                    else
                        g.Text = feed.url;
                    rssItem.Tag = g;

                    catNode.Items.Add(rssItem);
                }
                root.Items.Add(catNode);
            }
            return root;
        }

        public string makeOpml(FeedDB database)
        {
            Document doc = new Document();
            doc.Title = "Database";
            doc.AuthorName = "Feed Fusion";
            doc.AuthorEmail = "Our email is currently unavailable :D";
            doc.DateCreated = "25.07.2006";
            doc.DateModified = DateTime.Today.ToShortDateString();//;DateTime.Now.Date.ToString();

           

            TreeViewItem root = new TreeViewItem();
            root = getRootDataBase(database);

            doc.Root = doc.Now = root;

            return doc.ToOpml();
        }
            /*
            XmlTextWriter w = new XmlTextWriter(fileName, Encoding.UTF8);
            w.WriteString(doc.ToString());
            w.Close();*/

            /*  //Libraria cu VB
            StringBuilder sb = new StringBuilder();
            sb.Append(String.Format("<a href={0}>{1}({2})</a><br/>{3}<br/>{4}<br/>",doc.AuthorEmail,doc.Title,doc.AuthorName,doc.DateCreated,doc.DateModified));
            foreach (Feed f in doc.Feeds)
            {                
                sb.Append(String.Format("&#149;&nbsp;<a href={0}>{1}({2})</a><br/>", f.XmlUrl, f.Title, f.Description));
            }
            using (StreamWriter sw = File.CreateText(Environment.CurrentDirectory + "\\opml.htm"))
            {
                sw.Write(sb.ToString());
            }*/
        
        /*
        public void Load()
        {
            StringBuilder sb = new StringBuilder();
            XmlDocument doc = new XmlDocument();
            doc.Load(Validator.ParseURL("blogroll.opml"));
            MessageBox.Show("S-a dld feedul.");
            int NumToDisp = int.Parse(doc.SelectSingleNode("/opml/@numberToDisplay").InnerText);
            XmlNodeList rss = doc.SelectNodes("//outline/@xmlUrl");
            foreach (XmlNode r in rss)
            {
                XmlDocument blogdoc=new XmlDocument();
                blogdoc.Load(r.Value);
                XmlNodeList items = blogdoc.SelectNodes("//item");
                for (int i = 0; i < items.Count && i < NumToDisp; i++)
                {
                    string author = "";
                    XmlNode authorNode = items[i].SelectSingleNode("*[local-name()='author'or local-name()='creator']");
                    if (authorNode != null)
                        author = authorNode.InnerText;
                    sb.Append(String.Format("&#149;&nbsp;<a href={0}>{1}({2})</a><br/>",items[i].SelectSingleNode("link").InnerText,items[i].SelectSingleNode("title").InnerText,author));
                }

            }
            using (StreamWriter sw = File.CreateText(Environment.CurrentDirectory + "\\opml.htm"))
                {
                    sw.Write(sb.ToString());
                }
            
        }*/
    }
}
