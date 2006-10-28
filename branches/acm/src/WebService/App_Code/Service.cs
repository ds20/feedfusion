using System;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Data.SqlClient;

[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
public class Service : System.Web.Services.WebService
{
    public Service () {

        //Uncomment the following line if using designed components 
        //InitializeComponent(); 
    }

    [WebMethod]
    public string HelloWorld() {
       
        return "Hello World";
    }


    [WebMethod]
    public string GetFeed(string user, string password)
    {

        string s="";
        try
        {
            SqlConnection thisConnection = new SqlConnection(@"Data Source=localhost\SQLEXPRESS;Initial Catalog=FeedFusion;Integrated Security=True");
            thisConnection.Open();
            SqlCommand thisCommand = thisConnection.CreateCommand();
            thisCommand.CommandText = "SELECT OPML FROM Users  WHERE UserName='"+user+"' and Password='"+password+"' ";
            SqlDataReader thisreader = thisCommand.ExecuteReader();
            bool i = thisreader.Read();
            s = (thisreader.IsDBNull(thisreader.GetOrdinal("OPML")))
                           ? null : thisreader["OPML"].ToString();
            
        }
        catch (SqlException e) 
        { 
        
        }

        return s;
       // return "0";   
    }


    [WebMethod]
    public int NewUser(string user,string password,string opml)
    {
        int i=0;
        try
        {
            SqlConnection thisConnection = new SqlConnection(@"Data Source=localhost\SQLEXPRESS;Initial Catalog=FeedFusion;Integrated Security=True");
            thisConnection.Open();
            SqlCommand thisCommand = thisConnection.CreateCommand();
            thisCommand.CommandText = "INSERT Users(UserName,Password,OPML) VALUES('"+user+"','"+password+"','"+opml+"')";
            i=thisCommand.ExecuteNonQuery(); 
         
        }
        catch (SqlException e) 
        { 

        }

        return i;
    }


    [WebMethod]
    public int UpdateFeed(string user, string password, string opml)
    {
        int i = 0;
        try
        {
            SqlConnection thisConnection = new SqlConnection(@"Data Source=localhost\SQLEXPRESS;Initial Catalog=FeedFusion;Integrated Security=True");
            thisConnection.Open();
            SqlCommand thisCommand = thisConnection.CreateCommand();
            thisCommand.CommandText = "UPDATE Users SET OPML='"+opml+"' WHERE UserName='"+user+"' and Password='"+password+"'";
            i = thisCommand.ExecuteNonQuery();
        }
        catch (SqlException e) 
        { 
        
        }

        return i;
    }
    
}
