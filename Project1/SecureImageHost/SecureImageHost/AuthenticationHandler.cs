using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Runtime.InteropServices;


namespace CSharpClientUI
{
    class AuthenticationHandler
    {
        //Error codes
        public const int UNRECOGNIZED_COMMAND = -10;
        public const int FAILED_TO_GET_PUBLIC_KEY = -20;
        public const int FAILED_TO_INITIALIZE_SIGMA = -30;
        public const int INCORRECT_S2_BUFFER = -40;
        public const int FAILED_TO_DISPOSE_SIGMA = -50;
        public const int WRONG_INTEL_SIGNED_CERT_TYPE = -60;
        public const int FAILED_TO_GET_S3_LEN = -70;
        public const int FAILED_TO_PROCESS_S2 = -80;
        public const int FAILED_TO_GET_SESSION_PARAMS = -90;
        private const int STATUS_SUCCEEDED = 0;
        private const int STATUS_FAILED = -1;
        private const int INITIALIZE_FAILED = -2;
        private const int INSTALL_FAILED = -3;
        private const int OPEN_SESSION_FAILED = -4;

        private const int INT_SIZE = 4;
        private const int S1_MESSAGE_LEN = 104;

        byte[] S1MsgToSend = new byte[S1_MESSAGE_LEN];
        byte[] s2Message;
        int s2MsgLen;
        static bool isConnected;
        byte[] serverData;
        byte[] statusBytes = new byte[INT_SIZE];


        private Socket socket;
        public AuthenticationHandler(Socket socket)
        {
            this.socket = socket;
        }

        public bool makeKeys()
        {
            if (!processS1())
                return false;
            if (!processS2())
                return false;
            if (!processS3())
                return false;
            return true;
        }

        private bool processS1()
        {
            IntPtr s1Msg = Marshal.AllocHGlobal(S1_MESSAGE_LEN);
            Console.WriteLine("before s1");
            int status = SecureImageHostWrapper.GetS1Message(s1Msg);
            Console.WriteLine(s1Msg);
            Console.WriteLine("after s1");
            Console.WriteLine(status);
            if (status != STATUS_SUCCEEDED)
            {
                return false;
            }
            else
            {
                Marshal.Copy(s1Msg, S1MsgToSend, 0, S1MsgToSend.Length);
                Marshal.FreeHGlobal(s1Msg);
                return true;
            }
        }

        private bool processS2()
        {
            int status;
            //Send S1 message to server for processing and verification
            socket.Send(S1MsgToSend);

            //Get status from server
            socket.Receive(statusBytes, 0, INT_SIZE, 0);
            status = BitConverter.ToInt32(statusBytes, 0);
            if (status == STATUS_FAILED)
            {
                //lblGetS2MsgRet.Text = "Server failed to verify S1 message.";

                return false;
            }
            else
            {
                //Get status from server
                socket.Receive(statusBytes, 0, INT_SIZE, 0);
                status = BitConverter.ToInt32(statusBytes, 0);

                //server response
                //The cause may be that your firewall blocks the OCSP server access.
                if (status == STATUS_FAILED)
                {
                    //lblGetS2MsgRet.Text = "Server failed to create S2 message.";
                    return false;
                }
                else
                {
                    //Get S2 message from server
                    byte[] datasize = new byte[INT_SIZE];
                    int recv = socket.Receive(datasize, 0, INT_SIZE, 0);
                    s2MsgLen = BitConverter.ToInt32(datasize, 0);
                    int total = 0;
                    int dataleft = s2MsgLen;
                    serverData = new byte[s2MsgLen];
                    while (total < s2MsgLen)
                    {
                        recv = socket.Receive(serverData, total, dataleft, 0);
                        if (recv == 0)
                        {
                            break;
                        }
                        total += recv;
                        dataleft -= recv;
                    }
                    s2Message = serverData;
                    //btnGetS2Msg.Enabled = false;
                    //btnGetS3Msg.Enabled = true;
                    //lblGetS2MsgRet.Text = "S2 message created successfully.";
                    return true;
                }
            }
        }

        

        private bool processS3()
        {
            return true;
        }

    }
}
