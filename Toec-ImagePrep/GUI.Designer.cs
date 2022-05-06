namespace Toec_ImagePrep
{
    partial class GUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GUI));
            this.btnApply = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.chkDisableHibernate = new System.Windows.Forms.CheckBox();
            this.chkEnableBackground = new System.Windows.Forms.CheckBox();
            this.chkCreateSetupComplete = new System.Windows.Forms.CheckBox();
            this.chkRunSysprep = new System.Windows.Forms.CheckBox();
            this.chkResetToec = new System.Windows.Forms.CheckBox();
            this.tabGeneral = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.txtConnectOutput = new System.Windows.Forms.TextBox();
            this.ToemsConnect = new System.Windows.Forms.Button();
            this.General = new System.Windows.Forms.TabPage();
            this.chkRemoveRemoteAccess = new System.Windows.Forms.CheckBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.label3 = new System.Windows.Forms.Label();
            this.ddlSetupComplete = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtSetupComplete = new System.Windows.Forms.TextBox();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.txtSysprep = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.ddlSysprep = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtSysprepAnswerFile = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.checkedListBoxDrivers = new System.Windows.Forms.CheckedListBox();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.txtOutput = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tabGeneral.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.General.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnApply
            // 
            this.btnApply.BackColor = System.Drawing.SystemColors.WindowFrame;
            this.btnApply.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnApply.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnApply.Location = new System.Drawing.Point(469, 344);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(143, 36);
            this.btnApply.TabIndex = 0;
            this.btnApply.Text = "Run Image Prep";
            this.btnApply.UseVisualStyleBackColor = false;
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.BackColor = System.Drawing.SystemColors.WindowFrame;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnCancel.Location = new System.Drawing.Point(169, 344);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(143, 36);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // chkDisableHibernate
            // 
            this.chkDisableHibernate.AutoSize = true;
            this.chkDisableHibernate.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.chkDisableHibernate.Checked = true;
            this.chkDisableHibernate.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkDisableHibernate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkDisableHibernate.Location = new System.Drawing.Point(74, 29);
            this.chkDisableHibernate.Name = "chkDisableHibernate";
            this.chkDisableHibernate.Size = new System.Drawing.Size(115, 17);
            this.chkDisableHibernate.TabIndex = 2;
            this.chkDisableHibernate.Text = "Disable Hibernation";
            this.chkDisableHibernate.UseVisualStyleBackColor = false;
            // 
            // chkEnableBackground
            // 
            this.chkEnableBackground.AutoSize = true;
            this.chkEnableBackground.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.chkEnableBackground.Checked = true;
            this.chkEnableBackground.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkEnableBackground.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkEnableBackground.Location = new System.Drawing.Point(74, 61);
            this.chkEnableBackground.Name = "chkEnableBackground";
            this.chkEnableBackground.Size = new System.Drawing.Size(215, 17);
            this.chkEnableBackground.TabIndex = 3;
            this.chkEnableBackground.Text = "Enable WinLogon Finalizing Background";
            this.chkEnableBackground.UseVisualStyleBackColor = false;
            // 
            // chkCreateSetupComplete
            // 
            this.chkCreateSetupComplete.AutoSize = true;
            this.chkCreateSetupComplete.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.chkCreateSetupComplete.Checked = true;
            this.chkCreateSetupComplete.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkCreateSetupComplete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkCreateSetupComplete.Location = new System.Drawing.Point(74, 95);
            this.chkCreateSetupComplete.Name = "chkCreateSetupComplete";
            this.chkCreateSetupComplete.Size = new System.Drawing.Size(152, 17);
            this.chkCreateSetupComplete.TabIndex = 5;
            this.chkCreateSetupComplete.Text = "Create SetupComplete.cmd";
            this.chkCreateSetupComplete.UseVisualStyleBackColor = false;
            // 
            // chkRunSysprep
            // 
            this.chkRunSysprep.AutoSize = true;
            this.chkRunSysprep.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.chkRunSysprep.Checked = true;
            this.chkRunSysprep.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkRunSysprep.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkRunSysprep.Location = new System.Drawing.Point(74, 127);
            this.chkRunSysprep.Name = "chkRunSysprep";
            this.chkRunSysprep.Size = new System.Drawing.Size(84, 17);
            this.chkRunSysprep.TabIndex = 6;
            this.chkRunSysprep.Text = "Run Sysprep";
            this.chkRunSysprep.UseVisualStyleBackColor = false;
            // 
            // chkResetToec
            // 
            this.chkResetToec.AutoSize = true;
            this.chkResetToec.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.chkResetToec.Checked = true;
            this.chkResetToec.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkResetToec.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkResetToec.Location = new System.Drawing.Point(74, 162);
            this.chkResetToec.Name = "chkResetToec";
            this.chkResetToec.Size = new System.Drawing.Size(79, 17);
            this.chkResetToec.TabIndex = 7;
            this.chkResetToec.Text = "Reset Toec";
            this.chkResetToec.UseVisualStyleBackColor = false;
            // 
            // tabGeneral
            // 
            this.tabGeneral.Controls.Add(this.tabPage1);
            this.tabGeneral.Controls.Add(this.General);
            this.tabGeneral.Controls.Add(this.tabPage2);
            this.tabGeneral.Controls.Add(this.tabPage3);
            this.tabGeneral.Controls.Add(this.tabPage4);
            this.tabGeneral.Location = new System.Drawing.Point(28, 11);
            this.tabGeneral.Name = "tabGeneral";
            this.tabGeneral.Padding = new System.Drawing.Point(6, 6);
            this.tabGeneral.SelectedIndex = 0;
            this.tabGeneral.Size = new System.Drawing.Size(741, 326);
            this.tabGeneral.TabIndex = 8;
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.Color.DimGray;
            this.tabPage1.Controls.Add(this.txtConnectOutput);
            this.tabPage1.Controls.Add(this.ToemsConnect);
            this.tabPage1.Location = new System.Drawing.Point(4, 28);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage1.Size = new System.Drawing.Size(733, 294);
            this.tabPage1.TabIndex = 3;
            this.tabPage1.Text = "Server Connection";
            // 
            // txtConnectOutput
            // 
            this.txtConnectOutput.Location = new System.Drawing.Point(123, 105);
            this.txtConnectOutput.Margin = new System.Windows.Forms.Padding(2);
            this.txtConnectOutput.Multiline = true;
            this.txtConnectOutput.Name = "txtConnectOutput";
            this.txtConnectOutput.ScrollBars = System.Windows.Forms.ScrollBars.Horizontal;
            this.txtConnectOutput.Size = new System.Drawing.Size(500, 153);
            this.txtConnectOutput.TabIndex = 1;
            // 
            // ToemsConnect
            // 
            this.ToemsConnect.Location = new System.Drawing.Point(248, 36);
            this.ToemsConnect.Margin = new System.Windows.Forms.Padding(2);
            this.ToemsConnect.Name = "ToemsConnect";
            this.ToemsConnect.Size = new System.Drawing.Size(218, 38);
            this.ToemsConnect.TabIndex = 0;
            this.ToemsConnect.Text = "Connect To Toems";
            this.ToemsConnect.UseVisualStyleBackColor = true;
            this.ToemsConnect.Click += new System.EventHandler(this.ToemsConnect_Click);
            // 
            // General
            // 
            this.General.BackColor = System.Drawing.Color.DimGray;
            this.General.Controls.Add(this.chkRemoveRemoteAccess);
            this.General.Controls.Add(this.chkEnableBackground);
            this.General.Controls.Add(this.chkResetToec);
            this.General.Controls.Add(this.chkDisableHibernate);
            this.General.Controls.Add(this.chkRunSysprep);
            this.General.Controls.Add(this.chkCreateSetupComplete);
            this.General.Location = new System.Drawing.Point(4, 28);
            this.General.Name = "General";
            this.General.Padding = new System.Windows.Forms.Padding(3);
            this.General.Size = new System.Drawing.Size(733, 294);
            this.General.TabIndex = 0;
            this.General.Text = "General";
            // 
            // chkRemoveRemoteAccess
            // 
            this.chkRemoveRemoteAccess.AutoSize = true;
            this.chkRemoveRemoteAccess.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.chkRemoveRemoteAccess.Checked = true;
            this.chkRemoveRemoteAccess.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkRemoveRemoteAccess.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkRemoveRemoteAccess.Location = new System.Drawing.Point(74, 195);
            this.chkRemoveRemoteAccess.Name = "chkRemoveRemoteAccess";
            this.chkRemoveRemoteAccess.Size = new System.Drawing.Size(170, 17);
            this.chkRemoveRemoteAccess.TabIndex = 8;
            this.chkRemoveRemoteAccess.Text = "Uninstall Remote Access Client";
            this.chkRemoveRemoteAccess.UseVisualStyleBackColor = false;
            // 
            // tabPage2
            // 
            this.tabPage2.BackColor = System.Drawing.Color.DimGray;
            this.tabPage2.Controls.Add(this.label3);
            this.tabPage2.Controls.Add(this.ddlSetupComplete);
            this.tabPage2.Controls.Add(this.label1);
            this.tabPage2.Controls.Add(this.txtSetupComplete);
            this.tabPage2.Location = new System.Drawing.Point(4, 28);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(733, 294);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "SetupComplete.cmd";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(33, 11);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(93, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Server Templates:";
            // 
            // ddlSetupComplete
            // 
            this.ddlSetupComplete.BackColor = System.Drawing.SystemColors.WindowFrame;
            this.ddlSetupComplete.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddlSetupComplete.ForeColor = System.Drawing.SystemColors.ScrollBar;
            this.ddlSetupComplete.FormattingEnabled = true;
            this.ddlSetupComplete.Location = new System.Drawing.Point(138, 9);
            this.ddlSetupComplete.Margin = new System.Windows.Forms.Padding(2);
            this.ddlSetupComplete.Name = "ddlSetupComplete";
            this.ddlSetupComplete.Size = new System.Drawing.Size(184, 21);
            this.ddlSetupComplete.TabIndex = 2;
            this.ddlSetupComplete.SelectedIndexChanged += new System.EventHandler(this.ddlSetupComplete_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(31, 66);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(177, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "SetupComplete.cmd Script Contents";
            // 
            // txtSetupComplete
            // 
            this.txtSetupComplete.Location = new System.Drawing.Point(33, 82);
            this.txtSetupComplete.Multiline = true;
            this.txtSetupComplete.Name = "txtSetupComplete";
            this.txtSetupComplete.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtSetupComplete.Size = new System.Drawing.Size(679, 210);
            this.txtSetupComplete.TabIndex = 0;
            // 
            // tabPage3
            // 
            this.tabPage3.BackColor = System.Drawing.Color.DimGray;
            this.tabPage3.Controls.Add(this.txtSysprep);
            this.tabPage3.Controls.Add(this.label5);
            this.tabPage3.Controls.Add(this.ddlSysprep);
            this.tabPage3.Controls.Add(this.label4);
            this.tabPage3.Controls.Add(this.txtSysprepAnswerFile);
            this.tabPage3.Controls.Add(this.button1);
            this.tabPage3.Location = new System.Drawing.Point(4, 28);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(733, 294);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Sysprep";
            // 
            // txtSysprep
            // 
            this.txtSysprep.Location = new System.Drawing.Point(71, 164);
            this.txtSysprep.Multiline = true;
            this.txtSysprep.Name = "txtSysprep";
            this.txtSysprep.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtSysprep.Size = new System.Drawing.Size(615, 124);
            this.txtSysprep.TabIndex = 7;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.75F);
            this.label5.Location = new System.Drawing.Point(66, 19);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(609, 25);
            this.label5.TabIndex = 6;
            this.label5.Text = "Select A Local Sysprep Answer File Or A Template From The Server";
            // 
            // ddlSysprep
            // 
            this.ddlSysprep.BackColor = System.Drawing.SystemColors.WindowFrame;
            this.ddlSysprep.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddlSysprep.ForeColor = System.Drawing.SystemColors.ScrollBar;
            this.ddlSysprep.FormattingEnabled = true;
            this.ddlSysprep.Location = new System.Drawing.Point(71, 138);
            this.ddlSysprep.Margin = new System.Windows.Forms.Padding(2);
            this.ddlSysprep.Name = "ddlSysprep";
            this.ddlSysprep.Size = new System.Drawing.Size(184, 21);
            this.ddlSysprep.TabIndex = 5;
            this.ddlSysprep.SelectedIndexChanged += new System.EventHandler(this.ddlSysprep_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(68, 112);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(93, 13);
            this.label4.TabIndex = 4;
            this.label4.Text = "Server Templates:";
            // 
            // txtSysprepAnswerFile
            // 
            this.txtSysprepAnswerFile.Location = new System.Drawing.Point(71, 76);
            this.txtSysprepAnswerFile.Name = "txtSysprepAnswerFile";
            this.txtSysprepAnswerFile.Size = new System.Drawing.Size(475, 20);
            this.txtSysprepAnswerFile.TabIndex = 1;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(71, 47);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(174, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "Select Local Answer File";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // tabPage4
            // 
            this.tabPage4.BackColor = System.Drawing.Color.DimGray;
            this.tabPage4.Controls.Add(this.checkedListBoxDrivers);
            this.tabPage4.Location = new System.Drawing.Point(4, 28);
            this.tabPage4.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage4.Size = new System.Drawing.Size(733, 294);
            this.tabPage4.TabIndex = 4;
            this.tabPage4.Text = "Additional Drivers";
            // 
            // checkedListBoxDrivers
            // 
            this.checkedListBoxDrivers.FormattingEnabled = true;
            this.checkedListBoxDrivers.Location = new System.Drawing.Point(124, 38);
            this.checkedListBoxDrivers.Margin = new System.Windows.Forms.Padding(2);
            this.checkedListBoxDrivers.Name = "checkedListBoxDrivers";
            this.checkedListBoxDrivers.ScrollAlwaysVisible = true;
            this.checkedListBoxDrivers.Size = new System.Drawing.Size(471, 229);
            this.checkedListBoxDrivers.TabIndex = 3;
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // txtOutput
            // 
            this.txtOutput.BackColor = System.Drawing.SystemColors.Menu;
            this.txtOutput.Location = new System.Drawing.Point(33, 405);
            this.txtOutput.Multiline = true;
            this.txtOutput.Name = "txtOutput";
            this.txtOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtOutput.Size = new System.Drawing.Size(737, 180);
            this.txtOutput.TabIndex = 9;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(45, 388);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(63, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "Log Output:";
            // 
            // GUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.DimGray;
            this.ClientSize = new System.Drawing.Size(792, 595);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtOutput);
            this.Controls.Add(this.tabGeneral);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnApply);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "GUI";
            this.Text = "Toec Image Prep";
            this.tabGeneral.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.General.ResumeLayout(false);
            this.General.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.tabPage4.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.CheckBox chkDisableHibernate;
        private System.Windows.Forms.CheckBox chkEnableBackground;
        private System.Windows.Forms.CheckBox chkCreateSetupComplete;
        private System.Windows.Forms.CheckBox chkRunSysprep;
        private System.Windows.Forms.CheckBox chkResetToec;
        private System.Windows.Forms.TabControl tabGeneral;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage3;
        public System.Windows.Forms.TabPage General;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtSetupComplete;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.TextBox txtSysprepAnswerFile;
        private System.Windows.Forms.TextBox txtOutput;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox chkRemoveRemoteAccess;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TextBox txtConnectOutput;
        private System.Windows.Forms.Button ToemsConnect;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.CheckedListBox checkedListBoxDrivers;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox ddlSetupComplete;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox ddlSysprep;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtSysprep;
    }
}