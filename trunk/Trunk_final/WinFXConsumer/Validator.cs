using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Collections;
using System.Windows;
namespace WinFXConsumer
{


    //intro pt xml schemas:
    //http://www.w3schools.com/schema/schema_intro.asp

    //schema de aplicat pt validare rss 1.0
    //http://xmlfr.org/documentations/tutoriels/041022-0001

    //schema de aplicat petnru validare ATOM
    //http://atompub.org/2005/07/11/draft-ietf-atompub-format-10.html#schema
    //da e dubioasa, nu e  ok
    class Validator
    {
        static XmlReader reader;
        static XmlReader reader2;
        //
        //http://www.c-sharpcorner.com/Code/2002/April/XmlParser.asp
        //
       public  static XmlReader  ParseURL(string strUrl)
        {
            //string s="";
            reader = new XmlTextReader(strUrl);
            reader2 =new XmlTextReader(strUrl);
            try
            {

                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            Hashtable attributes = new Hashtable();
                            string strURI = reader.NamespaceURI;
                            string strName = reader.Name;
                            if (reader.HasAttributes)
                            {
                                for (int i = 0; i < reader.AttributeCount; i++)
                                {
                                    reader.MoveToAttribute(i);
                                    attributes.Add(reader.Name, reader.Value);
                                }
                            }
                            break;

                        default:
                            break;
                    }
                }
            }

            catch (XmlException e)
            {
                MessageBox.Show("Parsing error: Unspecified error.");
            }

            return reader2;
        }
        
    }
}
