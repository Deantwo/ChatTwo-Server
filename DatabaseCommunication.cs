﻿using System;
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
    static class DatabaseCommunication
    {
        private static bool _online;
        public static bool Active
        {
            get { return _online; }
        }

        private static int _version = 0;
        public static int Version
        {
            get { return _version; }
        }

        // StatusIntervalUpdate thread.
        private static Thread _threadStatusIntervalUpdate;

        // May have to implement this later. But only needed for the DateTime objects.
        //private  static CultureInfo _ci = CultureInfo.CreateSpecificCulture("en-US");

        // SqlConnection object is saved here for continued use.
        private static MySqlConnection _conn;

        // My attempt at making the MySqlConnection not close before all tasks are done using it.
        private static int _SqlWorker = 0;

        /// <summary>
        /// Creates the SqlConnection object and starts threaded methods.
        /// </summary>
        /// <param name="user">MySQL "User id" with either root access or access to the `ChatTwo` database.</param>
        /// <param name="password">Password of the user.</param>
        /// <param name="ip">IP address of the machine hosting the MySQL server.</param>
        /// <param name="port">Port number the MySQL server is running on.</param>
        public static void Connect(string user, string password, string ip, int port)
        {
            MySqlConnectionStringBuilder connBuilder = new MySqlConnectionStringBuilder();
            connBuilder.Add("User id", user);
            connBuilder.Add("Password", password);
            connBuilder.Add("Network Address", ip);
            connBuilder.Add("Port", port);
            connBuilder.Add("Database", "ChatTwo");
            connBuilder.Add("Connection timeout", 5);

            // Create the SqlConnection object using the saved IP address from settings.
            _conn = new MySqlConnection(connBuilder.ConnectionString);

            // Set the status of the database connection to on.
            _online = true;

            // Start the thread.
            _threadStatusIntervalUpdate = new Thread(() => StatusIntervalUpdate(connBuilder.ConnectionString));
            _threadStatusIntervalUpdate.Name = "StatusIntervalUpdate Thread (StatusIntervalUpdate method)";
            _threadStatusIntervalUpdate.Start();
        }

        /// <summary>
        /// Shuts down the database connection and gracefully stops threaded methods.
        /// </summary>
        public static void Disconnect()
        {
            // Set the status of the database connection to off.
            // This also makes the threaded method stop gracefully.
            _online = false;

            // Stop the thread.
            if (_threadStatusIntervalUpdate != null)
                _threadStatusIntervalUpdate.Join();

            // Delete the SqlConnection object.
            _conn = null;
        }

        /// <summary>
        /// Opens the MySqlConnection if it's not already open.
        /// </summary>
        private static void Open()
        {
            if (_SqlWorker == 0)
                _conn.Open();
            _SqlWorker++;
        }

        /// <summary>
        /// Closes the MySqlConnection if all tasks are down using it.
        /// </summary>
        private static void Close()
        {
            _SqlWorker--;
            if (_SqlWorker == 0 && _conn.State == ConnectionState.Open)
                _conn.Close();
        }

        #region Testing
        public enum ConnectionTestResult
        {
            UnknownError,
            NoConnection,
            FailLogin,
            NoPermission,
            MissingDatabase,
            MissingTable,
            OutDated,
            Successful
        }

        /// <summary>
        /// Tests the connection to the SQL server and returns a ConnectionTestResult enum result.
        /// </summary>
        /// <param name="user">MySQL "User id" with either root access or access to the `ChatTwo` database.</param>
        /// <param name="password">Password of the user.</param>
        /// <param name="ip">IP address of the machine hosting the MySQL server.</param>
        /// <param name="port">Port number the MySQL server is running on.</param>
        public static ConnectionTestResult TestConnection(string user, string password, string ip, int port)
        {
            // Shorter timeout will make the user not have to wait as long.
            // (Does not seem to have much of an effect on the connection timeout.)
            const int timeout = 5;

            MySqlConnectionStringBuilder connBuilder = new MySqlConnectionStringBuilder();
            connBuilder.Add("User id", user);
            connBuilder.Add("Password", password);
            connBuilder.Add("Network Address", ip);
            connBuilder.Add("Port", port);
            //connBuilder.Add("Database", "ChatTwo");
            connBuilder.Add("Connection timeout", timeout);

            // Test1: Test connection to the server using the IP address from the settings. Add "Connection Timeout" (even though it seem not to work).
            using (MySqlConnection testConn = new MySqlConnection(connBuilder.ConnectionString))
            {
                // Test2: Test access to the database.
                MySqlCommand test2 = new MySqlCommand("USE `ChatTwo`;", testConn);
                test2.CommandTimeout = timeout;

                // Test3: Test access to the `Contacts` table and the `Users` table..
                MySqlCommand test3 = new MySqlCommand("SELECT * FROM `ChatTwo`.`Contacts` WHERE 0 = 1;SELECT * FROM `ChatTwo`.`Users` WHERE 0 = 1;", testConn);
                test3.CommandTimeout = timeout;

                // Test4: Test access to the `ServerStatus` table and get the version number.
                MySqlCommand test4 = new MySqlCommand("SELECT `Version` FROM `ChatTwo`.`ServerStatus`;", testConn);
                test4.CommandTimeout = timeout;
                int version = -1;

                // Run all tests.
                try
                {
                    testConn.Open();
                    test2.ExecuteNonQuery();
                    test3.ExecuteNonQuery();
                    MySqlDataReader reader = test4.ExecuteReader();
                    if (reader.Read())
                    {
                        version = (int)reader["Version"];
                    }
                }
                catch (MySqlException ex)
                {
                    // If one of the tests fail, return an error message.
                    switch (ex.Number)
                    { // http://dev.mysql.com/doc/refman/5.6/en/error-messages-server.html
                        case 0:
                            if (ex.Message.Contains("Access denied"))
                            {
                                // Login failed
                                return ConnectionTestResult.FailLogin;
                            }
                            else
                            {
                                // SQL query timed out
                                return ConnectionTestResult.NoConnection;
                            }
                        case 1042:
                            // (ER_BAD_HOST_ERROR) Message: Can't get hostname for your address
                            return ConnectionTestResult.NoConnection;
                        case 1044:
                            // (ER_DBACCESS_DENIED_ERROR) Message: Access denied for user '%s'@'%s' to database '%s'
                            return ConnectionTestResult.NoPermission;
                        case 1049:
                            // (ER_BAD_DB_ERROR) Message: Unknown database '%s'
                            return ConnectionTestResult.MissingDatabase;
                        case 1146:
                            // (ER_NO_SUCH_TABLE) Message: Table '%s.%s' doesn't exist
                            return ConnectionTestResult.MissingTable;
                        default:
                            // Unknown SQL error
#if !DEBUG
                            return ConnectionTestResult.UnknownError;
#else
                            throw;
#endif
                    }
                }
                finally
                {
                    if (testConn.State != ConnectionState.Closed)
                        testConn.Close();
                }
            
                // If the version is old, suggest an update.
                if (version < _version)
                    return ConnectionTestResult.OutDated;
            }

            // If nothing bad happens, tell the user the program is ready.
            return ConnectionTestResult.Successful;
        }
        #endregion

        #region Creating and updating database
        /// <summary>
        /// Create the whole database from scratch.
        /// </summary>
        /// <param name="user">MySQL "User id" with either root access or access to the `ChatTwo` database.</param>
        /// <param name="password">Password of the user.</param>
        /// <param name="ip">IP address of the machine hosting the MySQL server.</param>
        /// <param name="port">Port number the MySQL server is running on.</param>
        public static bool CreateDatabase(string user, string password, string ip, int port)
        {
            int cmdResult = 0;

            MySqlConnectionStringBuilder connBuilder = new MySqlConnectionStringBuilder();
            connBuilder.Add("User id", user);
            connBuilder.Add("Password", password);
            connBuilder.Add("Network Address", ip);
            connBuilder.Add("Port", port);
            //connBuilder.Add("Database", "ChatTwo");
            //connBuilder.Add("Connection timeout", 5);

            using (MySqlConnection tempConn = new MySqlConnection(connBuilder.ConnectionString))
            {
                using (MySqlCommand cmd = new MySqlCommand(
                    "CREATE DATABASE IF NOT EXISTS `ChatTwo`;" + Environment.NewLine +
                    "USE `ChatTwo`;" + Environment.NewLine +
                    "" + Environment.NewLine +
                    "DROP TABLE IF EXISTS `ServerStatus`;" + Environment.NewLine +
                    "CREATE TABLE `ServerStatus` (" + Environment.NewLine +
                    "    `Version` INT NOT NULL," + Environment.NewLine +
                    "    `CreationDate` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP," + Environment.NewLine +
                    "    `LastUpdated` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP" + Environment.NewLine +
                    "    );" + Environment.NewLine +
                    "INSERT INTO `ServerStatus` (`Version`) VALUES(0);" + Environment.NewLine +
                    "" + Environment.NewLine +
                    "DROP TABLE IF EXISTS `Users`;" + Environment.NewLine +
                    "DROP TABLE IF EXISTS `Contacts`;" + Environment.NewLine +
                    "CREATE TABLE `Users` (" + Environment.NewLine +
                    "    `ID` INT NOT NULL PRIMARY KEY AUTO_INCREMENT," + Environment.NewLine +
                    "    `Name` VARCHAR(30) NOT NULL UNIQUE," + Environment.NewLine +
                    "    `Password` VARCHAR(26) NOT NULL," + Environment.NewLine +
                    "    `Online` TINYINT(1) NOT NULL DEFAULT 0," + Environment.NewLine +
                    "    `Socket` VARCHAR(51) NULL," + Environment.NewLine +
                    "    `LastOnline` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP," + Environment.NewLine +
                    "    `Registered` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP" + Environment.NewLine +
                    "    );" + Environment.NewLine +
                    "CREATE TABLE `Contacts` (" + Environment.NewLine +
                    "    `ID_1` INT NOT NULL," + Environment.NewLine +
                    "    `ID_2` INT NOT NULL," + Environment.NewLine +
                    "    `1To2` TINYINT(1) NOT NULL DEFAULT 0," + Environment.NewLine +
                    "    `2To1` TINYINT(1) NOT NULL DEFAULT 0" + Environment.NewLine +
                    "    );" + Environment.NewLine +
                    "" + Environment.NewLine +
                    "DROP TRIGGER IF EXISTS `trig_UserInsert`;" + Environment.NewLine +
                    "DROP TRIGGER IF EXISTS `trig_UserDeleted`;" + Environment.NewLine +
                    "DROP PROCEDURE IF EXISTS `StatusUpdate`;" + Environment.NewLine +
                    "DROP PROCEDURE IF EXISTS `StatusIntervalUpdate`;" + Environment.NewLine +
                    "DROP PROCEDURE IF EXISTS `ContactsMutual`;" + Environment.NewLine +
                    "DROP PROCEDURE IF EXISTS `ContactsALL`;" + Environment.NewLine +
                    "DROP PROCEDURE IF EXISTS `ContactsAdd`;" + Environment.NewLine +
                    "DROP PROCEDURE IF EXISTS `ContactsRemove`;" + Environment.NewLine +
                    "" + Environment.NewLine +
                    "CREATE TRIGGER `trig_UserInsert`" + Environment.NewLine +
                    "    BEFORE INSERT ON `users`" + Environment.NewLine +
                    "    FOR EACH ROW" + Environment.NewLine +
                    "BEGIN" + Environment.NewLine +
                    "    DELETE FROM `Contacts`" + Environment.NewLine +
                    "        WHERE `ID_1` = NEW.ID" + Environment.NewLine +
                    "           OR `ID_2` = NEW.ID;" + Environment.NewLine +
                    "END;" + Environment.NewLine +
                    "" + Environment.NewLine +
                    "CREATE TRIGGER `trig_UserDeleted`" + Environment.NewLine +
                    "    BEFORE DELETE ON `users`" + Environment.NewLine +
                    "    FOR EACH ROW" + Environment.NewLine +
                    "BEGIN" + Environment.NewLine +
                    "    DELETE FROM `Contacts`" + Environment.NewLine +
                    "        WHERE `ID_1` = OLD.ID" + Environment.NewLine +
                    "           OR `ID_2` = OLD.ID;" + Environment.NewLine +
                    "END;" + Environment.NewLine +
                    "" + Environment.NewLine +
                    "CREATE DEFINER=CURRENT_USER PROCEDURE `StatusUpdate`(" + Environment.NewLine +
                    "    IN p_ID INT," + Environment.NewLine +
                    "    IN p_Socket VARCHAR(51)" + Environment.NewLine +
                    ")" + Environment.NewLine +
                    "    MODIFIES SQL DATA" + Environment.NewLine +
                    "BEGIN" + Environment.NewLine +
                    "    UPDATE `Users`" + Environment.NewLine +
                    "        SET `Online` = 1," + Environment.NewLine +
                    "            `Socket` = p_Socket," + Environment.NewLine +
                    "            `LastOnline` = CURRENT_TIMESTAMP" + Environment.NewLine +
                    "        WHERE `ID` = p_ID;" + Environment.NewLine +
                    "END;" + Environment.NewLine +
                    "" + Environment.NewLine +
                    "CREATE DEFINER=CURRENT_USER PROCEDURE `StatusIntervalUpdate`()" + Environment.NewLine +
                    "    MODIFIES SQL DATA" + Environment.NewLine +
                    "BEGIN" + Environment.NewLine +
                    "    SELECT `ID` FROM `Users`" + Environment.NewLine +
                    "        WHERE (`Online` = 1) " + Environment.NewLine +
                    "          AND NOT (`LastOnline` BETWEEN timestamp(DATE_SUB(NOW(), INTERVAL 10 SECOND)) AND NOW());" + Environment.NewLine +
                    "    UPDATE `Users`" + Environment.NewLine +
                    "        SET `Online` = 0," + Environment.NewLine +
                    "            `Socket` = NULL" + Environment.NewLine +
                    "        WHERE (`Online` = 1)" + Environment.NewLine +
                    "          AND NOT (`LastOnline` BETWEEN timestamp(DATE_SUB(NOW(), INTERVAL 10 SECOND)) AND NOW());" + Environment.NewLine +
                    "END;" + Environment.NewLine +
                    "" + Environment.NewLine +
                    "CREATE DEFINER=CURRENT_USER PROCEDURE `ContactsMutual`(" + Environment.NewLine +
                    "    IN p_ID INT" + Environment.NewLine +
                    ")" + Environment.NewLine +
                    "    READS SQL DATA" + Environment.NewLine +
                    "BEGIN" + Environment.NewLine +
                    "    SELECT IF((`ID_1` = p_ID), `ID_2`, `ID_1`) AS `ContactID`" + Environment.NewLine +
                    "        FROM `Contacts`" + Environment.NewLine +
                    "        WHERE (`ID_1` = p_ID OR `ID_2` = p_ID)" + Environment.NewLine +
                    "          AND `1To2` = 1 AND `2To1` = 1;" + Environment.NewLine +
                    "END;" + Environment.NewLine +
                    "" + Environment.NewLine +
                    "CREATE DEFINER=CURRENT_USER PROCEDURE `ContactsAll`(" + Environment.NewLine +
                    "    IN `p_ID` INT" + Environment.NewLine +
                    ")" + Environment.NewLine +
                    "    READS SQL DATA" + Environment.NewLine +
                    "BEGIN" + Environment.NewLine +
                    "    SELECT IF((`ID_1` = p_ID), `ID_2`, `ID_1`) AS ContactID," + Environment.NewLine +
                    "           IF((`ID_1` = p_ID AND `1To2` = 1) OR (`ID_2` = p_ID AND `2To1` = 1), 1, 0) AS FromMe," + Environment.NewLine +
                    "           IF((`ID_1` = p_ID AND `2To1` = 1) OR (`ID_2` = p_ID AND `1To2` = 1), 1, 0) AS ToMe" + Environment.NewLine +
                    "        FROM `Contacts`" + Environment.NewLine +
                    "        WHERE `ID_1` = p_ID OR `ID_2` = p_ID;" + Environment.NewLine +
                    "END;" + Environment.NewLine +
                    "" + Environment.NewLine +
                    "CREATE DEFINER=CURRENT_USER PROCEDURE `ContactsAdd`(" + Environment.NewLine +
                    "    IN `p_ID` INT," + Environment.NewLine +
                    "    IN `p_ContactID` INT" + Environment.NewLine +
                    ")" + Environment.NewLine +
                    "    MODIFIES SQL DATA" + Environment.NewLine +
                    "BEGIN" + Environment.NewLine +
                    "    IF (SELECT EXISTS(SELECT 1 FROM `Contacts`" + Environment.NewLine +
                    "        WHERE (`ID_1` = p_ID AND `ID_2` = p_ContactID)" + Environment.NewLine +
                    "           OR (`ID_2` = p_ID AND `ID_1` = p_ContactID) LIMIT 1) as contactFound) = 1" + Environment.NewLine +
                    "    THEN" + Environment.NewLine +
                    "        UPDATE `contacts`" + Environment.NewLine +
                    "        SET `2To1` = IF(`ID_2` = p_ID, 1, `2To1`), `1To2` = IF(`ID_1` = p_ID, 1, `1To2`)" + Environment.NewLine +
                    "        WHERE (`ID_1` = p_ID AND `ID_2` = p_ContactID)" + Environment.NewLine +
                    "           OR (`ID_2` = p_ID AND `ID_1` = p_ContactID);" + Environment.NewLine +
                    "    ELSE" + Environment.NewLine +
                    "        INSERT INTO `contacts`(`ID_1`, `ID_2`, `1To2`, `2To1`)" + Environment.NewLine +
                    "        VALUES (p_ID, p_ContactID, 1, 0);" + Environment.NewLine +
                    "    END IF;" + Environment.NewLine +
                    "END;" + Environment.NewLine +
                    "" + Environment.NewLine +
                    "CREATE DEFINER=CURRENT_USER PROCEDURE `ContactsRemove`(" + Environment.NewLine +
                    "    IN `p_ID` INT," + Environment.NewLine +
                    "    IN `p_ContactID` INT" + Environment.NewLine +
                    ")" + Environment.NewLine +
                    "    MODIFIES SQL DATA" + Environment.NewLine +
                    "BEGIN" + Environment.NewLine +
                    "    UPDATE `contacts`" + Environment.NewLine +
                    "    SET `2To1` = IF(`ID_2` = p_ID, 0, `2To1`), `1To2` = IF(`ID_1` = p_ID, 0, `1To2`)" + Environment.NewLine +
                    "    WHERE (`ID_1` = p_ID AND `ID_2` = p_ContactID)" + Environment.NewLine +
                    "       OR (`ID_2` = p_ID AND `ID_1` = p_ContactID);" + Environment.NewLine +
                    "    DELETE FROM `contacts`" + Environment.NewLine +
                    "    WHERE `1To2` = 0 AND `2To1` = 0;" + Environment.NewLine +
                    "END;"
                    , tempConn))
                {
                    try
                    {
                        tempConn.Open();
                        // Execute SQL command.
                        cmdResult = cmd.ExecuteNonQuery();
                    }
                    finally
                    {
                        if (tempConn.State != System.Data.ConnectionState.Closed)
                            tempConn.Close();
                    }
                }
            }
            return (cmdResult != 0);
        }

        /// <summary>
        /// Update an out of date database.
        /// </summary>
        /// <param name="user">MySQL "User id" with either root access or access to the `ChatTwo` database.</param>
        /// <param name="password">Password of the user.</param>
        /// <param name="ip">IP address of the machine hosting the MySQL server.</param>
        /// <param name="port">Port number the MySQL server is running on.</param>
        public static bool UpdateDatabase(string user, string password, string ip, int port)
        {
            int cmdResult = 0;

            MySqlConnectionStringBuilder connBuilder = new MySqlConnectionStringBuilder();
            connBuilder.Add("User id", user);
            connBuilder.Add("Password", password);
            connBuilder.Add("Network Address", ip);
            connBuilder.Add("Port", port);
            //connBuilder.Add("Database", "ChatTwo");
            //connBuilder.Add("Connection timeout", 5);

            using (MySqlConnection tempConn = new MySqlConnection(connBuilder.ConnectionString))
            {
                // Get the database version number from the `ServerStatus` table.
                int version = -1;
                using (MySqlCommand cmd = new MySqlCommand("SELECT `ChatTwo`.`Version` FROM `ServerStatus`;", tempConn))
                {
                    try
                    {
                        Open();
                        // Execute SQL command.
                        MySqlDataReader reader = cmd.ExecuteReader();
                        if (reader.Read())
                        {
                            version = (int)reader["Version"];
                        }
                    }
                    finally
                    {
                        Close();
                    }
                }

                switch (version)
                {
                    case 0:
                        throw new NotImplementedException("There is no update from version 0 (yet).");

                        //using (MySqlCommand cmd = new MySqlCommand("UPDATE `ServerStatus` SET `Version` = 1, `LastUpdated` = NOW();", _conn))
                        //{
                        //    try
                        //    {
                        //        Open();
                        //        // Execute SQL command.
                        //        cmdResult = cmd.ExecuteNonQuery();
                        //    }
                        //    finally
                        //    {
                        //        Close();
                        //    }
                        //}
                        //break;
                    default:
                        break;
                }
            }
            return (cmdResult != 0);
        }
        #endregion

        #region Common routin
        /// <summary>
        /// Create a user on the `Users` table. Username is a unique column and this medthod will return false if it is already in use.
        /// </summary>
        /// <param name="username">Username of the user to create.</param>
        /// <param name="password">26 base64 character hash string of the user's password.</param>
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
                catch (MySqlException ex)
                {
                    if (ex.Number == 1062) // http://dev.mysql.com/doc/refman/5.6/en/error-messages-server.html
                        // (ER_DUP_ENTRY) Message: Duplicate entry '%s' for key %d
                        // If the username is already in use.
                        return false;
                    throw ex;
                }
                finally
                {
                    Close();
                }
            }
            return (cmdResult != 0); // cmdResult content the number of affected rows.
        }

        /// <summary>
        /// Read all database information about a user. Returns an UserObj with all the informatioin.
        /// </summary>
        /// <param name="id">ID number of the requested user.</param>
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
                        cmdResult = new UserObj();
                        cmdResult.ID = (int)reader["ID"];
                        cmdResult.Name = (string)reader["Name"];
                        cmdResult.Online = (bool)reader["Online"];
                        cmdResult.StringSocket(reader["Socket"].ToString());
                        cmdResult.LastOnline = (DateTime)reader["LastOnline"];//, _ci);
                        cmdResult.Registered = (DateTime)reader["Registered"];//, _ci);
                    }
                }
                finally
                {
                    Close();
                }
            }
            return cmdResult;
        }

        /// <summary>
        /// Returns an UserObj containing the username and the userId. Returns null if username/password is incorrect.
        /// </summary>
        /// <param name="name">User's username.</param>
        /// <param name="password">Base64 hash string of the password.</param>
        static public UserObj LoginUser(string name, string password)
        {
            UserObj cmdResult = null;
            using (MySqlCommand cmd = new MySqlCommand("SELECT `ID`, `Name` FROM `Users` WHERE `Name` = @name AND `Password` = @password;", _conn))
            {
                // Add parameterized parameters to prevent SQL injection.
                cmd.Parameters.AddWithValue("@name", name);
                cmd.Parameters.AddWithValue("@password", password);

                try
                {
                    Open();
                    // Execute SQL command.
                    MySqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        cmdResult = new UserObj();
                        cmdResult.ID = (int)reader["ID"];
                        cmdResult.Name = reader["Name"].ToString();
                    }
                }
                finally
                {
                    Close();
                }
            }
            return cmdResult;
        }

        /// <summary>
        /// Updates a user's online status and socket in the database. Using the StatusUpdate stored procedure.
        /// </summary>
        /// <param name="id">User's ID.</param>
        /// <param name="socket">The IPEndPoint to be written to the database.</param>
        static public bool UpdateUser(int id, IPEndPoint socket)
        {
            int cmdResult = 0;
            using (MySqlCommand cmd = new MySqlCommand("StatusUpdate", _conn))
            {
                //Set up cmd to reference stored procedure 'StatusUpdate'.
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
            return (cmdResult != 0); // cmdResult content the number of affected rows.
        }

        /// <summary>
        /// Checks for all the users that haven't reported in for more than 10 seconds. Using the StatusIntervalUpdate stored procedure.
        /// </summary>
        /// <param name="connString">The connection string is needed here because a separate MySqlConnetion is started for this.</param>
        static public void StatusIntervalUpdate(string connString) // Threaded looping method.
        {
            using (MySqlConnection intervalConn = new MySqlConnection(connString))
            {
                MySqlCommand cmd = new MySqlCommand("StatusIntervalUpdate", intervalConn);
                //Set up cmd to reference stored procedure 'StatusIntervalUpdate'.
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                try
                {
                    while (_online)
                    {
                        intervalConn.Open();
                        // Execute SQL command.
                        MySqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            int userId = (int)reader["ID"];
                            Thread userStatusChange = new Thread(() => UserStatusChanged(userId, false));
                            userStatusChange.Name = "UserChange Thread (UserStatusChanged method)";
                            userStatusChange.Start();
                        }
                        intervalConn.Close();
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
                    if (intervalConn.State == ConnectionState.Open)
                        intervalConn.Close();
                }
            }
        }

        /// <summary>
        /// When a user changes status. For example coming online or going offline.
        /// This will find all online mutual contacts and fire an OnUserStatusChange for each.
        /// </summary>
        /// <param name="userId">ID number of the user changing status.</param>
        /// <param name="comesOnline">Set true if they are coming online, or false if they are going offline.</param>
        private static void UserStatusChanged(int userId, bool comesOnline) // Threaded method.
        {
            // Should make the ContactsMutual stored procedure only return contacts that are online.
            // Would make this method faster by only having it make one query. Just not sure how.
            List<int> contactIds = new List<int>();
            using (MySqlCommand cmd = new MySqlCommand("ContactsMutual", _conn))
            {
                //Set up cmd to reference stored procedure 'ContactsMutual'.
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                //Create input parameter (p_ID) and assign a value (id)
                MySqlParameter idParam = new MySqlParameter("@p_ID", userId);
                idParam.Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters.Add(idParam);

                try
                {
                    Open();
                    // Execute SQL command.
                    MySqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        contactIds.Add((int)reader["ContactID"]);
                    }
                }
                finally
                {
                    Close();
                }
            }
            if (contactIds.Count != 0)
            {
                // This is all created in the method, so no chance of SQL injection.
                string users = String.Join(" OR `ID` = ", contactIds);
                contactIds.Clear();
                using (MySqlCommand cmd = new MySqlCommand("SELECT `ID` FROM `Users` WHERE `Online` = 1 AND (`ID` = " + users + ");", _conn))
                {

                    try
                    {
                        Open();
                        // Execute SQL command.
                        MySqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            contactIds.Add((int)reader["ID"]);
                        }
                    }
                    finally
                    {
                        Close();
                    }
                }
                // And finally we have a list of all the contacts that are online and we can message them.
                foreach (int contactId in contactIds)
                {
                    // Fire an OnUserStatusChange event.
                    UserStatusChangeEventArgs args = new UserStatusChangeEventArgs();
                    args.TellId = contactId;
                    args.IdIs = userId;
                    args.Online = comesOnline;
                    OnUserStatusChange(args);
                }
            }
        }
        #endregion

        private static void OnUserStatusChange(UserStatusChangeEventArgs e)
        {
            EventHandler<UserStatusChangeEventArgs> handler = UserStatusChange;
            if (handler != null)
            {
                handler(null, e);
            }
        }
        public static event EventHandler<UserStatusChangeEventArgs> UserStatusChange;
    }

    public class UserStatusChangeEventArgs : EventArgs
    {
        public int TellId { get; set; }
        public int IdIs { get; set; }
        public bool Online { get; set; }
        public IPEndPoint Socket { get; set; }
    }
}
