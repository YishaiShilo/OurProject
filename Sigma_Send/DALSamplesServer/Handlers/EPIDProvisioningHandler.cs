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
using System.IO;
using System.Runtime.InteropServices;
using DALSamplesServer.Utils;

namespace DALSamplesServer.Handlers
{
    class EPIDProvisioningHandler : SampleHandler
    {
        private bool isClientConnected;
        public override void HandleClientCommunication(object Client)
        {
            try
            {
                TcpClient tcpClient = (TcpClient)Client;
                Socket socket = tcpClient.Client;

                isClientConnected = socket.Connected;
                while (isClientConnected)
                {
                    // Receive EPID group ID from client
                    uint groupID = (uint)socket.ReceiveMessageAsInt();

                    // Create the provisioning data according to the group ID
                    byte[] provisioningData = CreateProvisioningData(groupID);

                    // Send the provisioning data to the client
                    socket.SendMessage(provisioningData);
                }
                Console.WriteLine("EPID Provisioning Sample Client disconnected.\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /**
         * Gets the provisioning data (including the mathematic parameters and certificate) according to the platform EPID group ID
         */
        private static byte[] CreateProvisioningData(uint GroupID)
        {
            // Provisioning data buffer
            EPIDDataStructs.ProvisioningDataSigma1_1 provisioningData = new EPIDDataStructs.ProvisioningDataSigma1_1();

            // Build provisioning data

            // Header
            provisioningData.Header.ApiVersion = (uint)SigmaDataStructs.SIGMA_API_VERSION_1_1;
            provisioningData.Header.CommandId = (uint)EPIDDataStructs.CommandId.CMD_SEND_SAFEID_PUBKEY;
            provisioningData.Header.Status = 0;

            // CryptoContext - contains the mathematic parameters
            provisioningData.CryptoContext = File.ReadAllBytes(EPIDDataStructs.DEBUG_SIGNED_BIN_PARAMS_CERT_FILE);

            // SafeIdCert - contains the SIGMA1_0 certificate for the specific EPID group ID
            provisioningData.SafeIdCert = SigmaUtils.GetSpecificEpidCertificate_SIGMA_1_0(GroupID);

            // tempCert contains the SIGMA 1.1 certificate for the specific EPID group ID
            byte[] tempCert = SigmaUtils.GetSpecificEpidCertificate_SIGMA_1_1(GroupID);

            // Calculate the needed padding length
            int paddingLen = (4 - (DataStructs.VLR_HEADER_LEN + tempCert.Length) % 4) % 4;

            // Pad original cert so it will be word aligned
            byte[] paddedCert = new byte[tempCert.Length + paddingLen];
            tempCert.CopyTo(paddedCert, 0);
            tempCert = paddedCert;

            // Set EPID group certificate
            provisioningData.X509GroupCert = new EPIDDataStructs.X509GroupCertificate();
            provisioningData.X509GroupCert.vlrHeader.ID = (byte)DataStructs.VlrIdType.X509_GROUP_CERTIFICATE_VLR_ID;
            provisioningData.X509GroupCert.vlrHeader.VLRLength = (short)(DataStructs.VLR_HEADER_LEN + tempCert.Length);
            provisioningData.X509GroupCert.vlrHeader.PaddedBytes = (byte)paddingLen;
            provisioningData.Header.BufferLength = (uint)(Marshal.SizeOf(typeof(EPIDDataStructs.ProvisioningDataSigma1_1)) - Marshal.SizeOf(typeof(EPIDDataStructs.SessionMgrCmdHdr)) + tempCert.Length); //cert has a different length

            byte[] provisioningDataArray = GeneralUtils.StructureToByteArray(provisioningData);

            // The provisioning data contains the whole data: dataArray and the SIGMA 1.1 certificate for the specific EPID group ID
            byte[] provisioningDataAndCert = new byte[provisioningDataArray.Length + tempCert.Length];

            Array.Copy(provisioningDataArray, provisioningDataAndCert, provisioningDataArray.Length);
            Array.Copy(tempCert, 0, provisioningDataAndCert, provisioningDataArray.Length, tempCert.Length);

            return provisioningDataAndCert;
        }

    }
}
