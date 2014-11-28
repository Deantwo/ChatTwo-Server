namespace ChatTwo_Server
{
    partial class FormMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.restoreToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.closeToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.minimizeToTrayToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showPasswordsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.rtbxLog = new System.Windows.Forms.RichTextBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.button1 = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.button3 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.groupBoxSql = new System.Windows.Forms.GroupBox();
            this.btnSqlUpdate = new System.Windows.Forms.Button();
            this.btnSqlConnect = new System.Windows.Forms.Button();
            this.btnSqlCreate = new System.Windows.Forms.Button();
            this.nudSqlPort = new System.Windows.Forms.NumericUpDown();
            this.lblSqlConnection = new System.Windows.Forms.Label();
            this.btnSqlTest = new System.Windows.Forms.Button();
            this.lblSqlPort = new System.Windows.Forms.Label();
            this.lblSqlUser = new System.Windows.Forms.Label();
            this.lblSqlPassword = new System.Windows.Forms.Label();
            this.tbxSqlUser = new System.Windows.Forms.TextBox();
            this.lblSqlServer = new System.Windows.Forms.Label();
            this.tbxSqlPassword = new System.Windows.Forms.TextBox();
            this.tbxSqlAddress = new System.Windows.Forms.TextBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.contextMenuStrip1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.tabPage3.SuspendLayout();
            this.groupBoxSql.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudSqlPort)).BeginInit();
            this.SuspendLayout();
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.BalloonTipText = "notifyIcon1";
            this.notifyIcon1.BalloonTipTitle = "notifyIcon1";
            this.notifyIcon1.ContextMenuStrip = this.contextMenuStrip1;
            this.notifyIcon1.Text = "notifyIcon1";
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon1_MouseDoubleClick);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.restoreToolStripMenuItem,
            this.toolStripSeparator3,
            this.closeToolStripMenuItem1});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(114, 54);
            // 
            // restoreToolStripMenuItem
            // 
            this.restoreToolStripMenuItem.Name = "restoreToolStripMenuItem";
            this.restoreToolStripMenuItem.Size = new System.Drawing.Size(113, 22);
            this.restoreToolStripMenuItem.Text = "Restore";
            this.restoreToolStripMenuItem.Click += new System.EventHandler(this.RestoreToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(110, 6);
            // 
            // closeToolStripMenuItem1
            // 
            this.closeToolStripMenuItem1.Name = "closeToolStripMenuItem1";
            this.closeToolStripMenuItem1.Size = new System.Drawing.Size(113, 22);
            this.closeToolStripMenuItem1.Text = "Close";
            this.closeToolStripMenuItem1.Click += new System.EventHandler(this.close_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.optionsToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(309, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSeparator1,
            this.closeToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(100, 6);
            // 
            // closeToolStripMenuItem
            // 
            this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            this.closeToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this.closeToolStripMenuItem.Text = "Close";
            this.closeToolStripMenuItem.Click += new System.EventHandler(this.close_Click);
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.minimizeToTrayToolStripMenuItem,
            this.showPasswordsToolStripMenuItem});
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.optionsToolStripMenuItem.Text = "Options";
            // 
            // minimizeToTrayToolStripMenuItem
            // 
            this.minimizeToTrayToolStripMenuItem.Checked = true;
            this.minimizeToTrayToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.minimizeToTrayToolStripMenuItem.Name = "minimizeToTrayToolStripMenuItem";
            this.minimizeToTrayToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.minimizeToTrayToolStripMenuItem.Text = "Minimize To Tray";
            this.minimizeToTrayToolStripMenuItem.Click += new System.EventHandler(this.minimizeToTrayToolStripMenuItem_Click);
            // 
            // showPasswordsToolStripMenuItem
            // 
            this.showPasswordsToolStripMenuItem.Name = "showPasswordsToolStripMenuItem";
            this.showPasswordsToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.showPasswordsToolStripMenuItem.Text = "Show Passwords";
            this.showPasswordsToolStripMenuItem.Click += new System.EventHandler(this.showPasswordsToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 338);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(309, 24);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.toolStripStatusLabel1.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenOuter;
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(294, 19);
            this.toolStripStatusLabel1.Spring = true;
            this.toolStripStatusLabel1.Text = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 24);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(309, 314);
            this.tabControl1.TabIndex = 10;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.rtbxLog);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(301, 288);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Log";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // rtbxLog
            // 
            this.rtbxLog.BackColor = System.Drawing.SystemColors.Window;
            this.rtbxLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbxLog.Font = new System.Drawing.Font("Lucida Console", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rtbxLog.Location = new System.Drawing.Point(3, 3);
            this.rtbxLog.Name = "rtbxLog";
            this.rtbxLog.ReadOnly = true;
            this.rtbxLog.Size = new System.Drawing.Size(295, 282);
            this.rtbxLog.TabIndex = 1;
            this.rtbxLog.Text = "";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.button1);
            this.tabPage2.Controls.Add(this.textBox1);
            this.tabPage2.Controls.Add(this.button3);
            this.tabPage2.Controls.Add(this.button2);
            this.tabPage2.Controls.Add(this.numericUpDown1);
            this.tabPage2.Controls.Add(this.textBox2);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(301, 288);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Manual Management";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(6, 6);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 3;
            this.button1.Text = "CreateUser";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.CreateUser_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(87, 8);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(100, 20);
            this.textBox1.TabIndex = 4;
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(6, 64);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 8;
            this.button3.Text = "StatusUpdate";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.StatusUpdate_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(6, 35);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 5;
            this.button2.Text = "ReadUser";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.ReadUser_Click);
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Location = new System.Drawing.Point(87, 49);
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(52, 20);
            this.numericUpDown1.TabIndex = 7;
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(193, 8);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(100, 20);
            this.textBox2.TabIndex = 6;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.groupBoxSql);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(301, 288);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Setup";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // groupBoxSql
            // 
            this.groupBoxSql.Controls.Add(this.btnSqlUpdate);
            this.groupBoxSql.Controls.Add(this.btnSqlConnect);
            this.groupBoxSql.Controls.Add(this.btnSqlCreate);
            this.groupBoxSql.Controls.Add(this.nudSqlPort);
            this.groupBoxSql.Controls.Add(this.lblSqlConnection);
            this.groupBoxSql.Controls.Add(this.btnSqlTest);
            this.groupBoxSql.Controls.Add(this.lblSqlPort);
            this.groupBoxSql.Controls.Add(this.lblSqlUser);
            this.groupBoxSql.Controls.Add(this.lblSqlPassword);
            this.groupBoxSql.Controls.Add(this.tbxSqlUser);
            this.groupBoxSql.Controls.Add(this.lblSqlServer);
            this.groupBoxSql.Controls.Add(this.tbxSqlPassword);
            this.groupBoxSql.Controls.Add(this.tbxSqlAddress);
            this.groupBoxSql.Location = new System.Drawing.Point(8, 6);
            this.groupBoxSql.Name = "groupBoxSql";
            this.groupBoxSql.Size = new System.Drawing.Size(218, 213);
            this.groupBoxSql.TabIndex = 5;
            this.groupBoxSql.TabStop = false;
            this.groupBoxSql.Text = "Database Settings";
            // 
            // btnSqlUpdate
            // 
            this.btnSqlUpdate.Enabled = false;
            this.btnSqlUpdate.Location = new System.Drawing.Point(6, 184);
            this.btnSqlUpdate.Name = "btnSqlUpdate";
            this.btnSqlUpdate.Size = new System.Drawing.Size(206, 23);
            this.btnSqlUpdate.TabIndex = 6;
            this.btnSqlUpdate.Text = "Update Database";
            this.btnSqlUpdate.UseVisualStyleBackColor = true;
            this.btnSqlUpdate.Click += new System.EventHandler(this.btnSqlUpdate_Click);
            // 
            // btnSqlConnect
            // 
            this.btnSqlConnect.Enabled = false;
            this.btnSqlConnect.Location = new System.Drawing.Point(6, 126);
            this.btnSqlConnect.Name = "btnSqlConnect";
            this.btnSqlConnect.Size = new System.Drawing.Size(206, 23);
            this.btnSqlConnect.TabIndex = 6;
            this.btnSqlConnect.Text = "Start Database Connection";
            this.btnSqlConnect.UseVisualStyleBackColor = true;
            this.btnSqlConnect.Click += new System.EventHandler(this.btnSqlConnect_Click);
            // 
            // btnSqlCreate
            // 
            this.btnSqlCreate.Enabled = false;
            this.btnSqlCreate.Location = new System.Drawing.Point(6, 155);
            this.btnSqlCreate.Name = "btnSqlCreate";
            this.btnSqlCreate.Size = new System.Drawing.Size(206, 23);
            this.btnSqlCreate.TabIndex = 6;
            this.btnSqlCreate.Text = "Create/Repair Database";
            this.btnSqlCreate.UseVisualStyleBackColor = true;
            this.btnSqlCreate.Click += new System.EventHandler(this.btnSqlCreate_Click);
            // 
            // nudSqlPort
            // 
            this.nudSqlPort.Location = new System.Drawing.Point(155, 72);
            this.nudSqlPort.Maximum = new decimal(new int[] {
            49151,
            0,
            0,
            0});
            this.nudSqlPort.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudSqlPort.Name = "nudSqlPort";
            this.nudSqlPort.Size = new System.Drawing.Size(57, 20);
            this.nudSqlPort.TabIndex = 10;
            this.nudSqlPort.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.nudSqlPort.Value = new decimal(new int[] {
            3306,
            0,
            0,
            0});
            this.nudSqlPort.ValueChanged += new System.EventHandler(this.tbxSql_ConnectionStringValuesChanged);
            // 
            // lblSqlConnection
            // 
            this.lblSqlConnection.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblSqlConnection.Location = new System.Drawing.Point(112, 94);
            this.lblSqlConnection.Name = "lblSqlConnection";
            this.lblSqlConnection.Size = new System.Drawing.Size(100, 29);
            this.lblSqlConnection.TabIndex = 4;
            this.lblSqlConnection.Text = "Test: -";
            this.lblSqlConnection.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnSqlTest
            // 
            this.btnSqlTest.Location = new System.Drawing.Point(6, 97);
            this.btnSqlTest.Name = "btnSqlTest";
            this.btnSqlTest.Size = new System.Drawing.Size(100, 23);
            this.btnSqlTest.TabIndex = 3;
            this.btnSqlTest.Text = "Test Connection";
            this.btnSqlTest.UseVisualStyleBackColor = true;
            this.btnSqlTest.Click += new System.EventHandler(this.btnSqlTest_Click);
            // 
            // lblSqlPort
            // 
            this.lblSqlPort.AutoSize = true;
            this.lblSqlPort.Location = new System.Drawing.Point(152, 55);
            this.lblSqlPort.Name = "lblSqlPort";
            this.lblSqlPort.Size = new System.Drawing.Size(29, 13);
            this.lblSqlPort.TabIndex = 3;
            this.lblSqlPort.Text = "Port:";
            // 
            // lblSqlUser
            // 
            this.lblSqlUser.AutoSize = true;
            this.lblSqlUser.Location = new System.Drawing.Point(6, 16);
            this.lblSqlUser.Name = "lblSqlUser";
            this.lblSqlUser.Size = new System.Drawing.Size(29, 13);
            this.lblSqlUser.TabIndex = 2;
            this.lblSqlUser.Text = "User";
            // 
            // lblSqlPassword
            // 
            this.lblSqlPassword.AutoSize = true;
            this.lblSqlPassword.Location = new System.Drawing.Point(109, 16);
            this.lblSqlPassword.Name = "lblSqlPassword";
            this.lblSqlPassword.Size = new System.Drawing.Size(53, 13);
            this.lblSqlPassword.TabIndex = 2;
            this.lblSqlPassword.Text = "Password";
            // 
            // tbxSqlUser
            // 
            this.tbxSqlUser.Location = new System.Drawing.Point(6, 32);
            this.tbxSqlUser.Name = "tbxSqlUser";
            this.tbxSqlUser.Size = new System.Drawing.Size(100, 20);
            this.tbxSqlUser.TabIndex = 1;
            this.tbxSqlUser.Text = "root";
            this.tbxSqlUser.TextChanged += new System.EventHandler(this.tbxSql_ConnectionStringValuesChanged);
            // 
            // lblSqlServer
            // 
            this.lblSqlServer.AutoSize = true;
            this.lblSqlServer.Location = new System.Drawing.Point(6, 55);
            this.lblSqlServer.Name = "lblSqlServer";
            this.lblSqlServer.Size = new System.Drawing.Size(79, 13);
            this.lblSqlServer.TabIndex = 2;
            this.lblSqlServer.Text = "Server Address";
            // 
            // tbxSqlPassword
            // 
            this.tbxSqlPassword.Location = new System.Drawing.Point(112, 32);
            this.tbxSqlPassword.Name = "tbxSqlPassword";
            this.tbxSqlPassword.Size = new System.Drawing.Size(100, 20);
            this.tbxSqlPassword.TabIndex = 1;
            this.tbxSqlPassword.UseSystemPasswordChar = true;
            this.tbxSqlPassword.TextChanged += new System.EventHandler(this.tbxSql_ConnectionStringValuesChanged);
            // 
            // tbxSqlAddress
            // 
            this.tbxSqlAddress.Location = new System.Drawing.Point(6, 71);
            this.tbxSqlAddress.Name = "tbxSqlAddress";
            this.tbxSqlAddress.Size = new System.Drawing.Size(143, 20);
            this.tbxSqlAddress.TabIndex = 1;
            this.tbxSqlAddress.Text = "localhost";
            this.tbxSqlAddress.TextChanged += new System.EventHandler(this.tbxSql_ConnectionStringValuesChanged);
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(309, 362);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.MinimumSize = new System.Drawing.Size(325, 400);
            this.Name = "FormMain";
            this.Text = "ChatTwo Server";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Resize += new System.EventHandler(this.FormMain_Resize);
            this.contextMenuStrip1.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.tabPage3.ResumeLayout(false);
            this.groupBoxSql.ResumeLayout(false);
            this.groupBoxSql.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudSqlPort)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem minimizeToTrayToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem restoreToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.RichTextBox rtbxLog;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.GroupBox groupBoxSql;
        private System.Windows.Forms.Button btnSqlCreate;
        private System.Windows.Forms.Button btnSqlConnect;
        private System.Windows.Forms.NumericUpDown nudSqlPort;
        private System.Windows.Forms.Label lblSqlConnection;
        private System.Windows.Forms.Button btnSqlTest;
        private System.Windows.Forms.Label lblSqlPort;
        private System.Windows.Forms.Label lblSqlUser;
        private System.Windows.Forms.Label lblSqlPassword;
        private System.Windows.Forms.TextBox tbxSqlUser;
        private System.Windows.Forms.Label lblSqlServer;
        private System.Windows.Forms.TextBox tbxSqlPassword;
        private System.Windows.Forms.TextBox tbxSqlAddress;
        private System.Windows.Forms.Button btnSqlUpdate;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ToolStripMenuItem showPasswordsToolStripMenuItem;
    }
}

