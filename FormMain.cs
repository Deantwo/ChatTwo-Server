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
            _server.PacketReceived += ChatTwo_Server_Protocol.MessageReceivedHandler;
            ChatTwo_Server_Protocol.MessageTransmission += _server.SendPacket;
            _server.EtherConnectionReply += EtherConnectReply;

            notifyIcon1.BalloonTipTitle = this.Text;
            notifyIcon1.Text = this.Text;
            notifyIcon1.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

            // Steal the system "informaion icon" and resize it.
            _infoTip = SystemIcons.Information.ToBitmap();
            _infoTip = (Image)(new Bitmap(_infoTip, new Size(12, 12)));

            tabSqlTest.Parent = null;
        }

        private void quickStartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!DatabaseCommunication.Active)
            {
                btnSqlTest_Click(null, null);
                //if (btnSqlCreate.Enabled)
                //{
                //    btnSqlCreate_Click(null, null);
                //    btnSqlTest_Click(null, null);
                //}
                if (btnSqlConnect.Enabled)
                    btnSqlConnect_Click(null, null);
            }
            if (!_server.Active)
            {
                if (btnIpConnect.Enabled)
                    btnIpConnect_Click(null, null);
            }
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
            btnSqlTest.Enabled = false;

            // Basic user feedback.
            toolStripStatusLabel1.Text = "Testing connection and access to the database...";
            statusStrip1.Refresh(); // Has to be done or the statusStrip won't display the new toolStripStatusLabel text.

            // Test the connection and access, giving the user feedback if it fails.
            DatabaseCommunication.ConnectionTestResult dbTest = DatabaseCommunication.TestConnection(tbxSqlUser.Text, tbxSqlPassword.Text, tbxSqlAddress.Text, (int)nudSqlPort.Value);
            
            // Check the result of the connetion test.
            bool connectTestSuccessful = dbTest == DatabaseCommunication.ConnectionTestResult.Successful;
            if (connectTestSuccessful)
            {
                toolStripStatusLabel1.Text = "Database connection test: successful";
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
                        errorMessage = "One or more of the tables do not exist";
                        errorTip = "." + Environment.NewLine + Environment.NewLine +
                            "You can create/repair the database by clicking the button below.";
                        btnSqlCreate.Enabled = true;
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
                if (sender != null)
                    MessageBox.Show(errorMessage + errorTip, "SQL Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                toolStripStatusLabel1.Text = "Database connection test: failed. " + errorMessage;
                lblSqlConnection.Text = "Test: failed";
                lblSqlConnection.Image = _infoTip;
                toolTip1.SetToolTip(lblSqlConnection, errorMessage + errorTip);
            }

            btnSqlTest.Enabled = true;
        }

        private void btnSqlConnect_Click(object sender, EventArgs e)
        {
            tbxSqlUser.ReadOnly = !tbxSqlUser.ReadOnly;
            tbxSqlPassword.ReadOnly = !tbxSqlPassword.ReadOnly;
            tbxSqlAddress.ReadOnly = !tbxSqlAddress.ReadOnly;
            nudSqlPort.Enabled = !nudSqlPort.Enabled;
            btnSqlTest.Enabled = !btnSqlTest.Enabled;
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
            UpdateServerStatus();
        }

        private void btnSqlCreate_Click(object sender, EventArgs e)
        {
            bool worked = DatabaseCommunication.CreateDatabase(tbxSqlUser.Text, tbxSqlPassword.Text, tbxSqlAddress.Text, (int)nudSqlPort.Value);
            if (worked)
            {
                tbxSql_ConnectionStringValuesChanged(null, null);
                toolStripStatusLabel1.Text = "SQL database created/repaired";
                btnSqlTest_Click(null, null);
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
                btnSqlTest_Click(null, null);
            }
            else
                WriteLog("Could not update the database.", Color.Red.ToArgb());
        }
        #endregion

        #region IP Setup
        private void chxIp_CheckedChanged(object sender, EventArgs e)
        {
            btnIpConnect.Enabled = chxIpUdp.Checked || chxIpTcp.Checked;
        }

        private void btnIpConnect_Click(object sender, EventArgs e)
        {
            nudIpPort.Enabled = !nudIpPort.Enabled;
            btnIpTest.Enabled = !btnIpTest.Enabled;
            chxIpUdp.Enabled = !chxIpUdp.Enabled;
            //chxIpTcp.Enabled = !chxIpTcp.Enabled; // Not implemented yet.
            if (_server.Active)
            {
                _server.Stop();
                btnIpConnect.Text = "Start IP Connection";
            }
            else
            {
                _server.Start((int)nudIpPort.Value);
                btnIpConnect.Text = "Stop IP Connection";
            }
            UpdateServerStatus();
        }

        private void tbxIp_ExternalIpValuesChanged(object sender, EventArgs e)
        {
            lblIpConnection.Text = "Test: -";
            lblIpConnection.Image = null;
        }

        private void btnIpTest_Click(object sender, EventArgs e)
        {
            tbxIp_ExternalIpValuesChanged(null, null);
            btnIpTest.Enabled = false;

            if (chxIpUdp.Checked)
            {
                // Basic user feedback.
                toolStripStatusLabel1.Text = "Testing UDP port-forward...";
                statusStrip1.Refresh(); // Has to be done or the statusStrip won't display the new toolStripStatusLabel text.
                
                string errorMessage;
                string errorTip;
                System.Net.IPAddress address;
                if (System.Net.IPAddress.TryParse(tbxIpExternalAddress.Text, out address))
                {
                    // Check the result of the port-forward test.
                    bool portforwardTestStartSuccessful = UdpCommunication.TestPortforward(new System.Net.IPEndPoint(address, (int)nudIpExternalPort.Value));
                    if (portforwardTestStartSuccessful)
                    {
                        lblIpConnection.Text = "Test: testing...";
                        toolTip1.SetToolTip(lblIpConnection, "");
                        timer1.Start();
                        return;
                    }
                    else
                    {
                        errorMessage = "The UDP port-forward test failed";
                        errorTip = "." + Environment.NewLine + Environment.NewLine +
                            "Could not start the test. Please ensure you have an internet connection.";
                        lblIpConnection.Text = "Test: failed";
                    }
                }
                else
                {
                    errorMessage = "The entered external IP address is invalid";
                    errorTip = "." + Environment.NewLine + Environment.NewLine +
                        "Please enter a valid IP address." + Environment.NewLine +
                        "If you don't know your external IP address, you can get it by googling \"what is my IP?\".";
                    lblIpConnection.Text = "Test: invalid IP address";
                }

                MessageBox.Show(errorMessage + errorTip, "Port-Forward Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                toolStripStatusLabel1.Text = "UDP port-forward test: failed. " + errorMessage;
                lblIpConnection.Image = _infoTip;
                toolTip1.SetToolTip(lblIpConnection, errorMessage + errorTip);
            }
            if (chxIpTcp.Checked)
            {
                // Basic user feedback.
                toolStripStatusLabel1.Text = "Testing TCP port-forward...";
                statusStrip1.Refresh(); // Has to be done or the statusStrip won't display the new toolStripStatusLabel text.

                throw new NotImplementedException("TCP is not implemented yet.");
            }

            btnIpTest.Enabled = true;
        }

        public void EtherConnectReply(object sender, EventArgs args)
        {
            if (lblIpConnection.InvokeRequired)
            { // Needed for multi-threading cross calls.
                this.Invoke(new Action<object, EventArgs>(this.EtherConnectReply), new object[] { sender, args });
            }
            else
            {
                if (timer1.Enabled)
                {
                    timer1.Stop();
                    toolStripStatusLabel1.Text = "UDP port-forward test: successful";
                    lblIpConnection.Text = "Test: successful";
                    toolTip1.SetToolTip(lblIpConnection, "");
                    btnIpTest.Enabled = true;
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (timer1.Enabled)
            {
                timer1.Stop();
                string errorMessage;
                string errorTip;

                errorMessage = "The UDP port-forward test failed";
                errorTip = "." + Environment.NewLine + Environment.NewLine +
                    "Please ensure your router is correctly configured.";
                lblIpConnection.Text = "Test: failed";

                MessageBox.Show(errorMessage + errorTip, "Port-Forward Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                toolStripStatusLabel1.Text = "UDP port-forward test: failed. " + errorMessage;
                lblIpConnection.Image = _infoTip;
                toolTip1.SetToolTip(lblIpConnection, errorMessage + errorTip);
                btnIpTest.Enabled = true;
            }
        }
        #endregion

        private void UpdateServerStatus()
        {
            lblServerStatus.Text = "Server Status: ";
            if (!_server.Active)
                lblServerStatus.Text += "Offline";
            else if (!DatabaseCommunication.Active)
                lblServerStatus.Text += "No database connection";
            else
                lblServerStatus.Text += "Online";
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

        #region Database Test Commands
        private void CreateUser_Click(object sender, EventArgs e)
        {
            string hashedPassword = ByteHelper.GetHashString(Encoding.Unicode.GetBytes(textBox2.Text));
            bool worked = DatabaseCommunication.CreateUser(textBox1.Text, hashedPassword);
            if (worked)
                WriteLog("user[\"" + textBox1.Text + "\"] Was created successfully.", Color.Blue.ToArgb());
            else
                WriteLog("user[\"" + textBox1.Text + "\"] Was not created.", Color.Red.ToArgb());
        }

        private void ReadUser_Click(object sender, EventArgs e)
        {
            UserObj user = DatabaseCommunication.ReadUser((int)numericUpDown1.Value);
            if (user != null)
                WriteLog(user.ToString(), Color.Purple.ToArgb());
            else
                WriteLog("user[" + (int)numericUpDown1.Value + "] Does not exist.", Color.Red.ToArgb());
        }

        private void StatusUpdate_Click(object sender, EventArgs e)
        {
            bool worked = DatabaseCommunication.UpdateUser((int)numericUpDown1.Value, new System.Net.IPEndPoint(new System.Net.IPAddress(new byte[] { 10, 0, 0, 1} ), 9020));
            if (worked)
                WriteLog("user[" + (int)numericUpDown1.Value + "] Changed to online.", Color.Blue.ToArgb());
            else
                WriteLog("user[" + (int)numericUpDown1.Value + "] Does not exist.", Color.Red.ToArgb());
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
                    TrayBalloonTip("Minimized to tray", ToolTipIcon.None);
                    this.ShowInTaskbar = false;
                }
                else if (FormWindowState.Normal == this.WindowState)
                {
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
