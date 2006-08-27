using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;
using System.IO;

namespace Xml.Opml
{   
    public class Document
    {
        private string title;
        private string dateCreated;
        private string dateModified;
        private string authorName;
        private string authorEmail;
        private TreeViewItem root;
        private TreeViewItem now;
        private int nrFeeds;
    
        /// <summary>
        /// Gets or sets the date the document title.
        /// </summary>

        public string Title
        {
            get
            {
                return title;
            }
            set
            {
                title = value;
            }
        }

        /// <summary>
        /// Gets or sets the date the document was created.
        /// </summary>
        
        public string DateCreated
        {
            get
            {
                return dateCreated;
            }
            set
            {
                dateCreated = value;
            }
        }

        /// <summary>
        /// Gets or sets the date the document was modified.
        /// </summary>
        
        public string DateModified
        {
            get
            {
                return dateModified;
            }
            set
            {
                dateModified = value;
            }
        }

        /// <summary>
        /// Gets or sets the author's name.
        /// </summary>
        
        public string AuthorName
        {
            get
            {
                return authorName;
            }
            set
            {
                authorName = value;
            }
        }

        /// <summary>
        /// Gets or sets the author's email.
        /// </summary>
        
        public string AuthorEmail
        {
            get
            {
                return authorEmail;
            }
            set
            {
                authorEmail = value;
            }
        }

        /// <summary>
        /// Gets or sets the root of the feed tree.
        /// </summary>
        
        public TreeViewItem Root
        {
            get
            {
                return root;
            }
            set
            {
                root = value;
            }
        }

        /// <summary>
        /// Gets or sets the last node of the feed tree.
        /// </summary>
        
