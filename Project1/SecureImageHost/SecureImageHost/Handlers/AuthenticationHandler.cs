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
using WysHost;

namespace CSharpClientUI
{
    class AuthenticationHandler
    {
        //Error codes
        public const int UNRECOGNIZED_COMMAND = -10;
        public const int WRONG_INTEL_SIGNED_CERT_TYPE = -60;
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
        WysForm wysIns;

        private Socket socket;
        public AuthenticationHandler(Socket socket)
        {

            this.socket = socket;
            this.wysIns = new WysForm();
            wysIns.ShowDialog();      
        }


        public bool sendAutKey(byte[] AutId)
        {
            byte[] encryptedId = new byte[16];

            int status = SecureImageHostWrapper.sendAuthenticationId(AutId, AutId.Length, encryptedId);
            if (status != STATUS_SUCCEEDED)
            {
                return false;
            }
            Console.WriteLine(Encoding.UTF8.GetString(encryptedId));
            socket.Send(encryptedId);
            socket.Receive(statusBytes, 0, INT_SIZE, 0);
            status = BitConverter.ToInt32(statusBytes, 0);
            if (status == STATUS_FAILED)
            {
                //lblGetS2MsgRet.Text = "Server failed to verify S1 message.";
                Console.WriteLine("Server failed to decrypt autId");
                return false;
            }
            else
            {
                Console.WriteLine("Server decrypt autId");
                return true;
            }

        }
    }
}
