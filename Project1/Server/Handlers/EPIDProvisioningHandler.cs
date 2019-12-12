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



using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Runtime.InteropServices;

namespace DALSamplesServer
{
    class EPIDProvisioningHandler
    {
        private bool clientConnected;

        public EPIDProvisioningHandler()
        {

        }

        public void handleClientComm(object client)
        {
            try
            {
                TcpClient tcpClient = (TcpClient)client;
                Socket socket = tcpClient.Client;
                clientConnected = socket.Connected;
                while (clientConnected)
                {
                    //Receive EPID group ID from client
                    byte[] groupIDByteArray = new byte[4];                    
                    socket.Receive(groupIDByteArray, 0, 4, 0);
                    int groupID = Utils.ByteArrayToInt(groupIDByteArray);

                    //Create the provisioning data according to the groupID
                    byte[] provisioningData = CreateProvisioningData((uint)groupID);
                    
                    //Send the provisioning data to the client
                    int total = 0;
                    int size = provisioningData.Length;
                    int dataLeft = size;
                    int sent;
                    byte[] dataSize = new byte[4];
                    dataSize = BitConverter.GetBytes(size);
                    sent = socket.Send(dataSize);                    
                    while (total < size)
                    {
                        sent = socket.Send(provisioningData, total, dataLeft, SocketFlags.None);
                        total += sent;
                        dataLeft -= sent;
                    }
    
                }
                Console.WriteLine("EPID Provisioning Sample Client disconnected.\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }



        //Get the provisioning data(including the mathematic parameters and certificate) according to the platform EPID groupID
        public static byte[] CreateProvisioningData(uint groupID)
        {
            //provisioning data buffer
            DataStructs.PROVISIONING_DATA_SIGMA_1_1 provisioningData = new DataStructs.PROVISIONING_DATA_SIGMA_1_1();//create an instance of the SEND_SAFE_ID_PUB_KEY_IN_1_1 structure


            //Build provisioning data

            //header
            provisioningData.Header.ApiVersion = (uint)DataStructs.SIGMA_API_VERSION_1_1;
            provisioningData.Header.CommandId = (uint)DataStructs.COMMAND_ID.CMD_SEND_SAFEID_PUBKEY;
            provisioningData.Header.Status = 0;

            //CryptoContext contains the mathematic parameters
            provisioningData.CryptoContext = File.ReadAllBytes(DataStructs.DEBUG_SIGNED_BIN_PARAMS_CERT_FILE);

            //SafeIdCert contains the SIGMA1_0 certificate for the specific EPID group ID
            provisioningData.SafeIdCert = Utils.GetSpecificEpidCertificate_SIGMA_1_0(groupID);

            //tempCert contains the SIGMA1_1 certificate for the specific EPID group ID
            byte[] tempCert;
            tempCert = Utils.GetSpecificEpidCertificate_SIGMA_1_1(groupID);


            //Calculate the needed padding length
            int paddingLen = (4 - (DataStructs.VLR_HEADER_LEN + tempCert.Length) % 4) % 4;

            //Padding original cert so it will be dword aligned
            byte[] paddedCert = new byte[tempCert.Length + paddingLen];
            tempCert.CopyTo(paddedCert, 0);
            tempCert = paddedCert;

            //EPID group certificate
            provisioningData.X509GroupCert = new DataStructs.X509_GROUP_CERTIFICATE();
            provisioningData.X509GroupCert.vlrHeader.ID = (byte)DataStructs.VLR_ID_TYPE.X509_GROUP_CERTIFICATE_VLR_ID;
            provisioningData.X509GroupCert.vlrHeader.VLRLength =(short)(DataStructs.VLR_HEADER_LEN + tempCert.Length);
            provisioningData.X509GroupCert.vlrHeader.PaddedBytes = (byte)paddingLen;
            provisioningData.Header.BufferLength = (uint)(Marshal.SizeOf(typeof(DataStructs.PROVISIONING_DATA_SIGMA_1_1)) - Marshal.SizeOf(typeof(DataStructs.SESSMGR_CMD_HEADER)) + tempCert.Length); //cert has a different length

            byte[] provisioningDataArray = Utils.StructureToByteArray(provisioningData);

            //provisioningData contains the whole data: dataArray and the SIGMA1_1 certificate for the specific EPID group ID
            byte[] provisioningDataAndCert = new byte[provisioningDataArray.Length + tempCert.Length];

            Array.Copy(provisioningDataArray, provisioningDataAndCert, provisioningDataArray.Length);
            Array.Copy(tempCert, 0, provisioningDataAndCert, provisioningDataArray.Length, tempCert.Length);

            return provisioningDataAndCert;
        }     

    }
}
