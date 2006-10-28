using System;
using System.Collections.Generic;
using System.Text;

namespace Xml.Opml
{
    public class Feed
    {
        private string text;
        private string title;
        private string description;
        private string xmlUrl;
        private string htmlUrl;
        private Boolean isleaf;

        /// <summary>
        /// Gets or sets the feed title.
        /// </summary>

        public string Text
        {
            get
            {
                return text;
            }
            set
            {
                text = value;
            }
        }

        /// <summary>
        /// Gets or sets the feed title.
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
        /// Gets or sets the feed description.
        /// </summary>
        public string Description
        {
            get
            {
                return description;
            }
            set
            {
                description = value;
            }
        }

        /// <summary>
        /// Gets or sets the feed xmlUrl.
        /// </summary>
        public string XmlUrl
        {
            get
            {
                return xmlUrl;
            }
            set
            {
                xmlUrl = value;
            }
        }

        /// <summary>
        /// Gets or sets the feed htmlUrl.
        /// </summary>
        public string HtmlUrl
        {
            get
            {
                return htmlUrl;
            }
            set
            {
                htmlUrl = value;
            }
        }

        /// <summary>
        /// Gets or sets whether the feed is a leaf or not
        /// </summary>

        public Boolean IsLeaf
        {
            get
            {
                return isleaf;
            }
            set
            {
                isleaf= value;
            }
        }

        override public String ToString()
        {
            return Title + " ( " + Description + " )";
        }
    }
}
