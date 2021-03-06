/**
***
*** Copyright  (C) 1985-2015 Intel Corporation. All rights reserved.
***
*** The information and source code contained herein is the exclusive
*** property of Intel Corporation. and may not be disclosed, examined
*** or reproduced in whole or in part without explicit written authorization
*** from the company.
***
*** ----------------------------------------------------------------------------
**/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Net.Sockets;

using System.Windows.Forms;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Runtime.InteropServices;

namespace CSharpClientUI
{
    public partial class SecureImageUI : Form
    {
        private Thread refreshThread;
        byte[] serverData;
        TcpClient client;
        Socket socket;
        EncryptionHandler encryptionHandler;
        AuthenticationHandler autHandler;
        int flickerCounter;
        bool sessionExists;
        int port = 27015;
        int bufferSize = 200;
        string serverName = "localhost";
        private const int EXIT = 1;
        private const int SENDING_KEYS = 2;
        private const int REQUESTING_IMAGE = 3;
        private const int PROTECTED_OUTPUT_SAMPLE = 1;
        private const int SECURE_IMAGE = 5;
        private const int SENDING_GROUP_ID = 4;

        const int EPID_NONCE_LEN = 32;
        const int EPID_SIGNATURE_LEN = 569;



        public SecureImageUI()
        {
            InitializeComponent();        
            btnGetPicture.Enabled = false;
            btnShow.Enabled = false;
            //btnSave.Enabled = false;
            btnSendKeys.Enabled = false;
            sessionExists = false;
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            //reset previous image UI
            resetUI();
            try
            {
                //connect to server
                client = new TcpClient();

                //IPAddress ipAddress = Dns.GetHostEntry("www.google.com").AddressList[0];  //added
                //Console.WriteLine(ipAddress.ToString());  //added
                //IPAddress addr = IPAddress.Parse("192.168.14.103");  //added.  need this
                //IPEndPoint ipEndPoint = new IPEndPoint(addr, 27015); //added.  need this
                //client.Connect(ipEndPoint); //added.  need this

                client.Connect(serverName, port); //was before
                socket = client.Client;

                socket.Send(BitConverter.GetBytes(SECURE_IMAGE));

                //UI enablements
                btnConnect.Enabled = false;
                btnSendKeys.Enabled = true;
                //rbLoad.Enabled = false;
                lblServerStatus.Text = "Connected";
                lblServerStatus.ForeColor = Color.Green;
            }
            catch (Exception)
            {
                //connection to server failed
                lblServerStatus.Text = "Error ocuured. Is your server on?";
                lblServerStatus.ForeColor = Color.Red;
            }


        }

        private void btnSigma_Click(object sender, EventArgs e)
        {
            lblKeyStatus.Text = "Sending, this might take a minute or two...";
            lblKeyStatus.Refresh();
            StringBuilder builder = new StringBuilder(bufferSize);
            encryptionHandler = new EncryptionHandler(socket);
            Console.WriteLine("click sigma");
            bool res = encryptionHandler.makeKeys(builder);
            if (res)
            {
                btnSendKeys.Enabled = false;
                btnGetPicture.Enabled = true;
                lblKeyStatus.ForeColor = Color.Green;
                lblKeyStatus.Text = "Key was sent successfully";
            }
            else
            {
                lblKeyStatus.Text = "Failed to send key. " +builder.ToString();
                lblKeyStatus.ForeColor = Color.Red;
            }
        }

        private void btnGetPicture_Click(object sender, EventArgs e)
        {
            //request image from the server
            if (getServerData())
            {
                //image was received succssfully - close connection with the server
                client.Client.Send(BitConverter.GetBytes(EXIT));
                client.Close();

                //UI enablements
                btnGetPicture.Enabled = false;
                btnShow.Enabled = true;
                flickerTimer.Enabled = true;
                //btnSave.Enabled = true;
                lblGetPicStatus.Text = "Image is loaded. Click Show Image button!";
                lblGetPicStatus.ForeColor = Color.Green;
            }
            else
            {
                lblGetPicStatus.Text = "Get image failed.";
                lblGetPicStatus.ForeColor = Color.Red;
            }
        }

