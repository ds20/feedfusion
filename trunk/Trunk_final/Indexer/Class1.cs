using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Specialized;
using System.Threading;

using Lucene.Net.Index;
using Lucene.Net.Documents;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Search;
using Lucene.Net.QueryParsers;



namespace Indexer
{
    public class XmlFeed:IComparable
    {
        public String catName, url, feedName;
        public override string ToString()
        {
            if (feedName == null || feedName == "")
                return url;
            return feedName;
        }

        #region IComparable Members

        int IComparable.CompareTo(object obj)
        {
            return feedName.CompareTo(((XmlFeed)obj).feedName);
        }

        #endregion
    }

    public class XmlHistory
    {
        public String feedUrl, contents;
        public DateTime dateTime;
        public override String ToString()
        {
            return ToString(50);
        }
        public String ToString(int maxLength)
        {
            /*if (contents.Length < maxLength) maxLength = contents.Length;
            return dateTime.ToString() + " - " + contents.Substring(0, maxLength);*/

            return dateTime.ToString();
        }
    }

    public class XmlCategory
    {
        public String name;
        public int index;
    }

    public class FeedDB:PluginInterface.DataBaseEngine 
    {
        private Object historyLock = new Object(), feedLock = new Object(), catLock = new Object();
        private List<PluginInterface.EventsClass> EventsVector=new List<PluginInterface.EventsClass>();  
        String dirCat, dirFeed, dirHistory;
        IndexWriter writerCat, writerFeed, writerHistory;
        IndexSearcher searcherCat, searcherFeed, searcherHistory;
        public delegate void DelegateCatFeedChanged();
        public DelegateCatFeedChanged delegCatFeedChanged;
        public delegate void DelegateHistoryChanged();
        public DelegateHistoryChanged delegHistoryChanged;

        /// <summary>
        /// Uses standard directories
        /// </summary>
        public FeedDB()
            : this(Environment.CurrentDirectory + "\\data\\cat", Environment.CurrentDirectory + "\\data\\feed", Environment.CurrentDirectory + "\\data\\history")
        {
        }

        /// <summary>
        /// Constructor with specified (custom) directories
        /// </summary>
        /// <param name="dirCat">Directory for categories 'database'</param>
        /// <param name="dirFeed">Directory for feed 'database'</param>
        /// <param name="dirHistory">Directory for history 'database'</param>
        public FeedDB(String dirCat, String dirFeed, String dirHistory)
        {
            this.dirCat = dirCat;
            this.dirFeed = dirFeed;
            this.dirHistory = dirHistory;
            try { writerCat = new IndexWriter(dirCat, new StandardAnalyzer(), false); }
            catch (Exception)
            {  //create 'category database'
                writerCat = new IndexWriter(dirCat, new StandardAnalyzer(), true);
                Document doc = new Document();
                doc.Add(Field.Text("catIndex", "" + 1));
                doc.Add(Field.Text("catName", "nextIndex"));
                writerCat.AddDocument(doc);
                saveCat();
            }
            try { writerFeed = new IndexWriter(dirFeed, new StandardAnalyzer(), false); }
            catch (Exception)
            {   //create new 'feed database'
                writerFeed = new IndexWriter(dirFeed, new StandardAnalyzer(), true);
                Document doc = new Document();
                doc.Add(Field.Text("feedIndex", "" + 1));
                doc.Add(Field.Text("feedName", "nextIndex"));
                writerFeed.AddDocument(doc);
                saveFeed();
            }

            try { writerHistory = new IndexWriter(dirHistory, new StandardAnalyzer(), false); }
            catch (Exception)
            {   //create new 'history database'
                writerHistory = new IndexWriter(dirHistory, new StandardAnalyzer(), true);
            }

            searcherCat = new IndexSearcher(dirCat);
            searcherFeed = new IndexSearcher(dirFeed);
            searcherHistory = new IndexSearcher(dirHistory);
            //maybe we should verify that they have been properly created
        }

        /// <summary>
        /// Saves to disc the changes made to categories.
        /// </summary>
        private void saveCat()
        {
            writerCat.Close();  //to save the data
            writerCat = new IndexWriter(dirCat, new StandardAnalyzer(), false); //reopen
            if (searcherCat != null) searcherCat.Close();
            searcherCat = new IndexSearcher(dirCat);
            foreach (PluginInterface.EventsClass ev in EventsVector)
            {
                ev.CategoryAdded("11"); 
            }
            //if (delegCatFeedChanged != null) delegCatFeedChanged();
        }

        /// <summary>
        /// Saves to disc the changes made to feeds.
        /// </summary>
        private void saveFeed()
        {
            writerFeed.Close();  //to save the data
            writerFeed = new IndexWriter(dirFeed, new StandardAnalyzer(), false); //reopen
            if (searcherFeed != null) searcherFeed.Close();
            searcherFeed = new IndexSearcher(dirFeed);
            //if (delegCatFeedChanged != null) delegCatFeedChanged();
            foreach (PluginInterface.EventsClass ev in EventsVector)
            {
                ev.NewFeedAdded("22");
            }
        }

