namespace CSharpClientUI
{
    partial class SecureImageUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SecureImageUI));
            this.btnConnect = new System.Windows.Forms.Button();
            this.btnSendKeys = new System.Windows.Forms.Button();
            this.btnGetPicture = new System.Windows.Forms.Button();
            this.btnShow = new System.Windows.Forms.Button();
            this.panel = new System.Windows.Forms.Panel();
            this.lblServerStatus = new System.Windows.Forms.Label();
            this.lblKeyStatus = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.pictureBox4 = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.flickerTimer = new System.Windows.Forms.Timer(this.components);
            this.rbNew = new System.Windows.Forms.RadioButton();
            this.btnReset = new System.Windows.Forms.Button();
            this.resetToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.button1 = new System.Windows.Forms.Button();
            this.sendPasswordbtn = new System.Windows.Forms.Button();
            this.passwordLabel = new System.Windows.Forms.Label();
            this.lblGetPicStatus = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label3 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // btnConnect
            // 
            this.btnConnect.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnConnect.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(66)))), ((int)(((byte)(129)))));
            this.btnConnect.Location = new System.Drawing.Point(353, 176);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(100, 28);
            this.btnConnect.TabIndex = 0;
            this.btnConnect.Text = "Connect";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // btnSendKeys
            // 
            this.btnSendKeys.Font = new System.Drawing.Font("Verdana", 9.75F);
            this.btnSendKeys.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(66)))), ((int)(((byte)(129)))));
            this.btnSendKeys.Location = new System.Drawing.Point(353, 270);
            this.btnSendKeys.Name = "btnSendKeys";
            this.btnSendKeys.Size = new System.Drawing.Size(100, 28);
            this.btnSendKeys.TabIndex = 1;
            this.btnSendKeys.Text = "Sigma";
            this.btnSendKeys.UseVisualStyleBackColor = true;
            this.btnSendKeys.Click += new System.EventHandler(this.btnSigma_Click);
            // 
            // btnGetPicture
            // 
            this.btnGetPicture.Font = new System.Drawing.Font("Verdana", 9.75F);
            this.btnGetPicture.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(66)))), ((int)(((byte)(129)))));
            this.btnGetPicture.Location = new System.Drawing.Point(353, 380);
            this.btnGetPicture.Name = "btnGetPicture";
            this.btnGetPicture.Size = new System.Drawing.Size(100, 28);
            this.btnGetPicture.TabIndex = 2;
            this.btnGetPicture.Text = "Get";
            this.btnGetPicture.UseVisualStyleBackColor = true;
            this.btnGetPicture.Click += new System.EventHandler(this.btnGetPicture_Click);
            // 
            // btnShow
            // 
            this.btnShow.Font = new System.Drawing.Font("Verdana", 9.75F);
            this.btnShow.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(66)))), ((int)(((byte)(129)))));
            this.btnShow.Location = new System.Drawing.Point(621, 125);
            this.btnShow.Name = "btnShow";
            this.btnShow.Size = new System.Drawing.Size(100, 28);
            this.btnShow.TabIndex = 4;
            this.btnShow.Text = "Show Image";
            this.btnShow.UseVisualStyleBackColor = true;
            this.btnShow.Click += new System.EventHandler(this.btnShow_Click);
            // 
            // panel
            // 
            this.panel.BackColor = System.Drawing.Color.Transparent;
            this.panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel.Location = new System.Drawing.Point(463, 159);
            this.panel.Name = "panel";
            this.panel.Size = new System.Drawing.Size(400, 300);
            this.panel.TabIndex = 8;
            // 
            // lblServerStatus
            // 
            this.lblServerStatus.AutoSize = true;
            this.lblServerStatus.Font = new System.Drawing.Font("Verdana", 9.75F);
            this.lblServerStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(66)))), ((int)(((byte)(129)))));
            this.lblServerStatus.Location = new System.Drawing.Point(73, 188);
            this.lblServerStatus.Name = "lblServerStatus";
            this.lblServerStatus.Size = new System.Drawing.Size(97, 16);
            this.lblServerStatus.TabIndex = 11;
            this.lblServerStatus.Text = "Disconnected";
            // 
            // lblKeyStatus
            // 
            this.lblKeyStatus.AutoSize = true;
            this.lblKeyStatus.Font = new System.Drawing.Font("Verdana", 9.75F);
            this.lblKeyStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(66)))), ((int)(((byte)(129)))));
            this.lblKeyStatus.Location = new System.Drawing.Point(73, 297);
            this.lblKeyStatus.MaximumSize = new System.Drawing.Size(300, 0);
            this.lblKeyStatus.Name = "lblKeyStatus";
            this.lblKeyStatus.Size = new System.Drawing.Size(0, 16);
            this.lblKeyStatus.TabIndex = 19;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Verdana", 9.75F);
            this.label4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(66)))), ((int)(((byte)(129)))));
            this.label4.Location = new System.Drawing.Point(15, 162);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(155, 16);
            this.label4.TabIndex = 22;
            this.label4.Text = "Connect to the server";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Verdana", 9.75F);
            this.label5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(66)))), ((int)(((byte)(129)))));
            this.label5.Location = new System.Drawing.Point(15, 188);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(58, 16);
            this.label5.TabIndex = 23;
            this.label5.Text = "Status:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Verdana", 9.75F);
            this.label6.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(66)))), ((int)(((byte)(129)))));
            this.label6.Location = new System.Drawing.Point(15, 270);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(221, 16);
            this.label6.TabIndex = 24;
            this.label6.Text = "Exchange keys - Sigma protocol";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Verdana", 9.75F);
            this.label7.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(66)))), ((int)(((byte)(129)))));
            this.label7.Location = new System.Drawing.Point(15, 386);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(253, 16);
            this.label7.TabIndex = 25;
            this.label7.Text = "Get encrypted image from the server";
            // 
            // btnClose
            // 
            this.btnClose.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClose.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(66)))), ((int)(((byte)(129)))));
            this.btnClose.Location = new System.Drawing.Point(768, 556);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(100, 28);
            this.btnClose.TabIndex = 30;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // pictureBox2
            // 
            this.pictureBox2.BackgroundImage = global::CSharpClientUI.Properties.Resources.Background;
            this.pictureBox2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.pictureBox2.Location = new System.Drawing.Point(0, 547);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(875, 47);
            this.pictureBox2.TabIndex = 29;
            this.pictureBox2.TabStop = false;
            // 
            // pictureBox4
            // 
            this.pictureBox4.BackgroundImage = global::CSharpClientUI.Properties.Resources.Title;
            this.pictureBox4.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.pictureBox4.Location = new System.Drawing.Point(0, 0);
            this.pictureBox4.Name = "pictureBox4";
            this.pictureBox4.Size = new System.Drawing.Size(875, 69);
            this.pictureBox4.TabIndex = 31;
            this.pictureBox4.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Verdana", 9.75F);
            this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(66)))), ((int)(((byte)(129)))));
            this.label1.Location = new System.Drawing.Point(15, 297);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 16);
            this.label1.TabIndex = 32;
            this.label1.Text = "Status:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Verdana", 9.75F);
            this.label2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(66)))), ((int)(((byte)(129)))));
            this.label2.Location = new System.Drawing.Point(15, 415);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(58, 16);
            this.label2.TabIndex = 33;
            this.label2.Text = "Status:";
            // 
            // flickerTimer
            // 
            this.flickerTimer.Interval = 500;
            this.flickerTimer.Tick += new System.EventHandler(this.flicker_Tick);
            // 
            // rbNew
            // 
            this.rbNew.AutoSize = true;
            this.rbNew.Checked = true;
            this.rbNew.Font = new System.Drawing.Font("Verdana", 9.75F);
            this.rbNew.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(66)))), ((int)(((byte)(129)))));
            this.rbNew.Location = new System.Drawing.Point(12, 75);
            this.rbNew.Name = "rbNew";
            this.rbNew.Size = new System.Drawing.Size(244, 20);
            this.rbNew.TabIndex = 36;
            this.rbNew.TabStop = true;
            this.rbNew.Text = "Get a new image from the server";
            this.rbNew.UseVisualStyleBackColor = true;
            this.rbNew.CheckedChanged += new System.EventHandler(this.rbNew_CheckedChanged);
            // 
            // btnReset
            // 
            this.btnReset.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnReset.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(66)))), ((int)(((byte)(129)))));
            this.btnReset.Location = new System.Drawing.Point(662, 556);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(100, 28);
            this.btnReset.TabIndex = 40;
            this.btnReset.Text = "Reset";
            this.resetToolTip.SetToolTip(this.btnReset, "By clicking the reset button the solution will be reseted. \r\nThis will clean the " +
        "TA keys and metadata and will reset the TA MTC.\r\nAll stored images will become i" +
        "nvalid.");
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("Verdana", 9.75F);
            this.button1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(66)))), ((int)(((byte)(129)))));
            this.button1.Location = new System.Drawing.Point(353, 225);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(100, 28);
            this.button1.TabIndex = 41;
            this.button1.Text = "Install";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.install_Click);
            // 
            // sendPasswordbtn
            // 
            this.sendPasswordbtn.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.sendPasswordbtn.ForeColor = System.Drawing.Color.Navy;
            this.sendPasswordbtn.Location = new System.Drawing.Point(353, 319);
            this.sendPasswordbtn.Name = "sendPasswordbtn";
            this.sendPasswordbtn.Size = new System.Drawing.Size(100, 28);
            this.sendPasswordbtn.TabIndex = 45;
            this.sendPasswordbtn.Text = "password";
            this.sendPasswordbtn.UseVisualStyleBackColor = true;
            this.sendPasswordbtn.Click += new System.EventHandler(this.passwordButtonClick);
            // 
            // passwordLabel
            // 
            this.passwordLabel.AutoSize = true;
            this.passwordLabel.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.passwordLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(66)))), ((int)(((byte)(129)))));
            this.passwordLabel.Location = new System.Drawing.Point(15, 359);
            this.passwordLabel.Name = "passwordLabel";
            this.passwordLabel.Size = new System.Drawing.Size(53, 14);
            this.passwordLabel.TabIndex = 46;
            this.passwordLabel.Text = "Status:";
            // 
            // lblGetPicStatus
            // 
            this.lblGetPicStatus.AutoSize = true;
            this.lblGetPicStatus.Font = new System.Drawing.Font("Verdana", 9.75F);
            this.lblGetPicStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(66)))), ((int)(((byte)(129)))));
            this.lblGetPicStatus.Location = new System.Drawing.Point(73, 415);
            this.lblGetPicStatus.MaximumSize = new System.Drawing.Size(300, 0);
            this.lblGetPicStatus.Name = "lblGetPicStatus";
            this.lblGetPicStatus.Size = new System.Drawing.Size(0, 16);
            this.lblGetPicStatus.TabIndex = 20;
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackgroundImage = global::CSharpClientUI.Properties.Resources.Background;
            this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(875, 47);
            this.pictureBox1.TabIndex = 47;
            this.pictureBox1.TabStop = false;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Verdana", 9.75F);
            this.label3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(66)))), ((int)(((byte)(129)))));
            this.label3.Location = new System.Drawing.Point(15, 225);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(94, 16);
            this.label3.TabIndex = 48;
            this.label3.Text = "Install applet";
            // 
            // SecureImageUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(875, 594);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.passwordLabel);
            this.Controls.Add(this.sendPasswordbtn);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btnReset);
            this.Controls.Add(this.rbNew);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pictureBox4);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.lblGetPicStatus);
            this.Controls.Add(this.lblKeyStatus);
            this.Controls.Add(this.lblServerStatus);
            this.Controls.Add(this.btnGetPicture);
            this.Controls.Add(this.btnConnect);
            this.Controls.Add(this.btnSendKeys);
            this.Controls.Add(this.panel);
            this.Controls.Add(this.btnShow);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(300, 39);
            this.Name = "SecureImageUI";
            this.Text = "Secure Image";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.formClosed);
            this.Load += new System.EventHandler(this.SecureImageUI_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Button btnSendKeys;
        private System.Windows.Forms.Button btnGetPicture;
        private System.Windows.Forms.Button btnShow;
        private System.Windows.Forms.Panel panel;
        private System.Windows.Forms.Label lblServerStatus;
        private System.Windows.Forms.Label lblKeyStatus;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.PictureBox pictureBox4;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Timer flickerTimer;
        private System.Windows.Forms.RadioButton rbNew;
        private System.Windows.Forms.Button btnReset;
        //private Microsoft.VisualBasic.PowerPacks.ShapeContainer shapeContainer1;
        //private Microsoft.VisualBasic.PowerPacks.LineShape lineShape3;
        //private Microsoft.VisualBasic.PowerPacks.LineShape lineShape2;
        private System.Windows.Forms.ToolTip resetToolTip;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button sendPasswordbtn;
        private System.Windows.Forms.Label passwordLabel;
        private System.Windows.Forms.Label lblGetPicStatus;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label3;
    }
}

