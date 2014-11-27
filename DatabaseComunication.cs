using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using MySql.Data.MySqlClient; // http://dev.mysql.com/downloads/connector/net/
using System.Net;
using System.Threading;
using System.Globalization;

namespace ChatTwo_Server
{
    static class DatabaseComunication
    {
        static bool _online;
        public static bool Active
        {
            get { return _online; }
        }

        // StatusIntervalUpdate thread.
        static Thread _threadStatusIntervalUpdate;

        static CultureInfo _ci = CultureInfo.CreateSpecificCulture("en-US");

        // Define the SQL connection string.
        private static string _connectionString = "Server=@IP;Database=ChatTwo;UID=root;PWD=";

        // SqlConnection object is saved here for continued use.
        private static MySqlConnection _conn;

        /// <summary>
        /// Creates the SqlConnection object.
        /// </summary>
        public static void Connect(string ip)
        {
            // Create the SqlConnection object using the saved IP address from settings.
            _conn = new MySqlConnection(_connectionString.Replace("@IP", ip));

            // Start the thread.
            _online = true;
            _threadStatusIntervalUpdate = new Thread(new ThreadStart(StatusIntervalUpdate));
            _threadStatusIntervalUpdate.Name = "StatusIntervalUpdate Thread (StatusIntervalUpdate method)";
            _threadStatusIntervalUpdate.Start();
        }

        /// <summary>
        /// Creates the SqlConnection object.
        /// </summary>
        public static void Disconnect()
        {
            // Stop the thread.
            _online = false;
            _threadStatusIntervalUpdate.Join();

            // Delete the SqlConnection object.
            _conn = null;
        }

        private static void Open()
        {
            _conn.Open();
        }

        private static void Close()
        {
            if (_conn.State != ConnectionState.Closed)
                _conn.Close();
        }

        public enum ConnectionTestResult
        {
            UnknownError,
            NoConnection,
            NoPermission,
            MissingDatabase,
            MissingTable,
            OutDated,
            Ready
        }

        /// <summary>
        /// Tests the connection to the SQL server and returns a string with the result.
        /// This method and FormMain.ConnectToDatabase need to be rewritten.
        /// </summary>
        public static ConnectionTestResult TestConnection(string ip)
        {
            // Shorter timeout will make the user not have to wait as long.
            // (Does not seem to have much of an effect on the connection timeout.)
            const int timeout = 5;

            // Test1: Test connection to the server using the IP address from the settings. Add "Connection Timeout" (even though it seem not to work).
            MySqlConnection conTest = new MySqlConnection(_connectionString.Replace("@IP", ip).Replace("Database=ChatTwo;","") + ";Connection Timeout=" + timeout.ToString());

            // Test2: Test access to the database.
            MySqlCommand test2 = new MySqlCommand("USE `ChatTwo`;", conTest);
            test2.CommandTimeout = timeout;

            // Test3: Test access to the `Contacts` table and the `Users` table..
            MySqlCommand test3 = new MySqlCommand("SELECT 1 FROM `ChatTwo`.`ServerStatus`;", conTest);
            test3.CommandTimeout = timeout;
            int version = -1;

            // Test3: Test access to the `Contacts` table and the `Users` table..
            MySqlCommand test4 = new MySqlCommand("SELECT * FROM `ChatTwo`.`Contacts` WHERE 0 = 1;SELECT * FROM `ChatTwo`.`Users` WHERE 0 = 1;", conTest);
            test4.CommandTimeout = timeout;

            // Run all tests.
            try
            {
                conTest.Open();
                test2.ExecuteNonQuery();
                MySqlDataReader reader = test3.ExecuteReader(); 
                if (reader.Read())
                {
                    version = (int)reader["Version"];
                }
                test4.ExecuteNonQuery();
            }
            catch (MySqlException ex)
            {
                // If one of the tests fail, return an error message.
                switch (ex.Number)
                { // http://dev.mysql.com/doc/refman/5.6/en/error-messages-server.html#error_er_dup_entry_with_key_name
                    case 0:
                        // SQL query timed out
                        return ConnectionTestResult.NoConnection;
                    case 1042:
                        // (ER_BAD_HOST_ERROR) Message: Can't get hostname for your address
                        return ConnectionTestResult.NoConnection;
                    //case ????:
                    //    // Permission failed for user
                    //    return ConnectionTestResult.NoPermission;
                    case 1049:
                        // (ER_BAD_DB_ERROR) Message: Unknown database '%s'
                        return ConnectionTestResult.MissingDatabase;
                    case 1146:
                        // (ER_NO_SUCH_TABLE) Message: Table '%s.%s' doesn't exist
                        return ConnectionTestResult.MissingTable;
                    default:
                        // Unknown SQL error
                        return ConnectionTestResult.UnknownError;
                }
            }
            finally
            {
                if (conTest.State != System.Data.ConnectionState.Closed)
                    conTest.Close();
            }
            
            // If the version is old, suggest an update.
            if (version < 0)
                return ConnectionTestResult.OutDated;

            // If nothing bad happens, tell the user the program is ready.
            //return "Ready";
            return ConnectionTestResult.Ready;
        }