        /// <summary>
        /// Saves to disc the changes made to history
        /// </summary>
        private void saveHistory()
        {
            lock (historyLock)
            {
                bool originalBackgroundState = Thread.CurrentThread.IsBackground;
                Thread.CurrentThread.IsBackground = false;
                
                writerHistory.Close();  //to save the data
                writerHistory = new IndexWriter(dirHistory, new StandardAnalyzer(), false); //reopen
                if (searcherHistory != null) searcherHistory.Close();
                searcherHistory = new IndexSearcher(dirHistory);
                //if (delegHistoryChanged != null) delegHistoryChanged();
                
                Thread.CurrentThread.IsBackground = originalBackgroundState;

            }
        }

        public void RegisterEventHandler(PluginInterface.EventsClass ev)
        { 
            EventsVector.Add(ev);  
        }
        /// <summary>
        /// Checks if the specified category exists in the database.
        /// </summary>
        /// <param name="catName">The name of the category</param>
        /// <returns>True if the category exists in the database. False otherwise.</returns>
        public bool categoryExists(String catName)
        {
            String q = "z" + catName;
            Query query = QueryParser.Parse(QueryParser.Escape(q), "catName", new StandardAnalyzer());
            Hits hits;
            lock (catLock)
            {
                hits = searcherCat.Search(query);
            }
            for (int i = 0; i < hits.Length(); i++)
            {
                Document doc = hits.Doc(i);
                if (doc.Get("catName").CompareTo(q) == 0)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Returns the index of the specified category
        /// </summary>
        /// <param name="catName">The category name</param>
        /// <returns>The index of the category (positive integer), or -1 if it does not exist</returns>
        private int getCategoryIndex(String catName)
        {
            String q = "z" + catName;
            Query query = QueryParser.Parse(QueryParser.Escape(q), "catName", new StandardAnalyzer());
            Hits hits;
            lock (catLock)
            {
                hits = searcherCat.Search(query);
            }
            for (int i = 0; i < hits.Length(); i++)
            {
                Document doc = hits.Doc(i);
                if (doc.Get("catName").CompareTo(q) == 0)
                    return Int32.Parse(doc.Get("catIndex"));
            }
            return -1;
        }

        /// <summary>
        /// Returns the index for the next category
        /// </summary>
        /// <returns>The index for the next category</returns>
        private int getNextCategoryIndex()
        {
            
            int nextIndex = 1;  //the default if there are no categories in the database
            String q = "nextIndex";
            Query query = QueryParser.Parse(q, "catName", new StandardAnalyzer());
            Hits hits;
            lock (catLock)
            {
                hits = searcherCat.Search(query);
            }
            for (int i = 0; i < hits.Length(); i++)
                if (hits.Doc(i).Get("catName").CompareTo("nextIndex") == 0)
                {
                    int crt = Int32.Parse(hits.Doc(i).Get("catIndex")); //in case there are more
                    if (crt > nextIndex)
                        nextIndex = crt;
                }
            return nextIndex;
        }

        /// <summary>
        /// Increments the entry showing the index for the next category
        /// </summary>
        private void incrementNextCategoryIndex()
        {
            int newNextIndex, nextIndex = -1;
            String q = "nextIndex";
            Query query = QueryParser.Parse(q, "catName", new StandardAnalyzer());
            Hits hits;
            lock (catLock)
            {
                hits = searcherCat.Search(query);
            }

            IndexReader ir = IndexReader.Open(dirCat);
            for (int i = 0; i < hits.Length(); i++)
                if (hits.Doc(i).Get("catName").CompareTo("nextIndex") == 0)
                {
                    int crt = Int32.Parse(hits.Doc(i).Get("catIndex"));
                    if (crt > nextIndex)
                        nextIndex = crt;
                    ir.Delete(hits.Id(i));  //delete the row(s) "nextIndex" - should be just one element in "hits"
                }
            lock (catLock)
            {
                ir.Close();
            }
            newNextIndex = (nextIndex == -1) ? 2 : nextIndex + 1;

            Document doc = new Document();
            doc.Add(Field.Text("catIndex", "" + newNextIndex));
            doc.Add(Field.Text("catName", "nextIndex"));

            lock (catLock)
            {
                writerCat.AddDocument(doc);
                saveCat();
            }
        }

        /// <summary>
        /// Adds a new category. If it already exists it is not added to the database.
        /// </summary>
        /// <param name="catName">The unique name of the new category</param>
        /// <returns>True if the category did not exist (and creates it). False otherwise</returns>
        public bool addCategory(String catName)
        {
            if (!categoryExists(catName))
            {
                int nextIndex = getNextCategoryIndex(); //the index for the new category we want to create

                incrementNextCategoryIndex();

                Document doc = new Document();
                doc.Add(Field.Text("catIndex", "" + nextIndex));
                doc.Add(Field.Text("catName", "z" + catName));
                lock (catLock)
                {
                    writerCat.AddDocument(doc);
                    saveCat();
                }
                if (delegCatFeedChanged != null) delegCatFeedChanged();
                return true;
            }
            return false;   //category already existed - we don't re-add it
        }

        private List<XmlCategory> addCategories(String[] catNames)
        {
            
            List<XmlCategory> existingCategories = new List<XmlCategory>(), newCategories = new List<XmlCategory>();
            List<XmlCategory> categories = new List<XmlCategory>();
            StringCollection existingCategoryNames = new StringCollection(), newCategoryNames = new StringCollection();

            
            //get all existing categories
            String q1 = "z*";
            Query query1 = QueryParser.Parse(q1, "catName", new StandardAnalyzer());
            Hits hits1;
            lock (catLock)
            {
                hits1 = searcherCat.Search(query1);
            }
            for (int i = 0; i < hits1.Length(); i++)
            {
                XmlCategory cat = new XmlCategory();
                cat.name = hits1.Doc(i).Get("catName").Substring(1); //to return without the leading "z"
                cat.index = Int32.Parse(hits1.Doc(i).Get("catIndex"));
                existingCategories.Add(cat);
                existingCategoryNames.Add(cat.name);
            }
            //end - get all existing categories

            for (int i = 0; i < catNames.Length; i++)
                if (!existingCategoryNames.Contains(catNames[i]))
                    newCategoryNames.Add(catNames[i]);

            int nextIndex = -1, newNextIndex;


            //find and delete the nextIndex entry in Categories
            String q = "nextIndex";
            Query query = QueryParser.Parse(q, "catName", new StandardAnalyzer());
            Hits hits;
            lock (catLock)
            {
                hits = searcherCat.Search(query);
            }

            IndexReader ir = IndexReader.Open(dirCat);
            for (int i = 0; i < hits.Length(); i++)
                if (hits.Doc(i).Get("catName").CompareTo("nextIndex") == 0)
                {
                    int crt = Int32.Parse(hits.Doc(i).Get("catIndex"));
                    if (crt > nextIndex)
                        nextIndex = crt;
                    ir.Delete(hits.Id(i));  //delete the row(s) "nextIndex" - should be just one element in "hits"
                }

            lock (catLock)
            {
                ir.Close();
            }
            newNextIndex = (nextIndex == -1) ? newCategoryNames.Count + 1 : nextIndex + newCategoryNames.Count;
            //end - find and delete the next entry in Categories




            int crtCatIndex = nextIndex == -1 ? 1 : nextIndex;

            for (int i = 0; i < newCategoryNames.Count; i++)
            {
                XmlCategory cat = new XmlCategory();
                cat.name = newCategoryNames[i];
                cat.index = crtCatIndex;
                newCategories.Add(cat);

                Document doc = new Document();
                doc.Add(Field.Text("catIndex", "" + crtCatIndex++));
                doc.Add(Field.Text("catName", "z" + newCategoryNames[i]));

                lock (catLock)
                {
                    writerCat.AddDocument(doc);
                }
            }

            Document nextIndexDoc = new Document();
            nextIndexDoc.Add(Field.Text("catIndex", "" + newNextIndex));
            nextIndexDoc.Add(Field.Text("catName", "nextIndex"));
            lock (catLock)
            {
                writerCat.AddDocument(nextIndexDoc);
                saveCat();
            }
            //the delegate will be called in addFeeds() function

            categories.AddRange(existingCategories);
            categories.AddRange(newCategories);
            return categories;
        }

        public void addFeeds(XmlFeed[] feeds)
        {
            List<XmlFeed> existingFeeds = new List<XmlFeed>(getAllFeeds()), newFeeds = new List<XmlFeed>();
            StringCollection existingURLs = new StringCollection(), categoryNames = new StringCollection();
            List<XmlCategory> categories = new List<XmlCategory>();
            List<String> catNames = new List<string>();
            List<int> catIndexes = new List<int>();

            foreach (XmlFeed f in existingFeeds)
                existingURLs.Add(f.url);

            foreach (XmlFeed f in feeds)
                if (!existingURLs.Contains(f.url))
                    newFeeds.Add(f);

            foreach (XmlFeed f in newFeeds)
                if (!categoryNames.Contains(f.catName))
                    categoryNames.Add(f.catName);

            String[] parametru = new String[categoryNames.Count];
            categoryNames.CopyTo(parametru, 0);
            categories = addCategories(parametru);
            foreach (XmlCategory cat in categories)
            {
                catNames.Add(cat.name);
                catIndexes.Add(cat.index);
            }


            //delete nextFeed entry
            int newNextIndex, nextIndex = -1;
            String q = "nextIndex";
            Query query = QueryParser.Parse(q, "feedName", new StandardAnalyzer());
            Hits hits;
            lock (feedLock)
            {
                hits = searcherFeed.Search(query);
            }

            IndexReader ir = IndexReader.Open(dirFeed);
            for (int i = 0; i < hits.Length(); i++)
                if (hits.Doc(i).Get("feedName").CompareTo("nextIndex") == 0)
                {
                    int crt = Int32.Parse(hits.Doc(i).Get("feedIndex"));
                    if (crt > nextIndex)
                        nextIndex = crt;
                    ir.Delete(hits.Id(i));  //delete the row(s) "nextIndex" - should be just one element in "hits"
                }
            lock (feedLock)
            {
                ir.Close();
            }
            newNextIndex = (nextIndex == -1) ? newFeeds.Count + 1 : nextIndex + newFeeds.Count;
            int crtFeedIndex = nextIndex == -1 ? 1 : nextIndex;

            //end - delete nextFeed entry


            foreach (XmlFeed f in newFeeds)
            {
                int poz = catNames.IndexOf(f.catName);
                int catIndex = catIndexes[poz];

                Document doc = new Document();
                doc.Add(Field.Text("feedIndex", "" + crtFeedIndex++));
                doc.Add(Field.Text("catIndex", "" + catIndex));
                doc.Add(Field.Text("url", "z" + f.url));
                doc.Add(Field.Text("feedName", "z" + f.feedName));
                writerFeed.AddDocument(doc);
            }

            //add new nextFeed entry
            Document doc1 = new Document();
            doc1.Add(Field.Text("feedIndex", "" + newNextIndex));
            doc1.Add(Field.Text("feedName", "nextIndex"));
            lock (feedLock)
            {
                writerFeed.AddDocument(doc1);
                //end - add new nextFeed entry
                saveFeed();
            }
            if (delegCatFeedChanged != null) delegCatFeedChanged();
        }


        /// <summary>
        /// Returns the name of the category with the specified index
        /// </summary>
        /// <param name="catIndex">The index of the category</param>
        /// <returns>The name of the category or null if it does not exist</returns>
        private String getCategoryName(int catIndex)
        {
            String q = "" + catIndex;
            Query query = QueryParser.Parse(q, "catIndex", new StandardAnalyzer());
            Hits hits;
            lock (catLock)
            {
                hits = searcherCat.Search(query);
            }
            for (int i = 0; i < hits.Length(); i++)
            {
                Document doc = hits.Doc(i);
                if (doc.Get("catIndex").CompareTo(q) == 0)
                    return doc.Get("catName").Substring(1);
            }
            return null;
        }

        /// <summary>
        /// Returns all the categories in the database
        /// </summary>
        /// <returns>All categories</returns>
        public String[] getCategories()
        {
            String q = "z*";
            Query query = QueryParser.Parse(q, "catName", new StandardAnalyzer());
            Hits hits;
            lock (catLock)
            {
                hits = searcherCat.Search(query);
            }
            String[] results = new String[hits.Length()];
            for (int i = 0; i < hits.Length(); i++)
                results[i] = hits.Doc(i).Get("catName").Substring(1); //to return without the leading "z"
            Array.Sort(results);
            return results;
        }

        /// <summary>
        /// Returns the index for the next feed
        /// </summary>
        /// <returns>The index for the next feed</returns>
        private int getNextFeedIndex()
        {
            int nextIndex = 1;  //the default if there are no feeds in the database
            String q = "nextIndex";
            Query query = QueryParser.Parse(q, "feedName", new StandardAnalyzer());
            Hits hits;
            lock (feedLock)
            {
                hits = searcherFeed.Search(query);
            }
            for (int i = 0; i < hits.Length(); i++)
                if (hits.Doc(i).Get("feedName").CompareTo("nextIndex") == 0)
                {
                    int crt = Int32.Parse(hits.Doc(i).Get("feedIndex")); //in case there are more
                    if (crt > nextIndex)
                        nextIndex = crt;
                }
            return nextIndex;
        }

        /// <summary>
        /// Increments the entry showing the index for the next feed
        /// </summary>
        private void incrementNextFeedIndex()
        {
            int newNextIndex, nextIndex = -1;
            String q = "nextIndex";
            Query query = QueryParser.Parse(q, "feedName", new StandardAnalyzer());
            Hits hits;
            lock (feedLock)
            {
                hits = searcherFeed.Search(query);
            }

            IndexReader ir = IndexReader.Open(dirFeed);
            for (int i = 0; i < hits.Length(); i++)
                if (hits.Doc(i).Get("feedName").CompareTo("nextIndex") == 0)
                {
                    int crt = Int32.Parse(hits.Doc(i).Get("feedIndex"));
                    if (crt > nextIndex)
                        nextIndex = crt;
                    ir.Delete(hits.Id(i));  //delete the row(s) "nextIndex" - should be just one element in "hits"
                }

            lock (feedLock)
            {
                ir.Close();
            }
            newNextIndex = (nextIndex == -1) ? 2 : nextIndex + 1;

            Document doc = new Document();
            doc.Add(Field.Text("feedIndex", "" + newNextIndex));
            doc.Add(Field.Text("feedName", "nextIndex"));
            lock (feedLock)
            {
                writerFeed.AddDocument(doc);
                saveFeed();
            }
        }

        /// <summary>
        /// Returns the index for the next history entry
        /// </summary>
        /// <returns>The index for the next history entry</returns>
        private int getNextHistoryIndex()
        {
            int nextIndex = 1;  //the default if there are no history entries in the database
            String q = "nextIndex";
            Query query = QueryParser.Parse(q, "contents", new StandardAnalyzer());
            Hits hits;
            lock (historyLock)
            {
                hits = searcherHistory.Search(query);
            }
            for (int i = 0; i < hits.Length(); i++)
                if (hits.Doc(i).Get("contents").CompareTo("nextIndex") == 0)
                {
                    int crt = Int32.Parse(hits.Doc(i).Get("historyIndex")); //in case there are more
                    if (crt > nextIndex)
                        nextIndex = crt;
                }
            return nextIndex;
        }

        /// <summary>
        /// Increments the entry showing the index for the next history entry
        /// </summary>
        private void incrementNextHistoryIndex()
        {
            int newNextIndex, nextIndex = -1;
            String q = "nextIndex";
            Query query = QueryParser.Parse(q, "contentsName", new StandardAnalyzer());
            Hits hits;
            lock (historyLock)
            {
                hits = searcherHistory.Search(query);
            }

            IndexReader ir = IndexReader.Open(dirHistory);
            for (int i = 0; i < hits.Length(); i++)
                if (hits.Doc(i).Get("contents").CompareTo("nextIndex") == 0)
                {
                    int crt = Int32.Parse(hits.Doc(i).Get("historyIndex"));
                    if (crt > nextIndex)
                        nextIndex = crt;
                    ir.Delete(hits.Id(i));  //delete the row(s) "nextIndex" - should be just one element in "hits"
                }
            lock (historyLock)
            {
                ir.Close();
            }
            newNextIndex = (nextIndex == -1) ? 2 : nextIndex + 1;

            Document doc = new Document();
            doc.Add(Field.Text("historyIndex", "" + newNextIndex));
            doc.Add(Field.Text("contents", "nextIndex"));
            lock (historyLock)
            {
                writerHistory.AddDocument(doc);
                saveHistory();
            }
        }

        /// <summary>
        /// Checks if a feed is already in the database
        /// </summary>
        /// <param name="url">The URL of the feed to check</param>
        /// <returns>True if a feed with the specified URL exists. False otherwise.</returns>
        public bool feedExists(String url)
        {
            String q = "z" + url;
            Query query = QueryParser.Parse(QueryParser.Escape(q), "url", new StandardAnalyzer());
            Hits hits;
            lock (feedLock)
            {
                hits = searcherFeed.Search(query);
            }
            for (int i = 0; i < hits.Length(); i++)
            {
                Document doc = hits.Doc(i);
                if (doc.Get("url").CompareTo(q) == 0)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Adds a feed to the database.
        /// </summary>
        /// <param name="catName">The category to which the feed belongs to. The category must already exist in the database</param>
        /// <param name="url">The unique URL of the feed. Two feeds in the database can not have the same URL.</param>
        /// <param name="feedName">A custom name chosen for the feed. It need not be unique.</param>
        /// <returns>True if the feed was added. False otherwise.</returns>
        public bool addFeed(String catName, String url, String feedName)
        {
            if (!categoryExists(catName))
                return false;   //category does not exist
            if (feedExists(url))
                return false;   //feed already in the database

            int catIndex = getCategoryIndex(catName);   //this could replace the first check if categoryExists
            int nextIndex = getNextFeedIndex(); //the index for the new feed we want to create
            incrementNextFeedIndex();

            Document doc = new Document();
            doc.Add(Field.Text("feedIndex", "" + nextIndex));
            doc.Add(Field.Text("catIndex", "" + catIndex));
            doc.Add(Field.Text("url", "z" + url));
            doc.Add(Field.Text("feedName", "z" + feedName));
            lock (feedLock)
            {
                writerFeed.AddDocument(doc);
                saveFeed();
            }
            if (delegCatFeedChanged != null) delegCatFeedChanged();
            return true;
        }

        /// <summary>
        /// Adds a feed to the database.
        /// </summary>
        /// <param name="catName">The category to which the feed belongs to. The category must already exist in the database</param>
        /// <param name="url">The unique URL of the feed. Two feeds in the database can not have the same URL.</param>
        /// <returns>True if the feed was added. False otherwise.</returns>
        public bool addFeed(String catName, String url)
        {
            return addFeed(catName, url, null);
        }

        /// <summary>
        /// Returns the index of the feed with the specified url
        /// </summary>
        /// <param name="url">The url of the feed you are searching for</param>
        /// <returns>The index of the feed (positive integer) or -1 if it was not found</returns>
        private int getFeedIndex(String url)
        {
            String q = "z" + url;
            Query query = QueryParser.Parse(QueryParser.Escape(q), "url", new StandardAnalyzer());
            Hits hits;
            lock (feedLock)
            {
                hits = searcherFeed.Search(query);
            }
            for (int i = 0; i < hits.Length(); i++)
            {
                Document doc = hits.Doc(i);
                if (doc.Get("url").CompareTo(q) == 0)
                    return Int32.Parse(doc.Get("feedIndex"));
            }
            return -1;
        }

        /// <summary>
        /// Returns the feed with the index given as parameter
        /// </summary>
        /// <param name="feedIndex">The index of the feed</param>
        /// <returns>The feed if it exists or null otherwise</returns>
        private XmlFeed getFeed(int feedIndex)
        {
            XmlFeed result;
            String q = "" + feedIndex;
            Query query = QueryParser.Parse(q, "feedIndex", new StandardAnalyzer());
            Hits hits;
            lock (feedLock)
            {
                hits = searcherFeed.Search(query);
            }
            for (int i = 0; i < hits.Length(); i++)
                if (hits.Doc(i).Get("feedIndex").CompareTo("" + feedIndex) == 0)
                {
                    Document doc = hits.Doc(i);
                    result = new XmlFeed();
                    result.catName = getCategoryName(Int32.Parse(doc.Get("catIndex")));
                    result.feedName = doc.Get("feedName").Substring(1);
                    result.url = doc.Get("url").Substring(1);
                    return result;
                }
            return null;
        }

        /// <summary>
        /// Returns the feed with the URL given as parameter
        /// </summary>
        /// <param name="url">The URL of the feed</param>
        /// <returns>The feed if it exists or null otherwise</returns>
        public XmlFeed getFeed(String url)
        {
            return getFeed(getFeedIndex(url));
        }

        /// <summary>
        /// Returns all the feeds from the given category
        /// </summary>
        /// <param name="catIndex">The index of the category</param>
        /// <returns>A vector with all the feeds in the category</returns>
        private XmlFeed[] getFeeds(int catIndex)
        {
            String q = "" + catIndex;
            Query query = QueryParser.Parse(q, "catIndex", new StandardAnalyzer());
            Hits hits;
            lock (feedLock)
            {
                hits = searcherFeed.Search(query);
            }
            XmlFeed[] results = new XmlFeed[hits.Length()];
            for (int i = 0; i < hits.Length(); i++)
            {
                Document doc = hits.Doc(i);
                results[i] = new XmlFeed();
                results[i].catName = getCategoryName(catIndex);
                results[i].feedName = doc.Get("feedName").Substring(1);
                results[i].url = doc.Get("url").Substring(1);
            }
            Array.Sort(results);
            return results;
        }

        /// <summary>
        /// Returns all the feeds from the given category
        /// </summary>
        /// <param name="catName">The name of the category</param>
        /// <returns>A vector with all the feeds in the category</returns>
        public XmlFeed[] getFeeds(String catName)
        {
            return getFeeds(getCategoryIndex(catName));
        }

        /// <summary>
        /// Gets all feeds from the database
        /// </summary>
        /// <returns>A vector containing all the feeds in the database</returns>
        public XmlFeed[] getAllFeeds()
        {
            String q = "z*";
            Query query = QueryParser.Parse(q, "url", new StandardAnalyzer());
            Hits hits;
            lock (feedLock)
            {
                hits = searcherFeed.Search(query);
            }
            XmlFeed[] results = new XmlFeed[hits.Length()];
            for (int i = 0; i < hits.Length(); i++)
            {
                Document doc = hits.Doc(i);
                results[i] = new XmlFeed();
                results[i].catName = getCategoryName(Int32.Parse(doc.Get("catIndex")));
                results[i].feedName = doc.Get("feedName").Substring(1);
                results[i].url = doc.Get("url").Substring(1);
            }
            Array.Sort(results);
            return results;
        }

        /// <summary>
        /// Gets all the history information from the database
        /// </summary>
        /// <returns>A vector containing all the history in the database</returns>
        public XmlHistory[] getAllHistory()
        {
            String q = "z*";
            Query query = QueryParser.Parse(q, "contents", new StandardAnalyzer());
            Hits hits;
            lock (historyLock)
            {
                hits = searcherHistory.Search(query);
            }
            XmlHistory[] results = new XmlHistory[hits.Length()];
            for (int i = 0; i < hits.Length(); i++)
            {
                Document doc = hits.Doc(i);
                results[i] = new XmlHistory();
                results[i].contents = doc.Get("contents").Substring(1);
                results[i].feedUrl = getFeed(Int32.Parse(doc.Get("feedIndex"))).url;
                results[i].dateTime = DateField.StringToDate(doc.Get("dateTime"));
            }
            return results;
        }

        /// <summary>
        /// Adds the current information from the feed to the database
        /// </summary>
        /// <param name="url">The URL of the feed. There must exist a feed with this URL</param>
        /// <param name="xmlContents">The current information contained in the feed</param>
        /// <returns>True if the feed exists. If the feed does not exist the information is not added and the return value is false</returns>
        public bool addHistory(String url, String xmlContents, DateTime dateTime)
        {

            if (!feedExists(url))
                return false;   //no such feed
            int nextIndex = getNextHistoryIndex(); //the index for the new history we want to create
            incrementNextHistoryIndex();

            Document doc = new Document();
            doc.Add(Field.Text("historyIndex", "" + nextIndex));
            doc.Add(Field.Text("feedIndex", "" + getFeedIndex(url)));
            doc.Add(Field.Text("contents", "z" + xmlContents));
            doc.Add(Field.Keyword("dateTime", dateTime));

            bool originalBackgroundState = Thread.CurrentThread.IsBackground;
            Thread.CurrentThread.IsBackground = false;

            lock (historyLock)
            {
                writerHistory.AddDocument(doc);
                saveHistory();
            }

            Thread.CurrentThread.IsBackground = originalBackgroundState;

            if (delegHistoryChanged != null) delegHistoryChanged();

            foreach (PluginInterface.EventsClass ev in EventsVector)
            {
                ev.FeedDownloaded(url);
            }
            return true;
        }

        /// <summary>
        /// Returns all the history of a feed
        /// </summary>
        /// <param name="feedIndex">The index of the feed</param>
        /// <returns>A vector containing the history of the feed</returns>
        private XmlHistory[] getHistory(int feedIndex)
        {
            String q = "" + feedIndex;
            Query query = QueryParser.Parse(q, "feedIndex", new StandardAnalyzer());
            Hits hits;
            XmlHistory[] results;
            lock (historyLock)
            {
                hits = searcherHistory.Search(query);

                results = new XmlHistory[hits.Length()];
                for (int i = 0; i < hits.Length(); i++)
                {
                    int j = results.Length - i - 1;
                    Document doc;
                    doc = hits.Doc(i);
                    //by using j we have the most recent results first
                    results[j] = new XmlHistory();
                    results[j].feedUrl = getFeed(Int32.Parse(doc.Get("feedIndex"))).url;
                    results[j].contents = doc.Get("contents").Substring(1);
                    results[j].dateTime = DateField.StringToDate(doc.Get("dateTime"));
                }
            }
            return results;
        }

        /// <summary>
        /// Returns all the history of a feed
        /// </summary>
        /// <param name="url">The URL of the feed</param>
        /// <returns>A vector containing the history of the feed</returns>
        public XmlHistory[] getHistory(String url)
        {
            return getHistory(getFeedIndex(url));
        }

        /// <summary>
        /// Removes all the history of the specified feed
        /// </summary>
        /// <param name="feedIndex">The index of the feed for which the history will be deleted</param>
        private void removeHistory(int feedIndex)
        {
            String q = "" + feedIndex;
            Query query = QueryParser.Parse(q, "feedIndex", new StandardAnalyzer());
            Hits hits;
            lock (historyLock)
            {
                hits = searcherHistory.Search(query);
            }
            IndexReader ir = IndexReader.Open(dirHistory);
            for (int i = 0; i < hits.Length(); i++)
                ir.Delete(hits.Id(i));
            lock (historyLock)
            {
                ir.Close();
                saveHistory();
            }
            if (delegHistoryChanged != null) delegHistoryChanged();
        }

        public void removeAllHistory()
        {
            String q = "z*";
            Query query = QueryParser.Parse(q, "contents", new StandardAnalyzer());
            Hits hits;
            lock (historyLock)
            {
                hits = searcherHistory.Search(query);
            }
            IndexReader ir = IndexReader.Open(dirHistory);
            for (int i = 0; i < hits.Length(); i++)
                ir.Delete(hits.Id(i));
            lock (historyLock)
            {
                ir.Close();
                saveHistory();
            }
            if (delegHistoryChanged != null) delegHistoryChanged();
        }

        /// <summary>
        /// Removes all the history of the specified feed
        /// </summary>
        /// <param name="feedURL">The URL of the feed for which the history will be deleted</param>
        public void removeHistory(String feedURL)
        {
            removeHistory(getFeedIndex(feedURL));
        }

        /// <summary>
        /// Deletes the specified feed and all its history from the database
        /// </summary>
        /// <param name="feedIndex">The index of the feed to remove</param>
        private void removeFeed(int feedIndex)
        {
            String q = "" + feedIndex;
            Query query = QueryParser.Parse(q, "feedIndex", new StandardAnalyzer());
            Hits hits;
            lock (feedLock)
            {
                hits = searcherFeed.Search(query);
            }

            IndexReader ir = IndexReader.Open(dirFeed);
            for (int i = 0; i < hits.Length(); i++)
                ir.Delete(hits.Id(i));      //should be just one feed found
            lock (feedLock)
            {
                ir.Close();
                saveFeed();
            }

            removeHistory(feedIndex);

            //if (delegCatFeedChanged != null) delegCatFeedChanged();
            //we will remove the item from the TreeView Manually
        }


        public void renameFeed(String feedURL, String newName)
        {
            String q = "z" + feedURL;
            Query query = QueryParser.Parse(QueryParser.Escape(q), "url", new StandardAnalyzer());
            Hits hits;
            lock (feedLock)
            {
                hits = searcherFeed.Search(query);
            }
            if (hits.Length() == 0) return;
            Document doc = hits.Doc(0);
            Document docNou = new Document();
            docNou.Add(Field.Text("feedIndex", doc.Get("feedIndex")));
            docNou.Add(Field.Text("catIndex", "" + doc.Get("catIndex")));
            docNou.Add(Field.Text("url", "z" + feedURL));
            docNou.Add(Field.Text("feedName", "z" + newName));
            IndexReader ir = IndexReader.Open(dirFeed);
            ir.Delete(hits.Id(0));      //should be just one feed found
            lock (feedLock)
            {
                ir.Close();
                writerFeed.AddDocument(docNou);
                saveFeed();
            }
            if (delegCatFeedChanged != null) delegCatFeedChanged(); //to re-sort the feed names
            
        }

        public void renameCategory(String catName, String newName)
        {
            String q = "z" + catName;
            Query query = QueryParser.Parse(QueryParser.Escape(q), "catName", new StandardAnalyzer());
            Hits hits;
            lock (catLock)
            {
                hits = searcherCat.Search(query);
            }
            if (hits.Length() == 0) return;
            Document doc = hits.Doc(0);
            Document docNou = new Document();
            docNou.Add(Field.Text("catIndex", doc.Get("catIndex")));
            docNou.Add(Field.Text("catName", "z" + newName));
            IndexReader ir = IndexReader.Open(dirCat);
            ir.Delete(hits.Id(0));      //should be just one feed found
            lock (catLock)
            {
                ir.Close();
                writerCat.AddDocument(docNou);
                saveCat();
            }
            if (delegCatFeedChanged != null) delegCatFeedChanged(); //to re-sort the categories

        }


        /// <summary>
        /// Deletes the specified feed and all its history from the database
        /// </summary>
        /// <param name="url">The URL of the feed to remove</param>
        public void removeFeed(String url)
        {
            removeFeed(getFeedIndex(url));
        }

        public void removeFeed(Object url_o)
        {
            removeFeed((String)url_o);
        }

        /// <summary>
        /// Deletes the specified category, all its feeds and their history from the database
        /// </summary>
        /// <param name="catName">The name of the category to delete</param>
        public void removeCategory(String catName)
        {
            
                int catIndex = -1;
                String q = "z" + catName;
                Query query = QueryParser.Parse(QueryParser.Escape(q), "catName", new StandardAnalyzer());
                Hits hits;

            lock (catLock)
            {
                hits = searcherCat.Search(query);
            }
            IndexReader ir = IndexReader.Open(dirCat);
            for (int i = 0; i < hits.Length(); i++)
            {
                Document doc = hits.Doc(i);
                if (doc.Get("catName").CompareTo(q) == 0)
                {
                    catIndex = Int32.Parse(doc.Get("catIndex"));
                    ir.Delete(hits.Id(i));
                }
            }
            if (catIndex == -1)
                return;
            lock (catLock)
            {
                ir.Close();
                saveCat();
            }

            
            q = "" + catIndex;
            query = QueryParser.Parse(q, "catIndex", new StandardAnalyzer());
            lock (feedLock)
            {
                hits = searcherFeed.Search(query);
            }
            ir = IndexReader.Open(dirFeed);
            for (int i = 0; i < hits.Length(); i++)
            {
                ir.Delete(hits.Id(i));
                removeHistory(Int32.Parse(hits.Doc(i).Get("feedIndex")));
            }
            lock (feedLock)
            {
                ir.Close();
                saveFeed();
            }
            //if (delegCatFeedChanged != null) delegCatFeedChanged();
            //ListRefresh() function will not be called - category will be deleted separately from TreeView
        }

        public void removeCategory(Object catName_o)
        {
            removeCategory((String)catName_o);
        }


        public XmlHistory[] searchHistory(String searchString)
        {
            String q = searchString;
            Query query = QueryParser.Parse(q, "contents", new StandardAnalyzer());
            Hits hits;
            lock (historyLock)
            {
                hits = searcherHistory.Search(query);
            }
            XmlHistory[] results = new XmlHistory[hits.Length()];
            for (int i = 0; i < hits.Length(); i++)
            {
                int j = results.Length - i - 1;
                Document doc = hits.Doc(i);
                results[j] = new XmlHistory();
                results[j].contents = doc.Get("contents").Substring(1);
                results[j].dateTime = DateField.StringToDate(doc.Get("dateTime"));
                //results[j].feedUrl = getFeed(Int32.Parse(doc.Get("feedIndex"))).url;
            }
            return results;

            //feedURL not returned in results!! (for now)
        }

        public void Optimize()
        {
            writerCat.Optimize();
            writerFeed.Optimize();
            writerHistory.Optimize();
        }

    }
}
