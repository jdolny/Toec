//https://www.codeproject.com/Articles/17253/A-Custom-Message-Box

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Toec_UI
{
    public class MsgBox : Form
    {
        public enum AnimateStyle
        {
            SlideDown = 1,
            FadeIn = 2,
            ZoomIn = 3
        }

        public enum Buttons
        {
            AbortRetryIgnore = 1,
            OK = 2,
            OKCancel = 3,
            RetryCancel = 4,
            YesNo = 5,
            YesNoCancel = 6
        }

        public new enum Icon
        {
            Application = 1,
            Exclamation = 2,
            Error = 3,
            Warning = 4,
            Info = 5,
            Question = 6,
            Shield = 7,
            Search = 8
        }

        private const int CS_DROPSHADOW = 0x00020000;
        private static MsgBox _msgBox;
        private static DialogResult _buttonResult;
        private static Timer _disposeTimer;
        private readonly List<Button> _buttonCollection = new List<Button>();
        private readonly FlowLayoutPanel _flpButtons = new FlowLayoutPanel();
        private readonly Label _lblAutoCloseTime;
        private readonly Label _lblMessage;
        private readonly Label _lblTitle;
        private readonly PictureBox _picIcon = new PictureBox();
        private readonly Panel _plFooter = new Panel();
        private readonly Panel _plHeader = new Panel();
        private readonly Panel _plIcon = new Panel();
        private readonly Panel _plTimeout = new Panel();
        private static int _autoClosetimer;
        private static bool _msgBoxIsOpen;

        public MsgBox()
        {
            
            this.ShowInTaskbar = false;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.FromArgb(45, 45, 48);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Padding = new Padding(3);
            this.Width = 400;
            this.BringToFront();
            this.TopMost = true;
            this.Focus();
            this.Activate();

            if (_autoClosetimer != 0)
            {
                _disposeTimer = new Timer();
                _disposeTimer.Interval = 1000;
                _disposeTimer.Enabled = true;
                _disposeTimer.Start();
                _disposeTimer.Tick += this.autoClosetimer_tick;
            }

            _lblTitle = new Label();
            _lblTitle.ForeColor = Color.White;
            _lblTitle.Font = new Font("Segoe UI", 18);
            _lblTitle.Dock = DockStyle.Top;
            _lblTitle.Height = 50;

            _lblMessage = new Label();
            _lblMessage.ForeColor = Color.White;
            _lblMessage.Font = new Font("Segoe UI", 10);
            _lblMessage.Dock = DockStyle.Fill;

            _lblAutoCloseTime = new Label();
            _lblAutoCloseTime.ForeColor = Color.White;
            _lblAutoCloseTime.Font = new Font("Segoe UI", 10);
            _lblAutoCloseTime.Dock = DockStyle.Fill;

            _flpButtons.FlowDirection = FlowDirection.RightToLeft;
            _flpButtons.Dock = DockStyle.Fill;

            _plHeader.Dock = DockStyle.Fill;
            _plHeader.Padding = new Padding(20);
            _plHeader.Controls.Add(_lblMessage);
            _plHeader.Controls.Add(_lblTitle);

            _plFooter.Dock = DockStyle.Bottom;
            _plFooter.Padding = new Padding(0, 10, 20, 0);
            _plFooter.BackColor = Color.FromArgb(37, 37, 38);
            _plFooter.Height = 60;
            _plFooter.Controls.Add(_flpButtons);

            _plTimeout.Dock = DockStyle.Bottom;

            _plTimeout.BackColor = Color.FromArgb(37, 37, 38);
            _plTimeout.Controls.Add(_lblAutoCloseTime);
            _plTimeout.Height = 20;

            _picIcon.Width = 32;
            _picIcon.Height = 32;
            _picIcon.Location = new Point(30, 50);

            _plIcon.Dock = DockStyle.Left;
            _plIcon.Padding = new Padding(20);
            _plIcon.Width = 70;
            _plIcon.Controls.Add(_picIcon);

            this.Controls.Add(_plHeader);
            this.Controls.Add(_plIcon);
            this.Controls.Add(_plFooter);
            this.Controls.Add(_plTimeout);

        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ClassStyle |= CS_DROPSHADOW;
                return cp;
            }
        }

        private static void ButtonClick(object sender, EventArgs e)
        {
            
            var btn = (Button) sender;
          
            switch (btn.Text)
            {
                case "Abort":
                    _buttonResult = DialogResult.Abort;
                    break;

                case "Retry":
                    _buttonResult = DialogResult.Retry;
                    break;

                case "Ignore":
                    _buttonResult = DialogResult.Ignore;
                    break;

                case "OK":
                    _buttonResult = DialogResult.OK;
                    break;

                case "Cancel":
                    _buttonResult = DialogResult.Cancel;
                    break;

                case "Yes":
                    _buttonResult = DialogResult.Yes;
                    break;

                case "No":
                    _buttonResult = DialogResult.No;
                    break;
            }

          
             _msgBox.Dispose();
            _msgBoxIsOpen = false;
            if (_autoClosetimer != 0)
            {
                _disposeTimer.Enabled = false;
                _disposeTimer.Dispose();
            }
        }

        private void InitAbortRetryIgnoreButtons()
        {
            var btnAbort = new Button();
            btnAbort.Text = "Abort";
            btnAbort.Click += ButtonClick;

            var btnRetry = new Button();
            btnRetry.Text = "Retry";
            btnRetry.Click += ButtonClick;

            var btnIgnore = new Button();
            btnIgnore.Text = "Ignore";
            btnIgnore.Click += ButtonClick;

            this._buttonCollection.Add(btnAbort);
            this._buttonCollection.Add(btnRetry);
            this._buttonCollection.Add(btnIgnore);
        }

        private static void InitButtons(Buttons buttons)
        {
            switch (buttons)
            {
                case Buttons.AbortRetryIgnore:
                    _msgBox.InitAbortRetryIgnoreButtons();
                    break;

                case Buttons.OK:
                    _msgBox.InitOKButton();
                    break;

                case Buttons.OKCancel:
                    _msgBox.InitOKCancelButtons();
                    break;

                case Buttons.RetryCancel:
                    _msgBox.InitRetryCancelButtons();
                    break;

                case Buttons.YesNo:
                    _msgBox.InitYesNoButtons();
                    break;

                case Buttons.YesNoCancel:
                    _msgBox.InitYesNoCancelButtons();
                    break;
            }

            foreach (var btn in _msgBox._buttonCollection)
            {
                btn.ForeColor = Color.FromArgb(170, 170, 170);
                btn.Font = new Font("Segoe UI", 8);
                btn.Padding = new Padding(3);
                btn.FlatStyle = FlatStyle.Flat;
                btn.Height = 30;
                btn.FlatAppearance.BorderColor = Color.FromArgb(99, 99, 98);

                _msgBox._flpButtons.Controls.Add(btn);
            }
        }

        private static void InitIcon(Icon icon)
        {
            switch (icon)
            {
                case Icon.Application:
                    _msgBox._picIcon.Image = SystemIcons.Application.ToBitmap();
                    break;

                case Icon.Exclamation:
                    _msgBox._picIcon.Image = SystemIcons.Exclamation.ToBitmap();
                    break;

                case Icon.Error:
                    _msgBox._picIcon.Image = SystemIcons.Error.ToBitmap();
                    break;

                case Icon.Info:
                    _msgBox._picIcon.Image = SystemIcons.Information.ToBitmap();
                    break;

                case Icon.Question:
                    _msgBox._picIcon.Image = SystemIcons.Question.ToBitmap();
                    break;

                case Icon.Shield:
                    _msgBox._picIcon.Image = SystemIcons.Shield.ToBitmap();
                    break;

                case Icon.Warning:
                    _msgBox._picIcon.Image = SystemIcons.Warning.ToBitmap();
                    break;
            }
        }

        private void InitOKButton()
        {
            var btnOK = new Button();
            btnOK.Text = "OK";
            btnOK.Click += ButtonClick;

            this._buttonCollection.Add(btnOK);
        }

        private void InitOKCancelButtons()
        {
            var btnOK = new Button();
            btnOK.Text = "OK";
            btnOK.Click += ButtonClick;

            var btnCancel = new Button();
            btnCancel.Text = "Cancel";
            btnCancel.Click += ButtonClick;

            this._buttonCollection.Add(btnOK);
            this._buttonCollection.Add(btnCancel);
        }

        private void InitRetryCancelButtons()
        {
            var btnRetry = new Button();
            btnRetry.Text = "OK";
            btnRetry.Click += ButtonClick;

            var btnCancel = new Button();
            btnCancel.Text = "Cancel";
            btnCancel.Click += ButtonClick;

            this._buttonCollection.Add(btnRetry);
            this._buttonCollection.Add(btnCancel);
        }

        private void InitYesNoButtons()
        {
            var btnYes = new Button();
            btnYes.Text = "Yes";
            btnYes.Click += ButtonClick;

            var btnNo = new Button();
            btnNo.Text = "No";
            btnNo.Click += ButtonClick;

            this._buttonCollection.Add(btnYes);
            this._buttonCollection.Add(btnNo);
        }

        private void InitYesNoCancelButtons()
        {
            var btnYes = new Button();
            btnYes.Text = "Abort";
            btnYes.Click += ButtonClick;

            var btnNo = new Button();
            btnNo.Text = "Retry";
            btnNo.Click += ButtonClick;

            var btnCancel = new Button();
            btnCancel.Text = "Cancel";
            btnCancel.Click += ButtonClick;

            this._buttonCollection.Add(btnYes);
            this._buttonCollection.Add(btnNo);
            this._buttonCollection.Add(btnCancel);
        }

   

        private static Size MessageSize(string message)
        {
            var g = _msgBox.CreateGraphics();
            var width = 350;
            var height = 230;

            var size = g.MeasureString(message, new Font("Segoe UI", 10));

            if (message == null)
                return new Size(width,height);

            if (message.Length < 150)
            {
                if ((int) size.Width > 350)
                {
                    width = (int) size.Width;
                }
            }
            else
            {
                var groups = (from Match m in Regex.Matches(message, ".{1,180}") select m.Value).ToArray();
                var lines = groups.Length + 1;
                width = 700;
                height += (int) (size.Height + 10)*lines;
            }
            return new Size(width, height);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            var rect = new Rectangle(new Point(0, 0), new Size(this.Width - 1, this.Height - 1));
            var pen = new Pen(Color.FromArgb(0, 151, 251));

            g.DrawRectangle(pen, rect);
        }

        public static DialogResult Show(string message, string title, Buttons buttons, Icon icon, int timer)
        {
            if(_msgBoxIsOpen) return DialogResult.OK;
            _msgBoxIsOpen = true;
            _autoClosetimer = timer;
            _msgBox = new MsgBox();
            _msgBox._lblMessage.Text = message;
            _msgBox._lblTitle.Text = title;

            InitButtons(buttons);
            InitIcon(icon);

            _msgBox.Size = MessageSize(message);
            _msgBox.BringToFront();
            _msgBox.TopMost = true;
            _msgBox.Focus();
            _msgBox.Activate();
            _msgBox.ShowDialog();
            return _buttonResult;
        }

     

        private void autoClosetimer_tick(object sender, EventArgs e)
        {
            _autoClosetimer--;

            if (_autoClosetimer >= 0)
            {
                _msgBox._lblAutoCloseTime.Text = _autoClosetimer.ToString();
            }
            else
            {
                _msgBoxIsOpen = false;
                _msgBox.Dispose();
                _disposeTimer.Enabled = false;
                _disposeTimer.Dispose();
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // MsgBox
            // 
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Name = "MsgBox";
            this.Load += new System.EventHandler(this.MsgBox_Load);
            this.ResumeLayout(false);

        }

        private void MsgBox_Load(object sender, EventArgs e)
        {

        }
    }

    internal class AnimateMsgBox
    {
        public Size FormSize;
        public MsgBox.AnimateStyle Style;

        public AnimateMsgBox(Size formSize, MsgBox.AnimateStyle style)
        {
            this.FormSize = formSize;
            this.Style = style;
        }
    }
}