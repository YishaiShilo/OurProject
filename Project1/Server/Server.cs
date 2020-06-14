/**
***
*** Copyright  (C) 2014 Intel Corporation. All rights reserved.
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
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;

using System.Windows.Forms;
using System.IO;
using System.Security.Cryptography;
using System.Drawing;

namespace DALSamplesServer
{
    class Server
    {
        private TcpListener tcpListener;
        private Thread listenThread;
        private uint clientId;
        public byte[] symmetric_key;
        public byte[] encrypted_image;

        private const int PROTECTED_OUTPUT_SAMPLE = 1;
        private const int EPID_PROVISIONING_SAMPLE = 2;
        private const int EPID_SIGNING_SAMPLE = 3;
        private const int SIGMA_SAMPLE = 4;
        private const int SECURE_IMAGE = 5;
        public Server()
        {
            //start listening for clients
            //IPAddress addr = IPAddress.Parse("192.168.14.39"); 
            tcpListener = new TcpListener(IPAddress.Any, 27015);
            //tcpListener = new TcpListener(addr, 27015);
            listenThread = new Thread(new ThreadStart(listenForClients));
            listenThread.Start();
        }

        public void stop()
        {
            //stop listening for connections
            listenThread.Abort();
            tcpListener.Stop();
        }

        private void listenForClients()
        {
            tcpListener.Start();
            Console.WriteLine("The server is running at port 27015...");
            Console.WriteLine("The local End point is  :" +
                              tcpListener.LocalEndpoint);
            Console.WriteLine("Waiting for a connections....\n");
            while (true)
            {
                //blocks until a client has connected to the server
                TcpClient client = this.tcpListener.AcceptTcpClient();
                Console.WriteLine("\nConnection accepted from " + client.Client.LocalEndPoint);

                //client should send an integer specifying which kind of client it is
                byte[] cmd = new byte[4];
                client.Client.Receive(cmd, 0, 4, 0);
                int sampleType = BitConverter.ToInt32(cmd, 0);
                switch (sampleType)
                {
                    case EPID_PROVISIONING_SAMPLE:
                        {
                            Console.WriteLine("Connected client is EPID Provisioning Sample");
                            //create a thread to handle communication 
                            //with connected client
                            EPIDProvisioningHandler ch = new EPIDProvisioningHandler();
                            Thread clientThread = new Thread(new ParameterizedThreadStart(ch.handleClientComm));
                            clientThread.Start(client);
                        }
                        break;
                    case EPID_SIGNING_SAMPLE:
                        {
                            Console.WriteLine("Connected client is EPID Signing Sample");
                            //create a thread to handle communication 
                            //with connected client
                            EPIDSigningHandler ch = new EPIDSigningHandler();
                            Thread clientThread = new Thread(new ParameterizedThreadStart(ch.handleClientComm));
                            clientThread.Start(client);
                        }
                        break;
                    case SECURE_IMAGE:
                        {
                            Console.WriteLine("Connected client is SECURE IMAGE");
                            //create a thread to handle communication 
                            //with connected client
                            SecureImageHandler ch = new SecureImageHandler();
                            Thread clientThread = new Thread(new ParameterizedThreadStart(ch.handleClientComm));
                            clientThread.Start(client);
                        }
                        break;
                    default: break;
                }
            }
        }
    }
}
