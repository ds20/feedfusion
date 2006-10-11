using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace PluginInterface
{
    public interface rssInterface
    {
        void setDocument(XmlDocument doc);

        bool canParse();

        string parsedHTML();

        void showConfiguration();

        void addToToolbar(System.Windows.Controls.ToolBar ToolBar);

        string description();

        string name();

        void getDataBase(DataBaseEngine data);

        void setOwner(System.Windows.Window window);

        void feedChanged(string name, string category);

    }


    public interface DataBaseEngine
    {
        bool categoryExists(String catName);

        bool addCategory(string catName);

        String[] getCategories();

        bool feedExists(String url);

        bool addFeed(String catName, String url, String feedName);

        bool addFeed(String catName, String url);

        void RegisterEventHandler(EventsClass e);
        

    }

    public interface EventsClass
    {
        void FeedDownloaded(string feed);
        void NewFeedAdded(string feed);
        void CategoryAdded(string category);
    }
}
