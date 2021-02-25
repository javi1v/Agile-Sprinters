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
		private string getConString()
		{
			return "SERVER=107.180.1.16; PORT=3306; DATABASE=" + dbName + "; UID=" + dbID + "; PASSWORD=" + dbPass;
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
				return "Something went wrong, please check your credentials and db name and try again.  Error: " + e.Message;
			}
		}


		[WebMethod(EnableSession = true)]
		public int NumberOfAccounts()
		{
			Account[] account = GetAccounts();
			return account.Length;
		}


		[WebMethod(EnableSession = true)]
		public int NumberOfAccountRequests()
		{
			Account[] account = GetAccountRequests();
			return account.Length;
		}


		[WebMethod(EnableSession = true)]
		public bool LogOn(string email, string password)
		{
			// Returns this value to say whether or not the log on was succesful
			bool wasSuccesful = false;

			// A basic select query that uses the @ symbol for safety
			string sqlSelect = "SELECT EmployeeId, Title FROM Users WHERE Email=@idValue and Password=@passValue";

			// Connects us to the database
			MySqlConnection sqlConnection = new MySqlConnection(getConString());
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
			
			sqlDa.Fill(sqlDt);
			
			// If something was returned then the log on was successful
			if (sqlDt.Rows.Count > 0)
			{
				//Returns true if it was succesful and creates a sesion id for them
				Session["id"] = sqlDt.Rows[0]["EmployeeId"];
				Session["admin"] = sqlDt.Rows[0]["Title"];
				wasSuccesful = true;
			}

			//return the result!
			return wasSuccesful;
		}


		[WebMethod(EnableSession = true)]
		public bool LogOff()
		{
			// This will make the user relog in, in order to access the services
			// This basically removes the cookie that identifies the user
			Session.Abandon();
			return true;
		}


		[WebMethod(EnableSession = true)]
		public Account[] GetAccounts()
		{
			// TODO Check if a user is logged in
			// TODO only give password out if the user is an admin

			if (Session["id"] != null)
			{
				DataTable sqlDataTable = new DataTable();
				string sqlSelect = "select EmployeeId, Password, Email, FirstName, LastName, PhoneNumber, Title from Users where IsActive=1";

				MySqlConnection sqlConnection = new MySqlConnection(getConString());
				//set up our command object to use our connection, and our query
				MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

				MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
				sqlDa.Fill(sqlDataTable);

				List<Account> accounts = new List<Account>();
				for (int i = 0; i < sqlDataTable.Rows.Count; i++)
				{
					if (isAdmin())
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
					else
					{
						accounts.Add(new Account
						{
							EmployeeId = Convert.ToInt32(sqlDataTable.Rows[i]["EmployeeId"]),
							Email = sqlDataTable.Rows[i]["Email"].ToString(),
							FirstName = sqlDataTable.Rows[i]["FirstName"].ToString(),
							LastName = sqlDataTable.Rows[i]["LastName"].ToString(),
							PhoneNumber = sqlDataTable.Rows[i]["PhoneNumber"].ToString(),
							Title = sqlDataTable.Rows[i]["Title"].ToString()
						});

					}

				}
				return accounts.ToArray();
			}
			else return new Account[0];
		}


		[WebMethod(EnableSession = true)]
		public Account[] GetAccountRequests()
		{
			// checks if the user is an admin, and if it is shows them the accounts
			if (isAdmin())
			{
				DataTable sqlDataTable = new DataTable();
				string sqlSelect = "select EmployeeId, Email, FirstName, LastName, PhoneNumber, Title from Users where IsActive=0";

				MySqlConnection sqlConnection = new MySqlConnection(getConString());
				//set up our command object to use our connection, and our query
				MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

				MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
				sqlDa.Fill(sqlDataTable);

				List<Account> accountRequests = new List<Account>();
				for (int i = 0; i < sqlDataTable.Rows.Count; i++)
				{
					accountRequests.Add(new Account
					{
						EmployeeId = Convert.ToInt32(sqlDataTable.Rows[i]["EmployeeId"]),
						Email = sqlDataTable.Rows[i]["Email"].ToString(),
						FirstName = sqlDataTable.Rows[i]["FirstName"].ToString(),
						LastName = sqlDataTable.Rows[i]["LastName"].ToString(),
						PhoneNumber = sqlDataTable.Rows[i]["PhoneNumber"].ToString(),
						Title = sqlDataTable.Rows[i]["Title"].ToString()
					});
				}
				return accountRequests.ToArray();
			}
			else return new Account[0];
		}


		[WebMethod(EnableSession = true)]
		public Account[] SeeEmails()
		{
			if (isUser())
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
			else return new Account[0];
		}


		[WebMethod(EnableSession = true)]
		public Account[] SeeFirstNames()
		{
			if (isUser())
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
			else return new Account[0];
		}


		[WebMethod(EnableSession = true)]
		public Account[] SeeLastNames()
		{
			if (isUser())
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
			else return new Account[0];
		}


		[WebMethod(EnableSession = true)]
		public void UpdateAccount(string EmployeeIdToUpdate, string Email, string Password, string FirstName, string LastName, string PhoneNumber, string Title, string IsActive)
        {
			if (isAdmin())
            {
				string sqlUpdate = "update Users set Email=@Email, Password=@Password, FirstName=@FirstName," +
					"LastName=@LastName, PhoneNumber=@PhoneNumber, Title=@Title, IsActive=@IsActive where EmployeeId=@EmployeeId";
				MySqlConnection sqlConnection = new MySqlConnection(getConString());
				MySqlCommand updateCommand = new MySqlCommand(sqlUpdate, sqlConnection);

				updateCommand.Parameters.AddWithValue("@EmployeeId", Convert.ToInt32(HttpUtility.UrlDecode(EmployeeIdToUpdate)));
				updateCommand.Parameters.AddWithValue("@Email", HttpUtility.UrlDecode(Email));
				updateCommand.Parameters.AddWithValue("@Password", HttpUtility.UrlDecode(Password));
				updateCommand.Parameters.AddWithValue("@FirstName", HttpUtility.UrlDecode(FirstName));
				updateCommand.Parameters.AddWithValue("@LastName", HttpUtility.UrlDecode(LastName));
				updateCommand.Parameters.AddWithValue("@PhoneNumber", HttpUtility.UrlDecode(PhoneNumber));
				updateCommand.Parameters.AddWithValue("@Title", HttpUtility.UrlDecode(Title));
				updateCommand.Parameters.AddWithValue("@IsActive", Convert.ToInt32(HttpUtility.UrlDecode(IsActive)));

				sqlConnection.Open();
				try
                {
					updateCommand.ExecuteNonQuery();
                }
				catch (Exception e)
                {
                }
				sqlConnection.Close();
			}
        }


		[WebMethod(EnableSession = true)]
		public void RequestAccount(string Email, string Password, string FirstName, string LastName, string PhoneNumber, string Title)
		{
			//the only thing fancy about this query is SELECT LAST_INSERT_ID() at the end.  All that
			//does is tell mySql server to return the primary key of the last inserted row.
			string sqlSelect = "insert into Users (Password, Email, FirstName, LastName, PhoneNumber, Title) " +
				"values(@Password, @Email, @FirstName, @LastName, @PhoneNumber, @Title); SELECT LAST_INSERT_ID();";

			MySqlConnection sqlConnection = new MySqlConnection(getConString());
			MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

			sqlCommand.Parameters.AddWithValue("@Email", HttpUtility.UrlDecode(Email));
			sqlCommand.Parameters.AddWithValue("@Password", HttpUtility.UrlDecode(Password));
			sqlCommand.Parameters.AddWithValue("@FirstName", HttpUtility.UrlDecode(FirstName));
			sqlCommand.Parameters.AddWithValue("@LastName", HttpUtility.UrlDecode(LastName));
			sqlCommand.Parameters.AddWithValue("@PhoneNumber", HttpUtility.UrlDecode(PhoneNumber));
			sqlCommand.Parameters.AddWithValue("@Title", HttpUtility.UrlDecode(Title));

			//this time, we're not using a data adapter to fill a data table.  We're just
			//opening the connection, telling our command to "executescalar" which says basically
			//execute the query and just hand me back the number the query returns (the ID, remember?).
			//don't forget to close the connection!
			sqlConnection.Open();
			//we're using a try/catch so that if the query errors out we can handle it gracefully
			//by closing the connection and moving on
			try
			{
				int accountID = Convert.ToInt32(sqlCommand.ExecuteScalar());
				//here, you could use this accountID for additional queries regarding
				//the requested account.  Really this is just an example to show you
				//a query where you get the primary key of the inserted row back from
				//the database!
			}
			catch (Exception e)
			{
			}
			sqlConnection.Close();
		}


		[WebMethod(EnableSession = true)]
		public void ApproveAccount(string IdToEnable)
        {
			if (isAdmin())
            {
				string sqlUpdate = "update Users set IsActive=1 where EmployeeId = @IdToEnable";
				MySqlConnection sqlConnection = new MySqlConnection(getConString());
				MySqlCommand sqlCommand = new MySqlCommand(sqlUpdate, sqlConnection);

				sqlCommand.Parameters.AddWithValue("@IdToEnable", Convert.ToInt32(HttpUtility.UrlDecode(IdToEnable)));

				sqlConnection.Open();
				try
				{
					sqlCommand.ExecuteNonQuery();
				}
				catch (Exception e)
				{
				}
				sqlConnection.Close();
			}
        }


		[WebMethod(EnableSession = true)]
		public void RejectAccount(string IdToDelete)
		{
			if (isAdmin())
			{
				string sqlDelete = "delete from Users where EmployeeId=@EmployeeId";

				MySqlConnection sqlConnection = new MySqlConnection(getConString());
				MySqlCommand sqlCommand = new MySqlCommand(sqlDelete, sqlConnection);

				sqlCommand.Parameters.AddWithValue("@EmployeeId", Convert.ToInt32(HttpUtility.UrlDecode(IdToDelete)));

				sqlConnection.Open();
				try
				{
					sqlCommand.ExecuteNonQuery();
				}
				catch (Exception e)
				{
				}
				sqlConnection.Close();
			}
		}


		[WebMethod(EnableSession =true)]
		public string GetCurrentEmail()
        {
			string sqlGetEmail = "Select Email from Users where EmployeeId = @EmployeeId";
			string currentEmailAddress = "";

			MySqlConnection sqlConnection = new MySqlConnection(getConString());
			//set up our command object to use our connection, and our query
			MySqlCommand emailCommand = new MySqlCommand(sqlGetEmail, sqlConnection);
			emailCommand.Parameters.AddWithValue("@EmployeeId", Session["id"]);

			MySqlDataAdapter sqlDa = new MySqlDataAdapter(emailCommand);
			//here's the table we want to fill with the results from our query
			DataTable sqlDt = new DataTable();
			//here we go filling it!
			sqlDa.Fill(sqlDt);
			//check to see if any rows were returned.  If they were, it means it's 
			//a legit account
			if (sqlDt.Rows.Count > 0)
			{
				currentEmailAddress = sqlDt.Rows[0]["Email"].ToString();
			}
			return currentEmailAddress;
		}


		[WebMethod(EnableSession = true)]
		public void SendMessage(string EmployeeEmailToSendTo, string Subject, string Message)
        {
			string sqlMessage = "Insert into Messages (From_id, To_id, Subject, Message, timesent, wasread) values(@fromEmail, @toEmail, @Subject, @Message, Now(), false)";
			string currentEmailAddress = GetCurrentEmail();
			
			MySqlConnection connection = new MySqlConnection(getConString());
			//set up our command object to use our connection, and our query
			MySqlCommand messageCommand = new MySqlCommand(sqlMessage, connection);
			messageCommand.Parameters.AddWithValue("@fromEmail", currentEmailAddress);
			messageCommand.Parameters.AddWithValue("@toEmail", EmployeeEmailToSendTo);
			messageCommand.Parameters.AddWithValue("@Subject", Subject);
			messageCommand.Parameters.AddWithValue("@Message", Message);

			connection.Open();
			try
			{
				messageCommand.ExecuteNonQuery();
			}
			catch (Exception e)
			{
			}
			connection.Close();
		}


		[WebMethod(EnableSession = true)] 
		public Message[] GetMessages()
        {
			// checks if the user is an admin, and if it is shows them the accounts
			if (isUser())
			{
				DataTable sqlDataTable = new DataTable();
				string sqlSelect = "select MessageId, From_id, To_id, Subject, Message, timesent, wasRead from Messages where To_id=@To_id";

				MySqlConnection sqlConnection = new MySqlConnection(getConString());
				//set up our command object to use our connection, and our query
				MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);
				sqlCommand.Parameters.AddWithValue("@To_id", GetCurrentEmail());

				MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
				sqlDa.Fill(sqlDataTable);

				List<Message> messages = new List<Message>();
				for (int i = 0; i < sqlDataTable.Rows.Count; i++)
				{
					messages.Add(new Message
					{
						MessageId = Convert.ToInt32(sqlDataTable.Rows[i]["MessageId"]),
						From_id = sqlDataTable.Rows[i]["From_id"].ToString(),
						To_id = sqlDataTable.Rows[i]["To_id"].ToString(),
						Subject = sqlDataTable.Rows[i]["Subject"].ToString(),
						Text = sqlDataTable.Rows[i]["Message"].ToString(),
						timesent = sqlDataTable.Rows[i]["timesent"].ToString(),
						wasRead = sqlDataTable.Rows[i]["wasRead"].ToString()
					});
				}
				return messages.ToArray();
			}
			else return new Message[0];
		}


		[WebMethod(EnableSession = true)]
		public Message[] GetSentMessages()
		{
			// checks if the user is an admin, and if it is shows them the accounts
			if (isUser())
			{
				DataTable sqlDataTable = new DataTable();
				string sqlSelect = "select MessageId, From_id, To_id, Subject, Message, timesent, wasRead from Messages where From_id=@From_id";

				MySqlConnection sqlConnection = new MySqlConnection(getConString());
				//set up our command object to use our connection, and our query
				MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);
				sqlCommand.Parameters.AddWithValue("@From_id", GetCurrentEmail());

				MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
				sqlDa.Fill(sqlDataTable);

				List<Message> messages = new List<Message>();
				for (int i = 0; i < sqlDataTable.Rows.Count; i++)
				{
					messages.Add(new Message
					{
						MessageId = Convert.ToInt32(sqlDataTable.Rows[i]["MessageId"]),
						From_id = sqlDataTable.Rows[i]["From_id"].ToString(),
						To_id = sqlDataTable.Rows[i]["To_id"].ToString(),
						Subject = sqlDataTable.Rows[i]["Subject"].ToString(),
						Text = sqlDataTable.Rows[i]["Message"].ToString(),
						timesent = sqlDataTable.Rows[i]["timesent"].ToString(),
						wasRead = sqlDataTable.Rows[i]["wasRead"].ToString()
					});
				}
				return messages.ToArray();
			}
			else return new Message[0];
		}


		[WebMethod(EnableSession = true)]
		public Message[] GetAllMessages()
        {
			if (isAdmin())
			{
				DataTable sqlDataTable = new DataTable();
				string sqlSelect = "select MessageId, From_id, To_id, Subject, Message, timesent, wasRead from Messages";

				MySqlConnection sqlConnection = new MySqlConnection(getConString());
				//set up our command object to use our connection, and our query
				MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

				MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
				sqlDa.Fill(sqlDataTable);

				List<Message> messages = new List<Message>();
				for (int i = 0; i < sqlDataTable.Rows.Count; i++)
				{
					messages.Add(new Message
					{
						MessageId = Convert.ToInt32(sqlDataTable.Rows[i]["MessageId"]),
						From_id = sqlDataTable.Rows[i]["From_id"].ToString(),
						To_id = sqlDataTable.Rows[i]["To_id"].ToString(),
						Subject = sqlDataTable.Rows[i]["Subject"].ToString(),
						Text = sqlDataTable.Rows[i]["Message"].ToString(),
						timesent = sqlDataTable.Rows[i]["timesent"].ToString(),
						wasRead = sqlDataTable.Rows[i]["wasRead"].ToString()
					});
				}
				return messages.ToArray();
			}
			else return new Message[0];
		}



		[WebMethod(EnableSession = true)]
		public void DeleteUser(int EmployeeIdToDelete)
        {
			// if it was an admin it deletes the account
			if (isAdmin())
			{
				string sqlDelete = "DELETE FROM Users WHERE EmployeeId=@employeeId";

				MySqlConnection sqlConnection = new MySqlConnection(getConString());
				//set up our command object to use our connection, and our query
				MySqlCommand deleteCommand = new MySqlCommand(sqlDelete, sqlConnection);
				deleteCommand.Parameters.AddWithValue("@employeeId", EmployeeIdToDelete);

				sqlConnection.Open();
				try
				{
					deleteCommand.ExecuteNonQuery();
				}
				catch (Exception e)
				{
				}
				sqlConnection.Close();
			}
		}

		
		[WebMethod(EnableSession = true)]
		public void CreateAccount(string Email, string Password, string FirstName, string LastName, string PhoneNumber, string Title)
        {
			if (isAdmin())
            {
				string sqlInsert = "insert into Users (Password, Email, FirstName, LastName, PhoneNumber, Title, IsActive) " +
				"Values(@password, @email, @firstName, @lastName, @phoneNumber, @title, 1)";

				MySqlConnection sqlConnection = new MySqlConnection(getConString());
				//set up our command object to use our connection, and our query
				MySqlCommand insertCommand = new MySqlCommand(sqlInsert, sqlConnection);
				insertCommand.Parameters.AddWithValue("@password", Password);
				insertCommand.Parameters.AddWithValue("@email", Email);
				insertCommand.Parameters.AddWithValue("@firstName", FirstName);
				insertCommand.Parameters.AddWithValue("@lastName", LastName);
				insertCommand.Parameters.AddWithValue("@phoneNumber", PhoneNumber);
				insertCommand.Parameters.AddWithValue("@title", Title);

				sqlConnection.Open();
				try
				{
					insertCommand.ExecuteNonQuery();
				}
				catch (Exception e)
				{
				}
				sqlConnection.Close();
			}
		}
		

		[WebMethod(EnableSession = true)] 
		public bool isAdmin()
        {
			if (Session["admin"] != null)
			{
				if (Session["admin"].ToString() == "admin" || Session["admin"].ToString() == "Admin")
				{
					return true;
				}
				else return false;
			}
			else return false;
		}

		
		[WebMethod(EnableSession = true)]
		public bool isUser()
        {
			if (Session["id"] != null)
			{
				return true;
			}
			else return false;
        }
		
		//WARNING
		// A deprecated function replaced with the built in Session Id 
		//WARNING
		/*
		public bool CheckIfAdmin(string adminEmail, string adminPassword)
        {
			bool isAdmin = false;
			// Checks to see if an admin is initiating the delete
			try
			{
				string sqlSelect = "SELECT EmployeeId FROM Users WHERE Email=@idValue and Password=@passValue and Title=@admin";

				// Connects us to the database
				MySqlConnection adminConnection = new MySqlConnection(getConString());
				// Create new object to send the select statement to the database connection
				MySqlCommand adminCommand = new MySqlCommand(sqlSelect, adminConnection);

				//tell our command to replace the @parameters with the actual values
				//we decode them because they had to be encoded to be sent over the web
				adminCommand.Parameters.AddWithValue("@idValue", HttpUtility.UrlDecode(adminEmail));
				adminCommand.Parameters.AddWithValue("@passValue", HttpUtility.UrlDecode(adminPassword));
				adminCommand.Parameters.AddWithValue("@admin", "Admin");

				//a data adapter acts like a bridge between our command object and 
				//the data we are trying to get back and put in a table object
				MySqlDataAdapter sqlData = new MySqlDataAdapter(adminCommand);
				//here's the table we want to fill with the results from our query
				DataTable sqlDatatable = new DataTable();
				//here we go filling it!
				sqlData.Fill(sqlDatatable);
				//check to see if any rows were returned.  If they were, it means it's 
				//a legit account
				if (sqlDatatable.Rows.Count > 0)
				{
					//flip our flag to true so we return a value that lets them know they're logged in
					isAdmin = true;
				}
			}
			catch (Exception e)
			{
			}
			return isAdmin;
		}
		*/

	}
}
