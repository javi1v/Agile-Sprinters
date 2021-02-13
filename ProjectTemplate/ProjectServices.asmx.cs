using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Data;

namespace ProjectTemplate
{
	[WebService(Namespace = "http://tempuri.org/")]
	[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
	[System.ComponentModel.ToolboxItem(false)]
	[System.Web.Script.Services.ScriptService]

	public class ProjectServices : System.Web.Services.WebService
	{
		// Our databases credentials
		private string dbID = "2021group4";
		private string dbPass = "group42021";
		private string dbName = "2021group4";
		
		////////////////////////////////////////////////////////////////////////
		///call this method anywhere that you need the connection string!
		////////////////////////////////////////////////////////////////////////
		private string getConString() {
			return "SERVER=107.180.1.16; PORT=3306; DATABASE=" + dbName+"; UID=" + dbID + "; PASSWORD=" + dbPass;
		}
		

		[WebMethod(EnableSession = true)]
		public string TestConnection()
		{
			try
			{
				string testQuery = "select * from Users";
				// Connects to the database
				MySqlConnection con = new MySqlConnection(getConString());

				MySqlCommand cmd = new MySqlCommand(testQuery, con);
				MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
				DataTable table = new DataTable();
				adapter.Fill(table);
				return "Success!";
			}
			catch (Exception e)
			{
				return "Something went wrong, please check your credentials and db name and try again.  Error: "+e.Message;
			}


		}

		[WebMethod]
		public int NumberOfAccounts()
		{
			// A simple query that will need to be updated when size of database is larger
			string sqlSelect = "SELECT * from Users";

			//set up our connection object to be ready to use our connection string
			MySqlConnection sqlConnection = new MySqlConnection(getConString());
			//set up our command object to use our connection, and our query
			MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);


			//a data adapter acts like a bridge between our command object and 
			//the data we are trying to get back and put in a table object
			MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
			//here's the table we want to fill with the results from our query
			DataTable sqlDt = new DataTable();
			//here we go filling it!
			sqlDa.Fill(sqlDt);
			//return the number of rows we have, that's how many accounts are in the system!
			return sqlDt.Rows.Count;
		}

		[WebMethod]
		public bool LogOn(string email, string password)
		{
			// Returns this value to say whether or not the log on was succesful
			bool wasSuccesful = false;

			// A basic select query that uses the @ symbol for safety
			string sqlSelect = "SELECT EmployeeId FROM Users WHERE Email=@idValue and Password=@passValue";

			// Connects us to the database
			MySqlConnection sqlConnection = new MySqlConnection(getConString());
			// Create new object to send the select statement to the database connection
			MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

			//tell our command to replace the @parameters with the actual values
			//we decode them because they had to be encoded to be sent over the web
			sqlCommand.Parameters.AddWithValue("@idValue", HttpUtility.UrlDecode(email));
			sqlCommand.Parameters.AddWithValue("@passValue", HttpUtility.UrlDecode(password));

			//a data adapter acts like a bridge between our command object and 
			//the data we are trying to get back and put in a table object
			MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
			//here's the table we want to fill with the results from our query
			DataTable sqlDt = new DataTable();
			//here we go filling it!
			sqlDa.Fill(sqlDt);
			//check to see if any rows were returned.  If they were, it means it's 
			//a legit account
			if (sqlDt.Rows.Count > 0)
			{
				//flip our flag to true so we return a value that lets them know they're logged in
				wasSuccesful = true;
			}

			//return the result!
			return wasSuccesful;
		}

		[WebMethod]
		public string SeeAccountNames()
		{

			// Stores all first and last names
			string accounts = "";

			// This query gets all the first and last names
			string sqlSelect = "SELECT FirstName, LastName FROM Users";

			//set up our connection object to be ready to use our connection string
			MySqlConnection sqlConnection = new MySqlConnection(getConString());
			//set up our command object to use our connection, and our query
			MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

			//a data adapter acts like a bridge between our command object and 
			//the data we are trying to get back and put in a table object
			MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
			//here's the table we want to fill with the results from our query
			DataTable sqlDt = new DataTable();
			//here we go filling it!
			sqlDa.Fill(sqlDt);

			// Formats the list of names
			foreach (DataRow row in sqlDt.Rows)
			{
				accounts += row[0];
				accounts += " " + row[1];
				accounts += ", ";
			}

			// returns all the names
			return accounts;
		}


		[WebMethod]
		public string SeeEmails()
		{

			// Stores all emails
			string emails = "";

			// This query gets all the emails
			string sqlSelect = "SELECT Email FROM Users";

			//set up our connection object to be ready to use our connection string
			MySqlConnection sqlConnection = new MySqlConnection(getConString());
			//set up our command object to use our connection, and our query
			MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

			//a data adapter acts like a bridge between our command object and 
			//the data we are trying to get back and put in a table object
			MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
			//here's the table we want to fill with the results from our query
			DataTable sqlDt = new DataTable();
			//here we go filling it!
			sqlDa.Fill(sqlDt);

			// Formats the list of emails
			foreach (DataRow row in sqlDt.Rows)
			{
				emails += row[0];
				emails += ", ";
			}

			// returns all the emails
			return emails;
		}


		[WebMethod]
		public string SeeFirstNames()
		{

			// Stores all first names
			string firstNames = "";

			// This query gets all the first names
			string sqlSelect = "SELECT FirstName FROM Users";

			//set up our connection object to be ready to use our connection string
			MySqlConnection sqlConnection = new MySqlConnection(getConString());
			//set up our command object to use our connection, and our query
			MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

			//a data adapter acts like a bridge between our command object and 
			//the data we are trying to get back and put in a table object
			MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
			//here's the table we want to fill with the results from our query
			DataTable sqlDt = new DataTable();
			//here we go filling it!
			sqlDa.Fill(sqlDt);

			// Formats the list of names
			foreach (DataRow row in sqlDt.Rows)
			{
				firstNames += row[0];
				firstNames += ", ";
			}

			// returns all the names
			return firstNames;
		}


		[WebMethod]
		public string SeeLastNames()
		{
			// Stores all last names
			string lastNames = "";

			// This query gets all the last names
			string sqlSelect = "SELECT LastName FROM Users";

			//set up our connection object to be ready to use our connection string
			MySqlConnection sqlConnection = new MySqlConnection(getConString());
			//set up our command object to use our connection, and our query
			MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

			//a data adapter acts like a bridge between our command object and 
			//the data we are trying to get back and put in a table object
			MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
			//here's the table we want to fill with the results from our query
			DataTable sqlDt = new DataTable();
			//here we go filling it!
			sqlDa.Fill(sqlDt);

			// Formats the list of names
			foreach (DataRow row in sqlDt.Rows)
			{
				lastNames += row[0];
				lastNames += ", ";
			}

			// returns all the names
			return lastNames;
		}

		[WebMethod]
		public bool LogOff()
        {
			// Can not log out of the system until their is a true log on feature
			// Very temporary return true statement
			return true;
        }

	}
}
