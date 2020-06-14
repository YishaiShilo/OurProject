using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Drawing;

namespace DALSamplesServer
{
    class Authentication
    {
        private const int STATUS_SUCCEEDED = 0;
        private const int STATUS_FAILED = -1;

        private bool clientConnected;
        private byte[] decryptedId;
        private Socket socket;
        private Encryption encryption;
   
        public Authentication(Encryption encrytionHandler)
        {
            encryption = encrytionHandler;
        }

        private bool getAutKey(byte[] encryptedId)
        {
            decryptedId = new byte[16];
            try
            {
                decryptedId = encryption.DecryptBytes(encryptedId);
                Console.WriteLine("decryptedId: " + Encoding.UTF8.GetString(decryptedId));
                Console.WriteLine("decryptedId: " + BitConverter.ToString(decryptedId));

                socket.Send(BitConverter.GetBytes(STATUS_SUCCEEDED));
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("exception at decryption");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.GetType());
                return false;
            }
        }

        public bool handleClientComm(object client)
        {
            try
            {
                TcpClient tcpClient = (TcpClient)client;
                socket = tcpClient.Client;
                clientConnected = socket.Connected;
                byte[] statusByte = new byte[DataStructs.INT_SIZE];
                while (clientConnected)
                {
                    byte[] encryptedId = new byte[16];
                    socket.Receive(encryptedId, 0, 16, 0);
                    Console.WriteLine(Encoding.UTF8.GetString(encryptedId));
                    if (getAutKey(encryptedId))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
    }
}
