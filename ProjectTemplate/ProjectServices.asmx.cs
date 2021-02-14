using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Data;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

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
			Account[] account = GetAccounts();
			return account.Length;
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
		public Account[] GetAccounts()
        {
			// TODO Check if a user is logged in
			// TODO only give password out if the user is an admin
			DataTable sqlDataTable = new DataTable();
			string sqlSelect = "select EmployeeId, Password, Email, FirstName, LastName, PhoneNumber, Title from Users";

			MySqlConnection sqlConnection = new MySqlConnection(getConString());
			//set up our command object to use our connection, and our query
			MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

			MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
			sqlDa.Fill(sqlDataTable);

			List<Account> accounts = new List<Account>();
			for (int i = 0; i < sqlDataTable.Rows.Count; i++)
            {
				accounts.Add(new Account
				{
					EmployeeId = Convert.ToInt32(sqlDataTable.Rows[i]["EmployeeId"]),
					Password = sqlDataTable.Rows[i]["Password"].ToString(),
					Email = sqlDataTable.Rows[i]["Email"].ToString(),
					FirstName = sqlDataTable.Rows[i]["FirstName"].ToString(),
					LastName = sqlDataTable.Rows[i]["LastName"].ToString(),
					PhoneNumber = sqlDataTable.Rows[i]["PhoneNumber"].ToString(),
					Title = sqlDataTable.Rows[i]["Title"].ToString()
				}); 
				
            }

			return accounts.ToArray();
		}


		[WebMethod]
		public Account[] SeeEmails()
		{

			Account[] accounts = GetAccounts();
			List<Account> accountEmails = new List<Account>();
			foreach (Account account in accounts)
			{
				accountEmails.Add(new Account
				{
					EmployeeId = account.EmployeeId,
					Email = account.Email
				});
			}

			return accountEmails.ToArray();
		}


		[WebMethod]
		public Account[] SeeFirstNames()
		{

			Account[] accounts = GetAccounts();
			List<Account> accountEmails = new List<Account>();
			foreach (Account account in accounts)
			{
				accountEmails.Add(new Account
				{
					EmployeeId = account.EmployeeId,
					FirstName = account.FirstName
				});
			}

			return accountEmails.ToArray();
		}


		[WebMethod]
		public Account[] SeeLastNames()
		{
			Account[] accounts = GetAccounts();
			List<Account> accountEmails = new List<Account>();
			foreach (Account account in accounts)
			{
				accountEmails.Add(new Account
				{
					EmployeeId = account.EmployeeId,
					LastName = account.LastName
				});
			}

			return accountEmails.ToArray();
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
