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

namespace DALSamplesServer.Utils
{
    public static class SocketsUtils
    {
        public static void SendMessage(this Socket Socket, byte[] data)
        {
            Socket.SendInt(data.Length);
            Socket.SendByteArray(data);
        }

        public static void SendByteArray(this Socket Socket, byte[] data)
        {
            int totalSentBytes = 0;
            int leftDataSize = data.Length;
            int sentDataSize = 0;

            while (totalSentBytes < data.Length)
            {
                sentDataSize = Socket.Send(data, totalSentBytes, leftDataSize, SocketFlags.None);
                totalSentBytes += sentDataSize;
                leftDataSize -= sentDataSize;
            }
        }

        public static void SendInt(this Socket Socket, int data)
        {
            byte[] dataArray = new byte[DataStructs.INT_SIZE];
            dataArray = BitConverter.GetBytes(data);
            Socket.SendByteArray(dataArray);
        }

        public static byte[] ReceiveMessage(this Socket Socket, int MsgSize)
        {
            byte[] msg = new byte[MsgSize];
            int bytesToRead = MsgSize;
            int bytesRcvd = -1;
            while (bytesToRead > 0 && bytesRcvd != 0)
            {
                bytesRcvd = Socket.Receive(msg, MsgSize - bytesToRead, bytesToRead, 0);
                bytesToRead -= bytesRcvd;
            }
            return msg;
        }

        public static int ReceiveMessageAsInt(this Socket socket)
        {
            byte[] msgBytes = socket.ReceiveMessage(DataStructs.INT_SIZE);
            return BitConverter.ToInt32(msgBytes, 0);
        }

    }
}