        /// <summary>
        /// Create a user on the `Users` table.
        /// </summary>
        public static bool CreateUser(string username, string password)
        {
            int cmdResult = 0;
            using (MySqlCommand cmd = new MySqlCommand("INSERT INTO `Users` (`Name`, `Password`) VALUES(@username, @password);", _conn))
            {
                // Add parameterized parameters to prevent SQL injection.
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@password", password);

                try
                {
                    Open();
                    // Execute SQL command.
                    cmdResult = cmd.ExecuteNonQuery();
                }
                catch(MySqlException ex)
                {
                    int a = ex.Number;
                    string b = ex.Message;
                    string c = ex.ToString();
                    switch (ex.Number)
                    { // http://dev.mysql.com/doc/refman/5.6/en/error-messages-server.html#error_er_dup_entry_with_key_name
                        case 1042:
                            // "Could not connect to the server at \"" + ip + "\"."
                            break;
                        case 1061:
                        case 1062:
                        case 1586:
                            // "Username already in use."
                            break;
                        default:
                            break;
                    }
                }
                finally
                {
                    Close();
                }
            }
            return (cmdResult != 0);
        }

        static public UserObj ReadUser(int id)
        {
            UserObj cmdResult = null;
            using (MySqlCommand cmd = new MySqlCommand("SELECT * FROM `Users` WHERE `ID` = @id;", _conn))
            {
                // Add parameterized parameters to prevent SQL injection.
                cmd.Parameters.AddWithValue("@id", id);

                try
                {
                    Open();
                    // Execute SQL command.
                    MySqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        cmdResult = new UserObj(
                            (int)reader["ID"],
                            (string)reader["Name"],
                            (string)reader["Password"],
                            (bool)reader["Online"],
                            reader["Socket"].ToString(),
                            (DateTime)reader["LastOnline"],// _ci),
                            (DateTime)reader["Registered"]//, _ci)
                            );
                    }
                }
                catch (MySqlException ex)
                {
                    int a = ex.Number;
                    string b = ex.Message;
                    string c = ex.ToString();
                    switch (ex.Number)
                    { // http://dev.mysql.com/doc/refman/5.6/en/error-messages-server.html#error_er_dup_entry_with_key_name
                        case 1042:
                            // "Could not connect to the server at \"" + ip + "\"."
                            break;
                        case 1061:
                        case 1062:
                        case 1586:
                            // "Username already in use."
                            break;
                        default:
                            break;
                    }
                }
                finally
                {
                    Close();
                }
            }
            return cmdResult;
        }

        static public bool UpdateUser(int id, IPEndPoint socket)
        {
            int cmdResult = 0;
            using (MySqlCommand cmd = new MySqlCommand("StatusUpdate", _conn))
            {
                //Set up myCommand to reference stored procedure 'StatusUpdate'.
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                //Create input parameter (p_ID) and assign a value (id)
                MySqlParameter idParam = new MySqlParameter("@p_ID", id);
                idParam.Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters.Add(idParam);
                //Create input parameter (p_Socket) and assign a value (socket)
                MySqlParameter socketParam = new MySqlParameter("@p_Socket", socket.ToString());
                socketParam.Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters.Add(socketParam);

                try
                {
                    Open();
                    // Execute SQL command.
                    cmdResult = cmd.ExecuteNonQuery();
                }
                finally
                {
                    Close();
                }
            }
            return (cmdResult != 0);
        }

        static public void StatusIntervalUpdate()
        {
            MySqlCommand cmd = new MySqlCommand("StatusIntervalUpdate", _conn);
            //Set up myCommand to reference stored procedure 'StatusIntervalUpdate'.
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            try
            {
                while (_online)
                {
                    Open();
                    cmd.ExecuteNonQuery(); // Execute SQL command.
                    Close();
                    Thread.Sleep(1000); // 1 seconds.
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("### " + _threadStatusIntervalUpdate.Name + " has crashed:");
                System.Diagnostics.Debug.WriteLine("### " + ex.Message);
                System.Diagnostics.Debug.WriteLine("### " + ex.ToString());
            }
            finally
            {
                Close();
            }
        }
    }
}
