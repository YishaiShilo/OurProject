using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace DALSamplesServer
{
    class SecureImageHandler
    {

        private bool clientConnected;
        private const int S1_MSG_LEN = 104;








        public void handleClientComm(object client)
        {
            //SIGMAHandler sigHan = new SIGMAHandler();
            //sigHan.handleClientComm(client);
            //var g = sigHan.GaGb;
            try
            {
                TcpClient tcpClient = (TcpClient)client;
                Socket socket = tcpClient.Client;
                clientConnected = socket.Connected;
                byte[] statusByte = new byte[DataStructs.INT_SIZE];
                while (clientConnected)
                {
                    //Receive S1 message from client
                    byte[] s1Msg = new byte[S1_MSG_LEN];
                    socket.Receive(s1Msg, 0, S1_MSG_LEN, 0);

                    ////Process S1 message
                    //CdgStatus status = ProcessS1Message(s1Msg);
                    ////If S1 message processing succeeded
                    //if (status == CdgStatus.CdgStsOk)
                    //{
                    //    socket.Send(BitConverter.GetBytes(STATUS_SUCCEEDED));

                    //    //Get S2 Message
                    //    byte[] s2Message;

                    //    if (GetS2Message(out s2Message))
                    //    {
                    //        socket.Send(BitConverter.GetBytes(STATUS_SUCCEEDED));

                    //        //Send the S2 message to the client
                    //        int total = 0;
                    //        int size = s2Message.Length;
                    //        int dataleft = size;
                    //        int sent;
                    //        byte[] datasize = new byte[DataStructs.INT_SIZE];
                    //        datasize = BitConverter.GetBytes(size);
                    //        sent = socket.Send(datasize);

                    //        while (total < size)
                    //        {
                    //            sent = socket.Send(s2Message, total, dataleft, SocketFlags.None);
                    //            total += sent;
                    //            dataleft -= sent;
                    //        }


                    //        //Receive S3 message length from client
                    //        byte[] s3MsgLen = new byte[DataStructs.INT_SIZE];
                    //        socket.Receive(s3MsgLen, 0, DataStructs.INT_SIZE, 0);
                    //        int s3MessageLen = Utils.ByteArrayToInt(s3MsgLen);

                    //        //Receive S3 message from client
                    //        byte[] s3Msg = new byte[s3MessageLen];
                    //        int bytesNeeded = s3MessageLen;
                    //        int bytesReceived = 0;
                    //        while (bytesNeeded > 0)
                    //        {
                    //            bytesReceived = socket.Receive(s3Msg, s3MessageLen - bytesNeeded, bytesNeeded, 0);
                    //            bytesNeeded -= bytesReceived;
                    //        }


                    //        //Send S3 verification status to client                            
                    //        if (VerifyS3Message(s3Msg))
                    //        {
                    //            socket.Send(BitConverter.GetBytes(STATUS_SUCCEEDED));
                    //            return;
                    //        }
                    //        else
                    //        {
                    //            socket.Send(BitConverter.GetBytes(STATUS_FAILED));
                    //        }
                    //    }
                    //    else
                    //    {
                    //        socket.Send(BitConverter.GetBytes(STATUS_FAILED));
                    //    }

                    //}
                    //else
                    //{
                    //    socket.Send(BitConverter.GetBytes(STATUS_FAILED));
                    //}
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
