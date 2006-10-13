using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;
using System.IO;


namespace Xml.Opml
{
    public class Parser
    {
        protected string xmlFilePath;

        /// <summary>
        /// Gets or sets the XmlFilePath.
        /// </summary>
        
        public string XmlFilePath
        {
            get
            {
                return xmlFilePath;
            }
            set
            {
                xmlFilePath = value;
            }
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="xmlFilePath"> the xml file path</param>
        public Parser(string xmlFilePath)
        {
            this.XmlFilePath=xmlFilePath;
        }

        public Document RetrieveDocument()
        {
            XmlDocument xmlDoc=new XmlDocument();
            xmlDoc.Load(this.XmlFilePath);
            XmlNode headNode=xmlDoc.SelectSingleNode("descendant::opml/head");
            XmlNode bodyNode = xmlDoc.SelectSingleNode("descendant::opml/body");
            Document d=new Document();

            string Name = Environment.CurrentDirectory + "\\temp\\blablabla.txt";
            StreamWriter sw = File.CreateText(Name);

            if (headNode!=null)
            {    
                foreach (XmlNode child in headNode.ChildNodes)
                {
                    switch (child.Name)
                    {
                        case "title":
                            d.Title = child.InnerText;
                            break;
                        case "dateCreated":
                            d.DateCreated = child.InnerText;
                            break;
                        case "dateModified":
                            d.DateModified = child.InnerText;
                            break;
                        case "ownerName":
                            d.AuthorName = child.InnerText;
                            break;
                        case "ownerEmail":
                            d.AuthorEmail = child.InnerText;
                            break;
                    }                   
                                    
                }
            }
            TreeViewItem r=new TreeViewItem();
            r.Header="Feeds";
            d.Root=d.Now=r;
            Feed f=new Feed();
            f.IsLeaf=false;
            f.Text="Feeds";
            r.Tag = f;

            int nrFeeds = 0;
            d = RetrieveFeeds(bodyNode.ChildNodes, d, ref nrFeeds,sw);
            d.NrFeeds = nrFeeds;
            /*
            sw.WriteLine("xxxxxxxx");
            sw.WriteLine("now:{0},root:{1}", d.Now.Header, d.Root.Header);*/
            sw.Close();
            
            return d;
        }

        public Document RetrieveFeeds(XmlNodeList outlineElements, Document d, ref int nrFeeds, StreamWriter sw)
        {
            TreeViewItem leaf = new TreeViewItem();
            foreach (XmlElement outline in outlineElements)
            {

                //sw.Write(outline.Name);
                //sw.Write(outline.GetAttribute("text"));
                //sw.Write(outline.GetAttribute("title"));
                //sw.Write(outline.GetAttribute("description"));
                //sw.Write(outline.GetAttribute("htmlUrl"));
                //sw.Write(outline.GetAttribute("xmlUrl"));

                //if(outline.InnerText==string.Empty)
                //  sw.Write("    da!  ");
                //if (outline.FirstChild == null)
                //  sw.Write("    yup!  ");
                //if("Feeds"==d.Now.Header)
                //  sw.Write("    yup!  ");
                //sw.WriteLine("...bla...");
                if (outline.Name == "outline")
                {
                    Feed f = new Feed();
                    if (outline.FirstChild == null)
                    {
                        /*sw.Write(outline.GetAttribute("text"));
                        sw.Write("     ");
                        sw.Write(pas);*/
                        sw.WriteLine("...is leaf...and it's parent is: {0}  ",d.Now.Header);

                        f.IsLeaf = true;
                        f.Text = outline.GetAttribute("text");
                        f.XmlUrl = outline.GetAttribute("xmlUrl");
                        sw.WriteLine("nodul ce tre adaugat {0}  ", f.XmlUrl);
                        
                        if (f.Text != null && f.Text != "" && f.XmlUrl != null && f.XmlUrl != "")
                        {
                            f.Title = outline.GetAttribute("title");
                            if (f.Title == "" || f.Title == null) f.Title = f.Text;
                            f.Description = outline.GetAttribute("description");
                            f.HtmlUrl = outline.GetAttribute("htmlUrl");
                            f.XmlUrl = outline.GetAttribute("xmlUrl");
                            sw.WriteLine("adaug {0}  ", f.Text);
                            d.addFeed(f);
                            nrFeeds++;
                        }
                    }
                    else
                    {
                        /*sw.Write(outline.GetAttribute("text"));
                        sw.Write("     ");*/
                        sw.Write("Is not leaf");
                        f.IsLeaf = false;
                        f.Text = outline.GetAttribute("text"); ;
                        TreeViewItem parent = new TreeViewItem();
                        parent = d.Now;
                        /*sw.Write(" setez parintele vechi ca fiind: {0} ", parent.Header);
                        sw.WriteLine("  ...bla...");*/
                        d.addFeed(f);
                        d = RetrieveFeeds(outline.ChildNodes, d, ref nrFeeds,sw);
                        d.Now = parent;
                        /*sw.Write(" acum resetez parintele vechi la: {0} ",parent.Header);
                        sw.WriteLine("  ... bla-revenire ...");*/
                    }
                }
            }
            return d;
        }
        
        
        //netestata...do not use!!!
        public XmlDocument CreateDocument(Document doc)
        {
            XmlDocument xmlDoc= new XmlDocument();
            xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration("1.0","utf-8","yes"));
            XmlElement opmlNode=xmlDoc.CreateElement("opml");
            XmlElement headNode=xmlDoc.CreateElement("head");
            if(doc.Title!=null)
                headNode.SetAttribute("title",doc.Title);
            if(doc.DateCreated!=null)
                headNode.SetAttribute("dateCreated",doc.DateCreated);
            if(doc.DateModified!=null)
                headNode.SetAttribute("dateModified",doc.DateModified);
            if(doc.AuthorName!=null)
                headNode.SetAttribute("ownerName",doc.AuthorName);
            if(doc.AuthorEmail!=null)
                headNode.SetAttribute("ownerEmail",doc.AuthorEmail);
            opmlNode.AppendChild(headNode);

            XmlElement bodyNode=xmlDoc.CreateElement("body");            
            bodyNode=CreateBodyElement(doc.Root,xmlDoc,bodyNode);
            opmlNode.AppendChild(bodyNode);
            xmlDoc.Save(XmlFilePath);
            return xmlDoc;
        }

        public XmlElement CreateBodyElement(TreeViewItem root, XmlDocument xmlDoc,XmlElement bodyNode)
        {
            foreach(TreeViewItem i in  root.Items)
            {
                Feed f=(Feed)i.Tag;
                XmlElement feedNode=xmlDoc.CreateElement("outline");
                feedNode.SetAttribute("text",f.Text);
                if(f.IsLeaf==true)
                {
                    feedNode.SetAttribute("title", f.Title);
                    feedNode.SetAttribute("description", f.Description);
                    feedNode.SetAttribute("xmlUrl", f.XmlUrl);
                    feedNode.SetAttribute("htmlUrl", f.HtmlUrl);                    
                }
                else
                {
                    feedNode=CreateBodyElement(i,xmlDoc,feedNode);
                }
                bodyNode.AppendChild(feedNode);
            }
            return bodyNode;
        }

        public Boolean ValidOpml()
        {
            return true;
            //validare opml:  exista fis xml+ e corect xml(fara taguri lipsa)-->throw exception daca nu e
        }

    }
}    
 