        private bool getServerData()
        {
            try
            {
                //request encrypted image from the server
                byte[] cmdGetImage = BitConverter.GetBytes(REQUESTING_IMAGE);
                socket.Send(cmdGetImage);

                byte[] datasize = new byte[4];
                datasize = new byte[4];
                int recv = socket.Receive(datasize, 0, 4, 0);

                int size = BitConverter.ToInt32(datasize, 0);
                int total = 0;
                int dataleft = size;
                serverData = new byte[size];

                //receive the image
                while (total < size)
                {
                    recv = socket.Receive(serverData, total, dataleft, 0);
                    if (recv == 0)
                    {
                        break;
                    }
                    total += recv;
                    dataleft -= recv;
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            resetUI();
            //open file dialog
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "All files (*.*)| *.*";
            dialog.Multiselect = false;
            DialogResult result = dialog.ShowDialog();
            if (result == DialogResult.Cancel)
            {
                return;
            }
            //read the image file
            string path = dialog.FileName;
            serverData = File.ReadAllBytes(path);

            //UI code
            btnShow.Enabled = true;
            //btnSave.Enabled = true;
            //lblLocalPictureStatus.Text = path.ToString() + " image is loaded. Click Show Image button!";
            //lblLocalPictureStatus.ForeColor = Color.Green;
            flickerTimer.Enabled = true;

        }

        private void btnShow_Click(object sender, EventArgs e)
        {
            //disable the button flickering
            flickerTimer.Enabled = false;
            flickerCounter = 0;
            btnShow.BackColor = SystemColors.Control;
            //if there is a PAVP session running - close it
            if (sessionExists)
            {
                //stop refreshing 
                if (refreshThread != null)
                    refreshThread.Abort();
                //close session
                SecureImageHostWrapper.closePavpSession();
                sessionExists = false;
            }
            //request library to show the image
            StringBuilder builder = new StringBuilder(bufferSize);
            if (SecureImageHostWrapper.showImage(serverData, panel.Handle,builder))
            {
                //get number of times  presented image can be shown again
                sessionExists = true;
                //lblNumViews.Text = SecureImageHostWrapper.getRemainingTimes().ToString();
                //start a refresh thred to refresh the view periodically
                refreshThread = new Thread(new ThreadStart(refresh));
                refreshThread.Start();
                //UI code
                btnShow.Enabled = false;
                //rbLoad.Enabled = true;
                rbNew.Enabled = true;
                //if (rbLoad.Checked)
                //    rbLoad_CheckedChanged(null, null);
                //else
                //    rbNew_CheckedChanged(null, null);
            }
            else
            {
                MessageBox.Show("Failed to show image. "+builder.ToString());
            }           
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            //open file dialog
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "All files (*.*)| *.*";
            dialog.FileName = "ProtectedPicture.bin";
            DialogResult result = dialog.ShowDialog();
            if (result == DialogResult.Cancel)
                return;

            string filename = dialog.FileName;
            string dir = Path.GetDirectoryName(filename);
            
            //write curent image data to the file
            File.WriteAllBytes(Path.Combine(dir, filename), serverData);
        }

        private void refresh()
        {
            while (true)
            {
                //refresh the image
                SecureImageHostWrapper.refresh();
                Thread.Sleep(100);
            }
        }

        private void formClosed(object sender, FormClosedEventArgs e)
        {
            Hide();
            //stop refreshiong
            if (refreshThread != null)
                refreshThread.Abort();

            StringBuilder builder = new StringBuilder(bufferSize);
            //de-init library
            SecureImageHostWrapper.close(builder);
            //exit application
            Application.Exit();
        }

        private void rbNew_CheckedChanged(object sender, EventArgs e)
        {
            //btnLoad.Enabled = false;
            btnConnect.Enabled = true;
        }

        private void rbLoad_CheckedChanged(object sender, EventArgs e)
        {
            //btnLoad.Enabled = true;
            btnConnect.Enabled = false;
        }

        private void flicker_Tick(object sender, EventArgs e)
        {
            //tick to make show button flicker
            if (btnShow.BackColor == SystemColors.Control)
                btnShow.BackColor = Color.FromArgb(33,197,255);
            else
                btnShow.BackColor = SystemColors.Control;
            flickerCounter++;
            if (flickerCounter == 9)
            {
                //stop after 4 flickers
                flickerCounter = 0;
                flickerTimer.Enabled = false;
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnAbout_Click(object sender, EventArgs e)
        {
            AboutForm frm = new AboutForm();
            frm.ShowDialog();
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            ////stop refresh
            //if (refreshThread != null)
            //    refreshThread.Abort();
            ////close session
            //SecureImageHostWrapper.closePavpSession();
            //resetUI();
            ////et library
            //StringBuilder builder = new StringBuilder(bufferSize);
            //if (SecureImageHostWrapper.resetSolution(builder))
            //    MessageBox.Show("Reset was successful!");
            //else
            //    MessageBox.Show("Failed to reset solution. " + builder.ToString());
        }

        private void resetUI()
        {
            //reset UI

            lblGetPicStatus.Text="";
            lblKeyStatus.Text="";
            //lblNumViews.Text="?";
            lblServerStatus.Text="Disconnected";
            //lblLocalPictureStatus.Text="No image is loaded";

            Color color=Color.FromArgb(0, 66, 129);
            lblGetPicStatus.ForeColor = color;
            lblKeyStatus.ForeColor = color;
            lblServerStatus.ForeColor = color;
            //lblLocalPictureStatus.ForeColor = color;           
        }

        private void install_Click(object sender, EventArgs e)
        {
            SecureImageHostWrapper.installApplet();
        }

        private void passwordButtonClick(object sender, EventArgs e)
        {
            //textBox2.Text
            
            byte[] AuthenticationId = Encoding.ASCII.GetBytes(this.passwordBox.Text);
            Console.WriteLine(this.passwordBox.Text);
            autHandler = new AuthenticationHandler(socket);
            bool res = autHandler.sendAutKey(AuthenticationId);
            if (res)
            {
                this.passwordLabel.Text = "password was sent successfully";
            }
            else
            {
                this.passwordLabel.Text = "password failed to be sent";
            }
        }

        private void SecureImageUI_Load(object sender, EventArgs e)
        {

        }
    }
}