using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;


namespace DALSamplesServer
{
    class SHandler
    {


        //private bool clientConnected;
        //rivate const int S1_MSG_LEN = 104;

        //public void handleClientComm(object client)
        //{
        //    try
        //    {
        //        TcpClient tcpClient = (TcpClient)client;
        //        Socket socket = tcpClient.Client;
        //        clientConnected = socket.Connected;
        //        byte[] statusByte = new byte[DataStructs.INT_SIZE];
        //        while (clientConnected)
        //        {
        //            //Receive S1 message from client
        //            byte[] s1Msg = new byte[S1_MSG_LEN];
        //            socket.Receive(s1Msg, 0, S1_MSG_LEN, 0);

        //            //Process S1 message
        //            CdgStatus status = ProcessS1Message(s1Msg);
        //            //If S1 message processing succeeded
        //            if (status == CdgStatus.CdgStsOk)
        //            {
        //                socket.Send(BitConverter.GetBytes(STATUS_SUCCEEDED));


        //            }
        //        }
        //        Console.WriteLine("Protected Output Sample Client disconnected.\n");
        //    }
        //    catch (Exception ex)
        //    {
        //    Console.WriteLine(ex.Message);
        //    }
        //}


    }

    
}
