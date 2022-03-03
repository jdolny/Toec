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
            this.chkDriversReg = new System.Windows.Forms.CheckBox();
            this.chkCreateSetupComplete = new System.Windows.Forms.CheckBox();
            this.chkRunSysprep = new System.Windows.Forms.CheckBox();
            this.chkResetToec = new System.Windows.Forms.CheckBox();
            this.tabGeneral = new System.Windows.Forms.TabControl();
            this.General = new System.Windows.Forms.TabPage();
            this.chkRemoveRemoteAccess = new System.Windows.Forms.CheckBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.label1 = new System.Windows.Forms.Label();
            this.txtSetupComplete = new System.Windows.Forms.TextBox();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.txtSysprepAnswerFile = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.txtOutput = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tabGeneral.SuspendLayout();
            this.General.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
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
            this.chkEnableBackground.Location = new System.Drawing.Point(74, 104);
            this.chkEnableBackground.Name = "chkEnableBackground";
            this.chkEnableBackground.Size = new System.Drawing.Size(215, 17);
            this.chkEnableBackground.TabIndex = 3;
            this.chkEnableBackground.Text = "Enable WinLogon Finalizing Background";
            this.chkEnableBackground.UseVisualStyleBackColor = false;
            // 
            // chkDriversReg
            // 
            this.chkDriversReg.AutoSize = true;
            this.chkDriversReg.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.chkDriversReg.Checked = true;
            this.chkDriversReg.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkDriversReg.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkDriversReg.Location = new System.Drawing.Point(74, 63);
            this.chkDriversReg.Name = "chkDriversReg";
            this.chkDriversReg.Size = new System.Drawing.Size(194, 17);
            this.chkDriversReg.TabIndex = 4;
            this.chkDriversReg.Text = "Add C:\\Drivers Location To Registry";
            this.chkDriversReg.UseVisualStyleBackColor = false;
            // 
            // chkCreateSetupComplete
            // 
            this.chkCreateSetupComplete.AutoSize = true;
            this.chkCreateSetupComplete.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.chkCreateSetupComplete.Checked = true;
            this.chkCreateSetupComplete.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkCreateSetupComplete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkCreateSetupComplete.Location = new System.Drawing.Point(74, 146);
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
            this.chkRunSysprep.Location = new System.Drawing.Point(74, 185);
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
            this.chkResetToec.Location = new System.Drawing.Point(74, 222);
            this.chkResetToec.Name = "chkResetToec";
            this.chkResetToec.Size = new System.Drawing.Size(79, 17);
            this.chkResetToec.TabIndex = 7;
            this.chkResetToec.Text = "Reset Toec";
            this.chkResetToec.UseVisualStyleBackColor = false;
            // 
            // tabGeneral
            // 
            this.tabGeneral.Controls.Add(this.General);
            this.tabGeneral.Controls.Add(this.tabPage2);
            this.tabGeneral.Controls.Add(this.tabPage3);
            this.tabGeneral.Location = new System.Drawing.Point(29, 12);
            this.tabGeneral.Name = "tabGeneral";
            this.tabGeneral.Padding = new System.Drawing.Point(6, 6);
            this.tabGeneral.SelectedIndex = 0;
            this.tabGeneral.Size = new System.Drawing.Size(741, 326);
            this.tabGeneral.TabIndex = 8;
            // 
            // General
            // 
            this.General.BackColor = System.Drawing.Color.DimGray;
            this.General.Controls.Add(this.chkRemoveRemoteAccess);
            this.General.Controls.Add(this.chkEnableBackground);
            this.General.Controls.Add(this.chkResetToec);
            this.General.Controls.Add(this.chkDisableHibernate);
            this.General.Controls.Add(this.chkRunSysprep);
            this.General.Controls.Add(this.chkDriversReg);
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
            this.chkRemoveRemoteAccess.Location = new System.Drawing.Point(74, 254);
            this.chkRemoveRemoteAccess.Name = "chkRemoveRemoteAccess";
            this.chkRemoveRemoteAccess.Size = new System.Drawing.Size(170, 17);
            this.chkRemoveRemoteAccess.TabIndex = 8;
            this.chkRemoveRemoteAccess.Text = "Uninstall Remote Access Client";
            this.chkRemoveRemoteAccess.UseVisualStyleBackColor = false;
            // 
            // tabPage2
            // 
            this.tabPage2.BackColor = System.Drawing.Color.DimGray;
            this.tabPage2.Controls.Add(this.label1);
            this.tabPage2.Controls.Add(this.txtSetupComplete);
            this.tabPage2.Location = new System.Drawing.Point(4, 28);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(733, 294);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "SetupComplete.cmd";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(26, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(177, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "SetupComplete.cmd Script Contents";
            // 
            // txtSetupComplete
            // 
            this.txtSetupComplete.Location = new System.Drawing.Point(26, 33);
            this.txtSetupComplete.Multiline = true;
            this.txtSetupComplete.Name = "txtSetupComplete";
            this.txtSetupComplete.Size = new System.Drawing.Size(679, 223);
            this.txtSetupComplete.TabIndex = 0;
            // 
            // tabPage3
            // 
            this.tabPage3.BackColor = System.Drawing.Color.DimGray;
            this.tabPage3.Controls.Add(this.txtSysprepAnswerFile);
            this.tabPage3.Controls.Add(this.button1);
            this.tabPage3.Location = new System.Drawing.Point(4, 28);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(733, 294);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Sysprep";
            // 
            // txtSysprepAnswerFile
            // 
            this.txtSysprepAnswerFile.Location = new System.Drawing.Point(133, 72);
            this.txtSysprepAnswerFile.Name = "txtSysprepAnswerFile";
            this.txtSysprepAnswerFile.Size = new System.Drawing.Size(475, 20);
            this.txtSysprepAnswerFile.TabIndex = 1;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(133, 43);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(117, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "Select Answer File";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
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
            this.General.ResumeLayout(false);
            this.General.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.CheckBox chkDisableHibernate;
        private System.Windows.Forms.CheckBox chkEnableBackground;
        private System.Windows.Forms.CheckBox chkDriversReg;
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
    }
}