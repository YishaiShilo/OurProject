namespace CSharpClientUI
{
    partial class ProtectedOutputUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProtectedOutputUI));
            this.btnConnect = new System.Windows.Forms.Button();
            this.btnSendKeys = new System.Windows.Forms.Button();
            this.btnGetPicture = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnShow = new System.Windows.Forms.Button();
            this.btnLoad = new System.Windows.Forms.Button();
            this.panel = new System.Windows.Forms.Panel();
            this.lblServerStatus = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lblLocalPictureStatus = new System.Windows.Forms.Label();
            this.lblNumViews = new System.Windows.Forms.Label();
            this.lblKeyStatus = new System.Windows.Forms.Label();
            this.lblGetPicStatus = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.pictureBox4 = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnAbout = new System.Windows.Forms.Button();
            this.label11 = new System.Windows.Forms.Label();
            this.flickerTimer = new System.Windows.Forms.Timer(this.components);
            this.rbNew = new System.Windows.Forms.RadioButton();
            this.rbLoad = new System.Windows.Forms.RadioButton();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.btnReset = new System.Windows.Forms.Button();
            //this.shapeContainer1 = new Microsoft.VisualBasic.PowerPacks.ShapeContainer();
            //this.lineShape2 = new Microsoft.VisualBasic.PowerPacks.LineShape();
            //this.lineShape3 = new Microsoft.VisualBasic.PowerPacks.LineShape();
            this.resetToolTip = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).BeginInit();
            this.SuspendLayout();
            // 
            // btnConnect
            // 
            this.btnConnect.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnConnect.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(66)))), ((int)(((byte)(129)))));
            this.btnConnect.Location = new System.Drawing.Point(332, 205);
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
            this.btnSendKeys.Location = new System.Drawing.Point(333, 267);
            this.btnSendKeys.Name = "btnSendKeys";
            this.btnSendKeys.Size = new System.Drawing.Size(100, 28);
            this.btnSendKeys.TabIndex = 1;
            this.btnSendKeys.Text = "Send";
            this.btnSendKeys.UseVisualStyleBackColor = true;
            this.btnSendKeys.Click += new System.EventHandler(this.btnSendKeys_Click);
            // 
            // btnGetPicture
            // 
            this.btnGetPicture.Font = new System.Drawing.Font("Verdana", 9.75F);
            this.btnGetPicture.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(66)))), ((int)(((byte)(129)))));
            this.btnGetPicture.Location = new System.Drawing.Point(333, 331);
            this.btnGetPicture.Name = "btnGetPicture";
            this.btnGetPicture.Size = new System.Drawing.Size(100, 28);
            this.btnGetPicture.TabIndex = 2;
            this.btnGetPicture.Text = "Get";
            this.btnGetPicture.UseVisualStyleBackColor = true;
            this.btnGetPicture.Click += new System.EventHandler(this.btnGetPicture_Click);
            // 
            // btnSave
            // 
            this.btnSave.Font = new System.Drawing.Font("Verdana", 9.75F);
            this.btnSave.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(66)))), ((int)(((byte)(129)))));
            this.btnSave.Location = new System.Drawing.Point(332, 390);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(100, 28);
            this.btnSave.TabIndex = 3;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnShow
            // 
            this.btnShow.Font = new System.Drawing.Font("Verdana", 9.75F);
            this.btnShow.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(66)))), ((int)(((byte)(129)))));
            this.btnShow.Location = new System.Drawing.Point(466, 179);
            this.btnShow.Name = "btnShow";
            this.btnShow.Size = new System.Drawing.Size(100, 28);
            this.btnShow.TabIndex = 4;
            this.btnShow.Text = "Show Image";
            this.btnShow.UseVisualStyleBackColor = true;
            this.btnShow.Click += new System.EventHandler(this.btnShow_Click);
            // 
            // btnLoad
            // 
            this.btnLoad.Enabled = false;
            this.btnLoad.Font = new System.Drawing.Font("Verdana", 9.75F);
            this.btnLoad.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(66)))), ((int)(((byte)(129)))));
            this.btnLoad.Location = new System.Drawing.Point(332, 452);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(100, 28);
            this.btnLoad.TabIndex = 5;
            this.btnLoad.Text = "Load";
            this.btnLoad.UseVisualStyleBackColor = true;
            this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
            // 
            // panel
            // 
            this.panel.BackColor = System.Drawing.Color.Transparent;
            this.panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel.Location = new System.Drawing.Point(465, 220);
            this.panel.Name = "panel";
            this.panel.Size = new System.Drawing.Size(400, 300);
            this.panel.TabIndex = 8;
            // 
            // lblServerStatus
            // 
            this.lblServerStatus.AutoSize = true;
            this.lblServerStatus.Font = new System.Drawing.Font("Verdana", 9.75F);
            this.lblServerStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(66)))), ((int)(((byte)(129)))));
            this.lblServerStatus.Location = new System.Drawing.Point(125, 237);
            this.lblServerStatus.Name = "lblServerStatus";
            this.lblServerStatus.Size = new System.Drawing.Size(97, 16);
            this.lblServerStatus.TabIndex = 11;
            this.lblServerStatus.Text = "Disconnected";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.Red;
            this.label3.Location = new System.Drawing.Point(680, 185);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(166, 16);
            this.label3.TabIndex = 16;
            this.label3.Text = "Number of views left: ";
            // 
            // lblLocalPictureStatus
            // 
            this.lblLocalPictureStatus.AutoSize = true;
            this.lblLocalPictureStatus.Font = new System.Drawing.Font("Verdana", 9.75F);
            this.lblLocalPictureStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(66)))), ((int)(((byte)(129)))));
            this.lblLocalPictureStatus.Location = new System.Drawing.Point(106, 486);
            this.lblLocalPictureStatus.MaximumSize = new System.Drawing.Size(300, 0);
            this.lblLocalPictureStatus.Name = "lblLocalPictureStatus";
            this.lblLocalPictureStatus.Size = new System.Drawing.Size(131, 16);
            this.lblLocalPictureStatus.TabIndex = 17;
            this.lblLocalPictureStatus.Text = "No image is loaded";
            // 
            // lblNumViews
            // 
            this.lblNumViews.AutoSize = true;
            this.lblNumViews.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNumViews.ForeColor = System.Drawing.Color.Red;
            this.lblNumViews.Location = new System.Drawing.Point(852, 185);
            this.lblNumViews.Name = "lblNumViews";
            this.lblNumViews.Size = new System.Drawing.Size(16, 16);
            this.lblNumViews.TabIndex = 18;
            this.lblNumViews.Text = "?";
            // 
            // lblKeyStatus
            // 
            this.lblKeyStatus.AutoSize = true;
            this.lblKeyStatus.Font = new System.Drawing.Font("Verdana", 9.75F);
            this.lblKeyStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(66)))), ((int)(((byte)(129)))));
            this.lblKeyStatus.Location = new System.Drawing.Point(125, 299);
            this.lblKeyStatus.MaximumSize = new System.Drawing.Size(300, 0);
            this.lblKeyStatus.Name = "lblKeyStatus";
            this.lblKeyStatus.Size = new System.Drawing.Size(0, 16);
            this.lblKeyStatus.TabIndex = 19;
            // 
            // lblGetPicStatus
            // 
            this.lblGetPicStatus.AutoSize = true;
            this.lblGetPicStatus.Font = new System.Drawing.Font("Verdana", 9.75F);
            this.lblGetPicStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(66)))), ((int)(((byte)(129)))));
            this.lblGetPicStatus.Location = new System.Drawing.Point(125, 363);
            this.lblGetPicStatus.MaximumSize = new System.Drawing.Size(300, 0);
            this.lblGetPicStatus.Name = "lblGetPicStatus";
            this.lblGetPicStatus.Size = new System.Drawing.Size(0, 16);
            this.lblGetPicStatus.TabIndex = 20;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Verdana", 9.75F);
            this.label4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(66)))), ((int)(((byte)(129)))));
            this.label4.Location = new System.Drawing.Point(48, 211);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(173, 16);
            this.label4.TabIndex = 22;
            this.label4.Text = "a. Connect to the server";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Verdana", 9.75F);
            this.label5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(66)))), ((int)(((byte)(129)))));
            this.label5.Location = new System.Drawing.Point(67, 237);
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
            this.label6.Location = new System.Drawing.Point(48, 273);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(152, 16);
            this.label6.TabIndex = 24;
            this.label6.Text = "b. Send TA public key";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Verdana", 9.75F);
            this.label7.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(66)))), ((int)(((byte)(129)))));
            this.label7.Location = new System.Drawing.Point(48, 337);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(271, 16);
            this.label7.TabIndex = 25;
            this.label7.Text = "c. Get encrypted image from the server";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Verdana", 9.75F);
            this.label8.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(66)))), ((int)(((byte)(129)))));
            this.label8.Location = new System.Drawing.Point(48, 486);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(58, 16);
            this.label8.TabIndex = 26;
            this.label8.Text = "Status:";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Verdana", 9.75F);
            this.label9.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(66)))), ((int)(((byte)(129)))));
            this.label9.Location = new System.Drawing.Point(48, 458);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(204, 16);
            this.label9.TabIndex = 27;
            this.label9.Text = "Load image file from hard disk";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Verdana", 9.75F);
            this.label10.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(66)))), ((int)(((byte)(129)))));
            this.label10.Location = new System.Drawing.Point(48, 396);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(180, 16);
            this.label10.TabIndex = 28;
            this.label10.Text = "Save image for future use";
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
            this.label1.Location = new System.Drawing.Point(67, 299);
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
            this.label2.Location = new System.Drawing.Point(67, 363);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(58, 16);
            this.label2.TabIndex = 33;
            this.label2.Text = "Status:";
            // 
            // btnAbout
            // 
            this.btnAbout.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAbout.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(66)))), ((int)(((byte)(129)))));
            this.btnAbout.Location = new System.Drawing.Point(333, 86);
            this.btnAbout.Name = "btnAbout";
            this.btnAbout.Size = new System.Drawing.Size(100, 28);
            this.btnAbout.TabIndex = 34;
            this.btnAbout.Text = "About";
            this.btnAbout.UseVisualStyleBackColor = true;
            this.btnAbout.Click += new System.EventHandler(this.btnAbout_Click);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Verdana", 9.75F);
            this.label11.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(66)))), ((int)(((byte)(129)))));
            this.label11.Location = new System.Drawing.Point(12, 92);
            this.label11.MaximumSize = new System.Drawing.Size(300, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(289, 16);
            this.label11.TabIndex = 35;
            this.label11.Text = "Click to learn more about the solution flow";
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
            this.rbNew.Location = new System.Drawing.Point(24, 183);
            this.rbNew.Name = "rbNew";
            this.rbNew.Size = new System.Drawing.Size(244, 20);
            this.rbNew.TabIndex = 36;
            this.rbNew.TabStop = true;
            this.rbNew.Text = "Get a new image from the server";
            this.rbNew.UseVisualStyleBackColor = true;
            this.rbNew.CheckedChanged += new System.EventHandler(this.rbNew_CheckedChanged);
            // 
            // rbLoad
            // 
            this.rbLoad.AutoSize = true;
            this.rbLoad.Font = new System.Drawing.Font("Verdana", 9.75F);
            this.rbLoad.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(66)))), ((int)(((byte)(129)))));
            this.rbLoad.Location = new System.Drawing.Point(19, 428);
            this.rbLoad.Name = "rbLoad";
            this.rbLoad.Size = new System.Drawing.Size(144, 20);
            this.rbLoad.TabIndex = 37;
            this.rbLoad.Text = "Load saved image";
            this.rbLoad.UseVisualStyleBackColor = true;
            this.rbLoad.CheckedChanged += new System.EventHandler(this.rbLoad_CheckedChanged);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(66)))), ((int)(((byte)(129)))));
            this.label12.Location = new System.Drawing.Point(463, 138);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(135, 16);
            this.label12.TabIndex = 38;
            this.label12.Text = "2. Display Image:";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label13.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(66)))), ((int)(((byte)(129)))));
            this.label13.Location = new System.Drawing.Point(16, 138);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(117, 16);
            this.label13.TabIndex = 39;
            this.label13.Text = "1. Load Image:";
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
            // shapeContainer1
            // 
            //this.shapeContainer1.Location = new System.Drawing.Point(0, 0);
            //this.shapeContainer1.Margin = new System.Windows.Forms.Padding(0);
            //this.shapeContainer1.Name = "shapeContainer1";
            //this.shapeContainer1.Shapes.AddRange(new Microsoft.VisualBasic.PowerPacks.Shape[] {
            //this.lineShape2,
            //this.lineShape3});
            //this.shapeContainer1.Size = new System.Drawing.Size(875, 594);
            //this.shapeContainer1.TabIndex = 15;
            //this.shapeContainer1.TabStop = false;
            // 
            // lineShape2
            // 
            //this.lineShape2.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(66)))), ((int)(((byte)(129)))));
            //this.lineShape2.Name = "lineShape2";
            //this.lineShape2.X1 = -5;
            //this.lineShape2.X2 = 877;
            //this.lineShape2.Y1 = 130;
            //this.lineShape2.Y2 = 130;
            // 
            // lineShape3
            // 
            //this.lineShape3.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(66)))), ((int)(((byte)(129)))));
            //this.lineShape3.Name = "lineShape3";
            //this.lineShape3.X1 = 449;
            //this.lineShape3.X2 = 449;
            //this.lineShape3.Y1 = 130;
            //this.lineShape3.Y2 = 547;
            // 
            // ProtectedOutputUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(875, 594);
            this.Controls.Add(this.btnReset);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.rbLoad);
            this.Controls.Add(this.rbNew);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.btnAbout);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pictureBox4);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.lblGetPicStatus);
            this.Controls.Add(this.lblKeyStatus);
            this.Controls.Add(this.lblNumViews);
            this.Controls.Add(this.lblLocalPictureStatus);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.lblServerStatus);
            this.Controls.Add(this.btnGetPicture);
            this.Controls.Add(this.btnConnect);
            this.Controls.Add(this.btnSendKeys);
            this.Controls.Add(this.btnLoad);
            this.Controls.Add(this.panel);
            this.Controls.Add(this.btnShow);
            this.Controls.Add(this.btnSave);
            //this.Controls.Add(this.shapeContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(300, 39);
            this.Name = "ProtectedOutputUI";
            this.Text = "Protected Output";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.formClosed);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Button btnSendKeys;
        private System.Windows.Forms.Button btnGetPicture;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnShow;
        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.Panel panel;
        private System.Windows.Forms.Label lblServerStatus;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblLocalPictureStatus;
        private System.Windows.Forms.Label lblNumViews;
        private System.Windows.Forms.Label lblKeyStatus;
        private System.Windows.Forms.Label lblGetPicStatus;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.PictureBox pictureBox4;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnAbout;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Timer flickerTimer;
        private System.Windows.Forms.RadioButton rbNew;
        private System.Windows.Forms.RadioButton rbLoad;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Button btnReset;
        //private Microsoft.VisualBasic.PowerPacks.ShapeContainer shapeContainer1;
        //private Microsoft.VisualBasic.PowerPacks.LineShape lineShape3;
        //private Microsoft.VisualBasic.PowerPacks.LineShape lineShape2;
        private System.Windows.Forms.ToolTip resetToolTip;
    }
}

