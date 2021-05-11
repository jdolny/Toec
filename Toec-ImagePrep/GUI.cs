using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Toec_Common.Dto;

namespace Toec_ImagePrep
{
    public partial class GUI : Form
    {

        public GUI()
        {
            this.ShowInTaskbar = true;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Padding = new Padding(3);
            this.Width = 400;
            this.BringToFront();
            this.TopMost = true;
            this.Focus();
            this.Activate();

         
            InitializeComponent();

            txtSetupComplete.Text = @"powercfg.exe /h off" + Environment.NewLine;
            txtSetupComplete.Text += @"del /Q /F c:\windows\system32\sysprep\unattend.xml" + Environment.NewLine;
            txtSetupComplete.Text += @"del /Q /F c:\windows\panther\unattend.xml" + Environment.NewLine;
            txtSetupComplete.Text += @"pnputil.exe /add-driver c:\drivers\*.inf /subdirs /install";


        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            var imagePrepOptions = new DtoImagePrepOptions();
            if (chkDisableHibernate.Checked)
                imagePrepOptions.RunHibernate = true;

            if (chkDriversReg.Checked)
                imagePrepOptions.AddDriverRegistry = true;

            if (chkEnableBackground.Checked)
                imagePrepOptions.EnableFinalizingBackground = true;

            if (chkCreateSetupComplete.Checked)
            {
                imagePrepOptions.CreateSetupComplete = true;
                imagePrepOptions.SetupCompleteContents = txtSetupComplete.Text;
            }

            if (chkRunSysprep.Checked)
            {
                imagePrepOptions.RunSysprep = true;
                imagePrepOptions.SysprepAnswerPath = txtSysprepAnswerFile.Text;
            }

            if (chkResetToec.Checked)
                imagePrepOptions.ResetToec = true;

            Console.Write(JsonConvert.SerializeObject(imagePrepOptions));
            this.Close();
        }


        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            txtSysprepAnswerFile.Text = openFileDialog1.FileName;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Console.Write("");
            this.Close();
        }
    }

   
}
