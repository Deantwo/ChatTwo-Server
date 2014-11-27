using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ChatTwo_Server
{
    public partial class Form1 : Form
    {
        UdpCommunication _server;
        string _advDbIp = "localhost";

        public Form1()
        {
            InitializeComponent();
            Global.MainWindow = this;
            
            _server = new UdpCommunication();

            notifyIcon1.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            bool worked;

            _server.MessageReceived += ChatTwo_Server_Protocol.MessageReceivedHandler;
            worked = _server.Start(9020);
            if(worked)
                WriteLog("UDP server started on port " + _server.Port + ".", Color.Green.ToArgb());
            else
                WriteLog("UDP server failed on port " + _server.Port + ".", Color.Red.ToArgb());

            worked = ConnectToDatabase();
            if (worked)
                WriteLog("Database connected esablished.", Color.Green.ToArgb());
            else
                WriteLog("Database connected failed.", Color.Red.ToArgb());
        }

        /// <summary>
        /// Test the connection to the server and if the domain user have access to it, if not then popup and warn the user.
        /// This method and RouterBackupDatabase.TestConnection need to be rewritten.
        /// </summary>
        private bool ConnectToDatabase()
        {
            // Basic user feedback.
            toolStripStatusLabel1.Text = "Testing connection and access to the database...";
            statusStrip1.Refresh(); // Has to be done or the statusStrip won't display the new toolStripStatusLabel text.

            // Test the connection and access, giving the user feedback if it fails.
            DatabaseComunication.ConnectionTestResult dbTest = DatabaseComunication.TestConnection(_advDbIp);
            toolStripStatusLabel1.Text = dbTest.ToString();
            // Check the result of the connetion test.
            bool connectTestSuccessful = dbTest == DatabaseComunication.ConnectionTestResult.Ready;
            if (connectTestSuccessful)
            {
                DatabaseComunication.Connect(_advDbIp);
                return true;
            }
            else
            {
                string errorMessage = "Unknown SQL error";
                string errorTip = "";
                switch (dbTest)
                {
                    case DatabaseComunication.ConnectionTestResult.NoConnection:
                        errorMessage = "Could not connect to the server at \"" + _advDbIp + "\".";
                        errorTip = Environment.NewLine + Environment.NewLine +
                            "You can change the IP address of the database in " + Environment.NewLine + 
                            "\"Settings -> SQL Database -> Set IP Address\"." + Environment.NewLine + 
                            "The problem could also be a timeout error.";
                        break;
                    case DatabaseComunication.ConnectionTestResult.NoPermission:
                        errorMessage = "Permission failed for user.";
                        errorTip = Environment.NewLine + Environment.NewLine +
                            "Talk to the system admin to gain access to the MySQL Database.";
                        break;
                    case DatabaseComunication.ConnectionTestResult.MissingDatabase:
                        errorMessage = "Database does not exist.";
                        errorTip = Environment.NewLine + Environment.NewLine +
                            "You can change the IP address of the database in " + Environment.NewLine +
                            "\"Settings -> SQL Database -> Create Database\"." + Environment.NewLine +
                            "The problem could also be a timeout error.";
                        break;
                    case DatabaseComunication.ConnectionTestResult.MissingTable:
                        errorMessage = "";
                        errorTip = "";
                        break;
                    case DatabaseComunication.ConnectionTestResult.OutDated:
                        errorMessage = "SQL database need to be updated.";
                        errorTip = "";
                        break;
                    default:
                        errorMessage = "Unknown SQL error.";
                        errorTip = "";
                        break;
                }
                MessageBox.Show(errorMessage + errorTip, "SQL Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                toolStripStatusLabel1.Text = errorMessage;
                return false;
            }
        }

        public void WriteLog(string log, int colorARGB = -16777216) // 0xFF000000 (Color.Black)
        {
            if (rtbxLog.InvokeRequired)
            { // Needed for multi-threading cross calls.
                this.Invoke(new Action<string, int>(this.WriteLog), new object[] { log, colorARGB });
            }
            else
            {
                // Add timestamp to the log entry.
                string timestamp = DateTime.Now.ToString("HH:mm:ss"); // "yyyy-MM-dd HH:mm:ss"
                log = timestamp + " " + log;
                // And this part only matters for the "### Start\n    ..." message.
                if (log.Contains(Environment.NewLine))
                {
                    int i = timestamp.Length;
                    timestamp = "";
                    for (; i > 0; i--)
                        timestamp += " ";
                    log = log.Replace(Environment.NewLine, Environment.NewLine + timestamp + " ");
                }

                // Just to prevent the first line from being empty.
                if (rtbxLog.Text != String.Empty)
                    rtbxLog.AppendText(Environment.NewLine);

                int lengthBeforeAppend = rtbxLog.Text.Length;
                // Write log to the textbox.
                rtbxLog.AppendText(log);
                // Put the focus on the textbox.
                rtbxLog.Focus();
                // Color the text.
                rtbxLog.SelectionStart = lengthBeforeAppend;
                rtbxLog.SelectionLength = log.Length;
                rtbxLog.SelectionColor = Color.FromArgb(colorARGB);

                // Delete the top line when there is over 1000 lines.
                if (rtbxLog.Lines.Length > 1000)
                {
                    rtbxLog.SelectionStart = 0;
                    rtbxLog.SelectionLength = rtbxLog.GetFirstCharIndexFromLine(1);
                    rtbxLog.ReadOnly = false; // Can't edit the text when the RichTextBox is in ReadOnly mode.
                    rtbxLog.SelectedText = String.Empty;
                    rtbxLog.ReadOnly = true;
                }

                // Put the cursor at the end of the text.
                rtbxLog.Select(rtbxLog.Text.Length, 0);
                // Scroll to the bottom of the textbox.
                rtbxLog.ScrollToCaret();
            }
        }

        private void CreateUser_Click(object sender, EventArgs e)
        {
            bool worked = DatabaseComunication.CreateUser(textBox1.Text, textBox2.Text);
            if (worked)
                WriteLog("User created: " + textBox1.Text, Color.Blue.ToArgb());
        }

        private void ReadUser_Click(object sender, EventArgs e)
        {
            UserObj user = DatabaseComunication.ReadUser((int)numericUpDown1.Value);
            if (user != null)
                WriteLog(user.ToString(), Color.Purple.ToArgb());
        }

        private void StatusUpdate_Click(object sender, EventArgs e)
        {
            bool worked = DatabaseComunication.UpdateUser((int)numericUpDown1.Value, new System.Net.IPEndPoint(new System.Net.IPAddress(new byte[] { 10, 0, 0, 1} ), 9020));
            if (worked)
                WriteLog("Updated user: " + (int)numericUpDown1.Value, Color.Blue.ToArgb());
        }

        #region Options
        private void minimizeToTrayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            minimizeToTrayToolStripMenuItem.Checked = !minimizeToTrayToolStripMenuItem.Checked;
            //Properties.Settings.Default.setTray = minimizeToTrayToolStripMenuItem.Checked;
            //Properties.Settings.Default.Save();
        }
        #endregion

        #region Minimize to Tray
        private void FormMain_Resize(object sender, EventArgs e)
        {
            if (minimizeToTrayToolStripMenuItem.Checked)
            {
                if (FormWindowState.Minimized == this.WindowState)
                {
                    notifyIcon1.Visible = true;
                    TrayBalloonTip("Minimized to tray", ToolTipIcon.None);
                    this.ShowInTaskbar = false;
                }
                else if (FormWindowState.Normal == this.WindowState)
                {
                    notifyIcon1.Visible = false;
                    this.ShowInTaskbar = true;
                }
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
        }

        private void RestoreToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
        }

        private void TrayBalloonTip(string message, ToolTipIcon toolTipIcon, int time = 500)
        {
            if (minimizeToTrayToolStripMenuItem.Checked)
            {
                notifyIcon1.BalloonTipIcon = toolTipIcon;
                notifyIcon1.BalloonTipText = message;
                notifyIcon1.ShowBalloonTip(time);
            }
        }
        #endregion

        private void close_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult reply = MessageBox.Show(this, "Are you sure you want to close the server?", "Close?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (reply == DialogResult.No)
                e.Cancel = true;
            else
            {
                // Add closing of sockets and stuff here!
                _server.Close();
                DatabaseComunication.Disconnect();
            }
        }
    }

    static class Global
    {
        public static Form1 MainWindow { set; get; }
    }
}
