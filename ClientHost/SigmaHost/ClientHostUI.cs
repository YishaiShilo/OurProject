/**
***
*** Copyright (c) 2013 - 2019 Intel Corporation. All Rights Reserved.
***
*** The information and source code contained herein is the exclusive
*** property of Intel Corporation. and may not be disclosed, examined
*** or reproduced in whole or in part without explicit written authorization
*** from the company.
***
*** ----------------------------------------------------------------------------
**/
using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ClientHostGUI
{
    public partial class Form1 : Form
    {
        #region Constants

        private enum StatusCode : int
        {
            STATUS_SUCCEEDED = 0,
            STATUS_FAILED = -1,
            INITIALIZE_FAILED = -2,
            INSTALL_FAILED = -3,
            OPEN_SESSION_FAILED = -4,
            UNRECOGNIZED_COMMAND = -10,
            FAILED_TO_GET_PUBLIC_KEY = -20,
            FAILED_TO_INITIALIZE_SIGMA = -30,
            INCORRECT_S2_BUFFER = -40,
            FAILED_TO_DISPOSE_SIGMA = -50,
            WRONG_INTEL_SIGNED_CERT_TYPE = -60,
            FAILED_TO_GET_S3_LEN = -70,
            FAILED_TO_PROCESS_S2 = -80,
            FAILED_TO_GET_SESSION_PARAMS = -90
        }

        private const int INT_SIZE = 4;
        private const int SIGMA_SAMPLE_ID = 3;
        private const string SERVER_NAME = "localhost";
        private const int SERVER_PORT = 27015;
        private const int S1_MESSAGE_LEN = 104;
        private const int ERROR_MESSAGE_LEN = 200;

        #endregion

        private byte[] S1MsgToSend = new byte[S1_MESSAGE_LEN];
        private byte[] s2Message;
        private int s2MsgLen;

        #region Server/Client communication

        private TcpClient client;
        private Socket socket;

        public byte[] ReceiveMessageFromServer(int MsgSize)
        {
            byte[] msg = new byte[MsgSize];
            int bytesToRead = MsgSize;
            int bytesRcvd = -1;
            while (bytesToRead > 0 && bytesRcvd != 0)
            {
                bytesRcvd = socket.Receive(msg, MsgSize - bytesToRead, bytesToRead, 0);
                bytesToRead -= bytesRcvd;
            }
            return msg;
        }

        private int ReceiveIntFromServer()
        {
            byte[] statusBytes = new byte[INT_SIZE];
            socket.Receive(statusBytes, 0, INT_SIZE, 0);
            return BitConverter.ToInt32(statusBytes, 0);
        }

        #endregion

        public Form1()
        {
            InitializeComponent();

            client = new TcpClient();

            // Update GUI
            btnGetS1Msg.Enabled = false;
            btnGetS2Msg.Enabled = false;
            btnGetS3Msg.Enabled = false;
            MaximizeBox = false;
        }

        /**
         * Connects to DAL samples server
         */
        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                client.Connect(SERVER_NAME, SERVER_PORT);
                socket = client.Client;

                socket.Send(BitConverter.GetBytes(1));

                // Update GUI
                btnConnect.Enabled = false;
                btnGetS1Msg.Enabled = true;
                lblServerStatus.Text = "Connected";
            }
            catch
            {
                lblServerStatus.Text = "Error has occurred. Is your server on?";
            }
        }

        private void btnGetS1Msg_Click(object sender, EventArgs e)
        {


            // Send S1 message
            Byte[] s1msg = BitConverter.GetBytes('h');
            socket.Send(s1msg);

            // Get S! processing status from server
            StatusCode status = (StatusCode)ReceiveIntFromServer();
            if (status == StatusCode.STATUS_FAILED)
            {
                lblGetS2MsgRet.Text = "Server failed to verify S1 message.";
                return;
            }


            // Update GUI
            lblGetS1MsgRet.Text = "S1 message send successfully, s1= " + s1msg[0];
            btnGetS1Msg.Enabled = false;
            btnGetS2Msg.Enabled = true;            
        }

        private void btnGetS2Msg_Click(object sender, EventArgs e)
        {           
            // Get S2 message from server
            s2MsgLen = ReceiveIntFromServer();
            s2Message = ReceiveMessageFromServer(s2MsgLen);

            // Update GUI
            btnGetS2Msg.Enabled = false;
            btnGetS3Msg.Enabled = true;
            lblGetS2MsgRet.Text = "S2 message recieved successfully. s2=" + s2Message[0];
        }

        private void btnGetS3Msg_Click(object sender, EventArgs e)
        {
            lblGetS3MsgRet.Text = string.Empty;

            byte[] S3MsgToSend = BitConverter.GetBytes('k');

            // Send S3 message length to server
            socket.Send(BitConverter.GetBytes(S3MsgToSend.Length));

            // Send S3 message to server for processing and verification
            socket.Send(S3MsgToSend);
            lblGetS3MsgRet.Text = "S3 message was created successfully. s3= " + S3MsgToSend[0];
            lblEnd.Text = "Now both parties send hi k";
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Hide();
            Application.Exit();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Hide();
            Application.Exit();
        }

    }
}
