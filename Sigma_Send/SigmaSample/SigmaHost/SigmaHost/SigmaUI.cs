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

namespace SigmaHostGUI
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

                socket.Send(BitConverter.GetBytes(SIGMA_SAMPLE_ID));

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
            lblGetS1MsgRet.Text = string.Empty;

            // Get S1 message from the trusted application
            IntPtr s1Msg = Marshal.AllocHGlobal(S1_MESSAGE_LEN);
            StatusCode status = (StatusCode)SigmaWrapper.GetS1Message(s1Msg);

            switch (status)
            {
                // S1 message was received successfully
                case StatusCode.STATUS_SUCCEEDED:
                    {
                        Marshal.Copy(s1Msg, S1MsgToSend, 0, S1MsgToSend.Length);

                        // Update GUI
                        lblGetS1MsgRet.Text = "S1 message created successfully.";
                        btnGetS1Msg.Enabled = false;
                        btnGetS2Msg.Enabled = true;
                        break;
                    }
                case StatusCode.INITIALIZE_FAILED:
                    lblGetS1MsgRet.Text = "Error: JHI Initializing failed.";
                    labelErrS1.Text = GetErrorMsgAsString();
                    break;
                case StatusCode.INSTALL_FAILED:
                    lblGetS1MsgRet.Text = "Error: Installing TA failed.";
                    labelErrS1.Text = GetErrorMsgAsString();
                    break;
                case StatusCode.OPEN_SESSION_FAILED:
                    lblGetS1MsgRet.Text = "Error: Opening a session failed.";
                    labelErrS1.Text = GetErrorMsgAsString();
                    break;
                case StatusCode.FAILED_TO_INITIALIZE_SIGMA:
                    lblGetS1MsgRet.Text = "Error: SIGMA Initializing failed.";
                    labelErrS1.Text = GetErrorMsgAsString();
                    break;
                case StatusCode.FAILED_TO_GET_PUBLIC_KEY:
                    lblGetS1MsgRet.Text = "Error: Failed to get public key.\nIs your platform EPID provisioned?";
                    labelErrS1.Text = GetErrorMsgAsString();
                    break;
                default:
                    lblGetS1MsgRet.Text = "Failed to perform send and receive operation in\norder to get S1 message.";
                    labelErrS1.Text = GetErrorMsgAsString();
                    break;
            }

            Marshal.FreeHGlobal(s1Msg);
        }

        private void btnGetS2Msg_Click(object sender, EventArgs e)
        {
            // Send S1 message to server for processing and verification
            socket.Send(S1MsgToSend);

            // Get S! processing status from server
            StatusCode status = (StatusCode)ReceiveIntFromServer();
            if (status == StatusCode.STATUS_FAILED)
            {
                lblGetS2MsgRet.Text = "Server failed to verify S1 message.";
                return;
            }

            // Get S2 message creation status from server
            status = (StatusCode)ReceiveIntFromServer();
            if (status == StatusCode.STATUS_FAILED)
            {
                // The failure cause may be that your firewall blocks the OCSP server access.
                lblGetS2MsgRet.Text = "Server failed to create S2 message.";
                return;
            }

            // Get S2 message from server
            s2MsgLen = ReceiveIntFromServer();
            s2Message = ReceiveMessageFromServer(s2MsgLen);

            // Update GUI
            btnGetS2Msg.Enabled = false;
            btnGetS3Msg.Enabled = true;
            lblGetS2MsgRet.Text = "S2 message created successfully.";
        }

        private void btnGetS3Msg_Click(object sender, EventArgs e)
        {
            lblGetS3MsgRet.Text = string.Empty;

            // Get S3 message length from the trusted application
            IntPtr s3MsgLen = Marshal.AllocHGlobal(INT_SIZE);
            StatusCode status = (StatusCode)SigmaWrapper.GetS3MessagLen(s2Message, s2MsgLen, s3MsgLen);
            switch (status)
            {
                case StatusCode.STATUS_SUCCEEDED:
                    {
                        byte[] S3MsgLenByteArray = new byte[INT_SIZE];
                        Marshal.Copy(s3MsgLen, S3MsgLenByteArray, 0, S3MsgLenByteArray.Length);
                        // Convert S3 message length from most significant byte first presentation to most significant byte last presentation
                        SwapArrBitEndianness(S3MsgLenByteArray);

                        int s3MessageLenInt = BitConverter.ToInt32(S3MsgLenByteArray, 0);

                        // Get S3 message from the trusted application
                        IntPtr s3Msg = Marshal.AllocHGlobal(s3MessageLenInt);
                        status = (StatusCode)SigmaWrapper.GetS3Message(s2Message, s2MsgLen, s3MessageLenInt, s3Msg);
                        switch (status)
                        {
                            // S3 message was received successfully
                            case StatusCode.STATUS_SUCCEEDED:
                                {
                                    // Send S3 message length to server
                                    socket.Send(BitConverter.GetBytes(s3MessageLenInt));

                                    // Send S3 message to server for processing and verification
                                    byte[] S3MsgToSend = new byte[s3MessageLenInt];
                                    Marshal.Copy(s3Msg, S3MsgToSend, 0, S3MsgToSend.Length);
                                    socket.Send(S3MsgToSend);
                                    lblGetS3MsgRet.Text = "S3 message was created successfully.";

                                    // Get S3 processing status from server
                                    status = (StatusCode)ReceiveIntFromServer();
                                    //if (status == StatusCode.STATUS_SUCCEEDED)
                                        //lblEnd.Text = "Now both parties have one shared secret and\ncan use any symmetrical encryption algorithm.";
                                    //else
                                        //lblEnd.Text = "Server failed to verify S3 message.";
                                    break;
                                }
                            case StatusCode.INCORRECT_S2_BUFFER:
                                lblGetS3MsgRet.Text = "Trusted application received an incorrect S2 message.";
                                break;
                            case StatusCode.FAILED_TO_PROCESS_S2:
                                lblGetS3MsgRet.Text = "Failed to process S2.";
                                break;
                            case StatusCode.WRONG_INTEL_SIGNED_CERT_TYPE:
                                lblGetS3MsgRet.Text = "Verifier's certificate is wrong Intel signed.";
                                break;
                            case StatusCode.FAILED_TO_GET_SESSION_PARAMS:
                                lblGetS3MsgRet.Text = "Failed to get session parameters.";
                                break;
                            case StatusCode.FAILED_TO_DISPOSE_SIGMA:
                                lblGetS3MsgRet.Text = "Failed to dispose SIGMA.";
                                break;
                            default:
                                lblGetS3MsgRet.Text = "Failed to perform send and receive operation in\norder to get S3 message.";
                                break;
                        }
                        Marshal.FreeHGlobal(s3Msg);
                        btnGetS3Msg.Enabled = false;
                        break;
                    }
                case StatusCode.FAILED_TO_GET_S3_LEN:
                    lblGetS3MsgRet.Text = "Error: Failed to get S3 message length.";
                    break;
                default:
                    lblGetS3MsgRet.Text = "Failed to perform send and receive operation in\norder to get S3 message length.";
                    break;
            }

            Marshal.FreeHGlobal(s3MsgLen);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Hide();
            SigmaWrapper.Close();
            Application.Exit();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Hide();
            SigmaWrapper.Close();
            Application.Exit();
        }

        #region Helper Methods

        private void SwapArrBitEndianness(byte[] Arr)
        {
            for (int i = 0; i < Arr.Length / 2; i++)
            {
                byte temp = Arr[i];
                Arr[i] = Arr[Arr.Length - i - 1];
                Arr[Arr.Length - i - 1] = temp;
            }
        }

        private string GetErrorMsgAsString()
        {
            IntPtr errorMessageIntPtr = Marshal.AllocHGlobal(ERROR_MESSAGE_LEN);
            IntPtr errorMsgLenIntPtr = Marshal.AllocHGlobal(INT_SIZE);

            SigmaWrapper.GetErrorMessage(errorMessageIntPtr, errorMsgLenIntPtr);

            byte[] messageLength = new byte[INT_SIZE];
            Marshal.Copy(errorMsgLenIntPtr, messageLength, 0, messageLength.Length);
            byte[] message = new byte[BitConverter.ToInt32(messageLength, 0)];
            Marshal.Copy(errorMessageIntPtr, message, 0, message.Length);

            Marshal.FreeHGlobal(errorMessageIntPtr);
            Marshal.FreeHGlobal(errorMsgLenIntPtr);

            return System.Text.Encoding.ASCII.GetString(message);
        }

        #endregion

        private void button1_Click(object sender, EventArgs e)
        {
            if (getServerData())
            {
                //image was received succssfully - close connection with the server
                //client.Client.Send(BitConverter.GetBytes(200));
                client.Close();

                //UI enablements
                //btnGetPicture.Enabled = false;
                //btnShow.Enabled = true;
                //flickerTimer.Enabled = true;
                //btnSave.Enabled = true;
                //lblGetPicStatus.Text = "Image is loaded. Click Show Image button!";
                //lblGetPicStatus.ForeColor = Color.Green;
            }
            else
            {
                //lblGetPicStatus.Text = "Get image failed.";
                //lblGetPicStatus.ForeColor = Color.Red;
            }
        }

        private bool getServerData()
        {
            try
            {
                //request encrypted image from the server
                byte[] cmdGetImage = BitConverter.GetBytes(100);
                socket.Send(cmdGetImage);

                byte[] datasize = new byte[4];
                datasize = new byte[4];
                int recv = socket.Receive(datasize, 0, 4, 0);

                int size = BitConverter.ToInt32(datasize, 0);
                int total = 0;
                int dataleft = size;
                byte[] serverData = new byte[size];

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
        /*
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
                        btnSave.Enabled = true;
                        lblGetPicStatus.Text = "Image is loaded. Click Show Image button!";
                        lblGetPicStatus.ForeColor = Color.Green;
                    }
                    else
                    {
                        lblGetPicStatus.Text = "Get image failed.";
                        lblGetPicStatus.ForeColor = Color.Red;
                    }
                }


            */
    }
    
}
