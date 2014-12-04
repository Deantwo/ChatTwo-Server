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
    public partial class FormMain : Form
    {
        UdpCommunication _server;

        Image _infoTip;

        public FormMain()
        {
            InitializeComponent();
            Global.MainWindow = this; // This is just so I can call WriteLog from other classes.

#if DEBUG
            this.Text += " (DEBUG)";
#endif
            
            _server = new UdpCommunication();

            notifyIcon1.BalloonTipTitle = this.Name;
            notifyIcon1.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

            // Steal the system "informaion icon" and resize it.
            _infoTip = SystemIcons.Information.ToBitmap();
            _infoTip = (Image)(new Bitmap(_infoTip, new Size(12, 12)));

            tabSqlTest.Parent = null;
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            _server.MessageReceived += ChatTwo_Server_Protocol.MessageReceivedHandler;
            ChatTwo_Server_Protocol.MessageTransmission += _server.SendMessage;

            _server.SocketServer = new System.Net.IPEndPoint(new System.Net.IPAddress(new byte[] { 87, 52, 32, 46 }), 9020); // My server IP and port. Just to test.

            bool worked = _server.Start(9020);
            if(worked)
                WriteLog("UDP server started on port " + _server.Port + ".", Color.Green.ToArgb());
            else
                WriteLog("UDP server failed on port " + _server.Port + ".", Color.Red.ToArgb());
        }

        #region Database Setup
        private void tbxSql_ConnectionStringValuesChanged(object sender, EventArgs e)
        {
            btnSqlConnect.Enabled = false;
            btnSqlCreate.Enabled = false;
            btnSqlUpdate.Enabled = false;
            lblSqlConnection.Text = "Test: -";
            lblSqlConnection.Image = null;
        }

        private void btnSqlTest_Click(object sender, EventArgs e)
        {
            tbxSql_ConnectionStringValuesChanged(null, null);

            // Basic user feedback.
            toolStripStatusLabel1.Text = "Testing connection and access to the database...";
            statusStrip1.Refresh(); // Has to be done or the statusStrip won't display the new toolStripStatusLabel text.

            // Test the connection and access, giving the user feedback if it fails.
            DatabaseCommunication.ConnectionTestResult dbTest = DatabaseCommunication.TestConnection(tbxSqlUser.Text, tbxSqlPassword.Text, tbxSqlAddress.Text, (int)nudSqlPort.Value);
            toolStripStatusLabel1.Text = dbTest.ToString();
            // Check the result of the connetion test.
            bool connectTestSuccessful = dbTest == DatabaseCommunication.ConnectionTestResult.Successful;
            if (connectTestSuccessful)
            {
                lblSqlConnection.Text = "Test: successful";
                toolTip1.SetToolTip(lblSqlConnection, "");
                btnSqlConnect.Enabled = true;
            }
            else
            {
                string errorMessage;
                string errorTip;
                switch (dbTest)
                {
                    case DatabaseCommunication.ConnectionTestResult.NoConnection:
                        errorMessage = "Could not connect to the server at \"" + tbxSqlAddress.Text + ":" + (int)nudSqlPort.Value + "\"";
                        errorTip = "." + Environment.NewLine + Environment.NewLine +
                            "Please make sure the MySQL server is running and the IP address and port is correct.";
                        break;
                    case DatabaseCommunication.ConnectionTestResult.FailLogin:
                        errorMessage = "Login rejected by server";
                        errorTip = "." + Environment.NewLine + Environment.NewLine +
                            "Please make sure the username and password is correct.";
                        break;
                    case DatabaseCommunication.ConnectionTestResult.NoPermission:
                        errorMessage = "Permission failed for user";
                        errorTip = "." + Environment.NewLine + Environment.NewLine +
                            "Talk to the system admin to gain access to the MySQL server.";
                        break;
                    case DatabaseCommunication.ConnectionTestResult.MissingDatabase:
                        errorMessage = "Database does not exist";
                        errorTip = "." + Environment.NewLine + Environment.NewLine +
                            "You can create/repair the database by clicking the button below.";
                        btnSqlCreate.Enabled = true;
                        break;
                    case DatabaseCommunication.ConnectionTestResult.MissingTable:
                        errorMessage = "";
                        errorTip = "";
                        break;
                    case DatabaseCommunication.ConnectionTestResult.OutDated:
                        errorMessage = "SQL database need to be updated";
                        errorTip = ".";
                        btnSqlUpdate.Enabled = true;
                        break;
                    default:
                        errorMessage = "Unknown SQL error";
                        errorTip = ".";
                        break;
                }
                MessageBox.Show(errorMessage + errorTip, "SQL Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                toolStripStatusLabel1.Text = "SQL Error: " + errorMessage;
                lblSqlConnection.Text = "Test: failed";
                lblSqlConnection.Image = _infoTip;
                toolTip1.SetToolTip(lblSqlConnection, errorMessage + errorTip);
            }
        }

        private void btnSqlConnect_Click(object sender, EventArgs e)
        {
            tbxSqlUser.ReadOnly = !tbxSqlUser.ReadOnly;
            tbxSqlPassword.ReadOnly = !tbxSqlPassword.ReadOnly;
            tbxSqlAddress.ReadOnly = !tbxSqlAddress.ReadOnly;
            nudSqlPort.ReadOnly = !nudSqlPort.ReadOnly;
            if (DatabaseCommunication.Active)
            {
                DatabaseCommunication.Disconnect();
                btnSqlConnect.Text = "Start Database Connection";
                tabSqlTest.Parent = null;
            }
            else
            {
                DatabaseCommunication.Connect(tbxSqlUser.Text, tbxSqlPassword.Text, tbxSqlAddress.Text, (int)nudSqlPort.Value);
                btnSqlConnect.Text = "Stop Database Connection";
                tabSqlTest.Parent = tabControl1;
            }
        }

        private void btnSqlCreate_Click(object sender, EventArgs e)
        {
            bool worked = DatabaseCommunication.CreateDatabase(tbxSqlUser.Text, tbxSqlPassword.Text, tbxSqlAddress.Text, (int)nudSqlPort.Value);
            if (worked)
            {
                tbxSql_ConnectionStringValuesChanged(null, null);
                toolStripStatusLabel1.Text = "SQL database created/repaired";
            }
            else
                WriteLog("Could not create the database.", Color.Red.ToArgb());
        }

        private void btnSqlUpdate_Click(object sender, EventArgs e)
        {
            bool worked = DatabaseCommunication.UpdateDatabase(tbxSqlUser.Text, tbxSqlPassword.Text, tbxSqlAddress.Text, (int)nudSqlPort.Value);
            if (worked)
            {
                tbxSql_ConnectionStringValuesChanged(null, null);
                toolStripStatusLabel1.Text = "SQL database Updated";
            }
            else
                WriteLog("Could not update the database.", Color.Red.ToArgb());
        }
        #endregion

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

        #region Database Test Commands
        private void CreateUser_Click(object sender, EventArgs e)
        {
            bool worked = DatabaseCommunication.CreateUser(textBox1.Text, textBox2.Text);
            if (worked)
                WriteLog("User created: " + textBox1.Text, Color.Blue.ToArgb());
        }

        private void ReadUser_Click(object sender, EventArgs e)
        {
            UserObj user = DatabaseCommunication.ReadUser((int)numericUpDown1.Value);
            if (user != null)
                WriteLog(user.ToString(), Color.Purple.ToArgb());
        }

        private void StatusUpdate_Click(object sender, EventArgs e)
        {
            bool worked = DatabaseCommunication.UpdateUser((int)numericUpDown1.Value, new System.Net.IPEndPoint(new System.Net.IPAddress(new byte[] { 10, 0, 0, 1} ), 9020));
            if (worked)
                WriteLog("Updated user: " + (int)numericUpDown1.Value, Color.Blue.ToArgb());
        }
        #endregion

        #region Options
        private void minimizeToTrayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            minimizeToTrayToolStripMenuItem.Checked = !minimizeToTrayToolStripMenuItem.Checked;
            //Properties.Settings.Default.setTray = minimizeToTrayToolStripMenuItem.Checked;
            //Properties.Settings.Default.Save();
        }

        private void showPasswordsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showPasswordsToolStripMenuItem.Checked = !showPasswordsToolStripMenuItem.Checked;
            tbxSqlPassword.UseSystemPasswordChar = !showPasswordsToolStripMenuItem.Checked;
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

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult reply = MessageBox.Show(this, "Are you sure you want to close the server?", "Close?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (reply == DialogResult.No)
                e.Cancel = true;
            else
            {
                // Add closing of sockets and stuff here!
                _server.Stop();
                DatabaseCommunication.Disconnect();
            }
        }
    }

    static class Global
    {
        public static FormMain MainWindow { set; get; }
    }
}
