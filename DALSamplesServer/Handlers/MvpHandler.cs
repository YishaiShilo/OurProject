using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using DALSamplesServer.Utils;


namespace DALSamplesServer.Handlers
{
    class MvpHandler : SampleHandler
    {
        private bool isClientConnected;
        // Status codes 
        private const int STATUS_SUCCEEDED = 0;
        private const int STATUS_FAILED = -1;

        public override void HandleClientCommunication(object client)
        {
            try
            {
                TcpClient tcpClient = (TcpClient)client;
                Socket socket = tcpClient.Client;

                isClientConnected = socket.Connected;
                while (isClientConnected)
                {
                    // Receive S1 message from client
                    byte[] s1Msg = socket.ReceiveMessage(1);
                    if (s1Msg.Length == 0)
                    {
                        socket.SendInt(STATUS_FAILED);
                        continue;
                    }
                    Console.WriteLine("got s1:");

                    Console.WriteLine(s1Msg[0]);

                    // Send S1 message processing status to client
                    socket.SendInt(STATUS_SUCCEEDED);

                    // Send S2 message length to client
                    socket.SendInt(1);


                    // Send the S2 message to the client
                    byte[] s2Message = BitConverter.GetBytes('i');
                    socket.SendMessage(s2Message);
                    Console.WriteLine("send s2:");
                    Console.WriteLine(s2Message[0]);

                    // Receive S3 message length from client
                    int s3MessageLen = socket.ReceiveMessageAsInt();
                    // Receive S3 message from client
                    byte[] s3Msg = socket.ReceiveMessage(s3MessageLen);

                    // Send S3 verification status to client                            
                    if (s3Msg.Length != s3MessageLen)
                    {
                        socket.SendInt(STATUS_FAILED);
                        continue;
                    }
                    Console.WriteLine("got s3:");
                    Console.WriteLine(s3Msg[0]);

                    socket.SendInt(STATUS_SUCCEEDED);
                }

                Console.WriteLine("Protected Output Sample Client disconnected.\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
