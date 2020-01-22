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
using System.Threading;
using System.Net;
using DALSamplesServer.Handlers;
using DALSamplesServer.Utils;

namespace DALSamplesServer
{
    class Server
    {
        private TcpListener tcpListener;
        private Thread listenThread;
        private TcpClient clientSocket;

        // The IDs of the samples that connect to the server
        private const int MVP_SAMPLE = 1;
       
        private const int SERVER_PORT = 27015;

        public Server()
        {
            Start();
        }

        #region TCP Server

        private void Start()
        {
            // Start listening for clients
            tcpListener = new TcpListener(IPAddress.Any, SERVER_PORT);
            listenThread = new Thread(new ThreadStart(ListenForClients));
            listenThread.Start();
        }

        public void Stop()
        {
            // Stop listening for connections
            if (clientSocket != null)
                clientSocket.Close();
            if (tcpListener != null)
                tcpListener.Stop();
            if (listenThread != null)
                listenThread.Abort();
        }

        private void AcceptConnection()
        {
            Console.WriteLine("\nWaiting for connections at port {0}...\n", SERVER_PORT);
            // Block until a client connects to the server
            clientSocket = tcpListener.AcceptTcpClient();
            Console.WriteLine("\nConnection accepted from " + clientSocket.Client.LocalEndPoint);
        }

        #endregion

        private void ListenForClients()
        {
            try
            {
                tcpListener.Start();
                Console.WriteLine("The server is running...");

                while (true)
                {
                    AcceptConnection();

                    // Client should send an integer specifying which sample it is
                    int sampleID = clientSocket.Client.ReceiveMessageAsInt();

                    SampleHandler sampleHandler = null;

                    switch (sampleID)
                    {
                        case MVP_SAMPLE:
                            {
                                Console.WriteLine("Connected client is MVP Sample.");
                                sampleHandler = new MvpHandler();
                            }
                            break;
                        
                        default: break;
                    }

                    // Create a thread to handle communication with the connected client
                    if (sampleHandler != null)
                    {
                        Thread clientThread = new Thread(new ParameterizedThreadStart(sampleHandler.HandleClientCommunication));
                        clientThread.Start(clientSocket);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("An exception was thrown. \nError message:");
                Console.WriteLine(e.Message);
            }
        }
    }
}