        public TreeViewItem Now
        {
            get
            {
                return now;
            }
            set
            {
                now = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of feeds in the document.
        /// </summary>

        public int NrFeeds
        {
            get
            {
                return nrFeeds;
            }
            set
            {
                nrFeeds = value;
            }
        }

        public StringBuilder TreeToString(TreeViewItem node, StringBuilder sb, int pas)
        {

                Feed f = (Feed)node.Tag;
                for (int i = 0; i < pas; i++)
                    sb.Append("&nbsp;&nbsp;&nbsp;&nbsp;");


                if (f.IsLeaf == true)
                {
                    if (((f.HtmlUrl == "") && (f.XmlUrl == "")) || (f.HtmlUrl == null) || (f.XmlUrl == null))
                        sb.Append(String.Format("&#149;&nbsp;{0}({1})<br/>", f.Text, f.Description));
                    else
                        if (f.HtmlUrl == "")
                            sb.Append(String.Format("&#149;&nbsp;<a href={0}>{1}({2})</a><br/>", f.XmlUrl, f.Text, f.Description));
                        else
                            sb.Append(String.Format("&#149;&nbsp;<a href={0}>{1}({2})</a><br/>", f.HtmlUrl, f.Text, f.Description));
                }
                else
                {
                    sb.Append(String.Format("&#149;&#149;&nbsp;<a>{0} :</a><br/>", f.Text));
                    foreach (TreeViewItem i in node.Items)
                    {
                        sb = TreeToString(i, sb, pas + 1);
                    }
                }
                     
            return sb;
        }

        override public string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(String.Format("<h2 style=\"color:blue\">{0}</h2><br/><h3 style=\"color:maroon\">Author: {1}</h3><br/><h3 style=\"color:maroon\">{2}</h3><br/>Created: {3}<br/>Modified: {4}<br/></br></br>", Title, AuthorName, AuthorEmail, DateCreated, DateModified));
            /*
            string Name = Environment.CurrentDirectory + "\\blablabla.txt";
            StreamWriter sw = File.CreateText(Name);
            */
            StringBuilder sb2 = new StringBuilder();

            //foreach (TreeViewItem i in Root.Items)
            sb.Append(TreeToString(Root, sb2, 0).ToString());


            //sw.Write(String.Format("<a href={0}>{1}({2})</a><br/>{3}<br/>{4}<br/>", AuthorEmail, Title, AuthorName, DateCreated, DateModified));

            //sw.Close();
            //return "xxx";
            return sb.ToString();


        }

        public StringBuilder TreeToOpml(TreeViewItem node, StringBuilder sb)
        {
            if ((string)node.Header != "Baza de date")
            {
                Feed f = (Feed)node.Tag;

                if (f.IsLeaf == true)
                {
                    sb.Append(String.Format("<outline text=\"{0}\" ", f.Text));
                    if ((f.Title != "") && (f.Title != null))
                        sb.Append(String.Format("title=\"{0}\" ", f.Title));
                    if ((f.XmlUrl != "") && (f.XmlUrl != null))
                        sb.Append(String.Format("xmlUrl=\"{0}\" ", f.XmlUrl));
                    if ((f.HtmlUrl != "") && (f.HtmlUrl != null))
                        sb.Append(String.Format("htmlUrl=\"{0}\" ", f.HtmlUrl));
                    if ((f.Description != "") && (f.Description != null))
                        sb.Append(String.Format("description=\"{0}\" ", f.Description));
                    sb.Append("/>");
                    sb.Append(Environment.NewLine);
                }
                else
                {
                    sb.Append(String.Format("<outline text=\"{0}\">", f.Text));
                    sb.Append(Environment.NewLine);
                    foreach (TreeViewItem i in node.Items)
                    {
                        sb = TreeToOpml(i, sb);
                    }
                    sb.Append("</outline>");
                    sb.Append(Environment.NewLine);
                }
            }

            else
            {
                foreach (TreeViewItem i in node.Items)
                {
                    sb = TreeToOpml(i, sb);
                }
            }
            return sb;
        }

        public string ToOpml()
        {
            StringBuilder sb = new StringBuilder();
            //sb.Append(" <?xml version=\"1.0\" encoding=\"utf-8\" ?>");
            sb.Append("<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>");
            sb.Append(Environment.NewLine);
            sb.Append("<opml version=\"2.0\">");
            sb.Append(Environment.NewLine);            
            sb.Append("<head>");
            sb.Append(Environment.NewLine);            
            sb.Append(String.Format("<title>{0}</title>", Title));
            sb.Append(Environment.NewLine);
            sb.Append(String.Format("<dateCreated>{0}</dateCreated>", DateCreated));
            sb.Append(Environment.NewLine);
            sb.Append(String.Format("<dateModified>{0}</dateModified>", DateModified));
            sb.Append(Environment.NewLine);
            sb.Append(String.Format("<ownerName>{0}</ownerName>",AuthorName));
            sb.Append(Environment.NewLine);
            sb.Append(String.Format("<ownerEmail>{0}</ownerEmail>",AuthorEmail));
            sb.Append(Environment.NewLine);
            sb.Append("<expansionState></expansionState>");
            sb.Append(Environment.NewLine); 
            sb.Append("<vertScrollState>1</vertScrollState>");
            sb.Append(Environment.NewLine); 
            sb.Append("<windowTop>20</windowTop>");
            sb.Append(Environment.NewLine); 
            sb.Append("<windowLeft>0</windowLeft>");
            sb.Append(Environment.NewLine);
            sb.Append("<windowBottom>120</windowBottom>"); 
            sb.Append(Environment.NewLine); 
            sb.Append("<windowRight>147</windowRight>");
            sb.Append(Environment.NewLine); 
            sb.Append("</head>");
            sb.Append(Environment.NewLine);
            sb.Append("<body>");
            sb.Append(Environment.NewLine); 
            /*
            string Name = Environment.CurrentDirectory + "\\blablabla.txt";
            StreamWriter sw = File.CreateText(Name);
            */
            StringBuilder sb2 = new StringBuilder();

            //foreach (TreeViewItem i in Root.Items)
            sb.Append(TreeToOpml(Root, sb2).ToString());

            sb.Append("</body>");
            sb.Append(Environment.NewLine);
            sb.Append("</opml>");
            //sw.Write(String.Format("<a href={0}>{1}({2})</a><br/>{3}<br/>{4}<br/>", AuthorEmail, Title, AuthorName, DateCreated, DateModified));

            //sw.Close();
            //return "xxx";
            return sb.ToString();
        }
        

        /// <summary>
        /// Adds a feed to the TreeView
        /// </summary>
        /// <param name="f"> The feed will be found in the TreeViewItem's Tag(which is Object)</param>
        /// <returns></returns>
        public Boolean addFeed(Feed f)
        {   
            if(f.Text!="")
            {
                if(f.IsLeaf == true)
                {
                    TreeViewItem leaf= new TreeViewItem();
                    leaf.Header=f.Text;                    
                    leaf.Tag=f;
                    if(Root == Now)
                    {
                        Root.Items.Add(leaf);
                        Now = Root;
                    }
                    else
                    {
                        Now.Items.Add(leaf);
                    }
                }
                else
                {
                    TreeViewItem node= new TreeViewItem();
                    node.Header=f.Text;
                    node.Tag=f;
                    if(Root == Now)
                    {
                        Root.Items.Add(node);
                        Now=node;
                    }
                    else
                    {
                        Now.Items.Add(node);
                        Now=node;
                    }
                }
                return true;
            }
            return false;
        }
    }
}