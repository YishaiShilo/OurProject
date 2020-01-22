﻿/**
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


namespace DALSamplesServer
{
    class EPIDSigningHandler
    {
        private const int ERROR = -1;
        private const int VERIFYNG_SUCCESS = 0;
        private const int VERIFYNG_FAILED = 1;
        private bool clientConnected;

        public void handleClientComm(object client)
        {
            try
            {
                TcpClient tcpClient = (TcpClient)client;
                Socket socket = tcpClient.Client;
                clientConnected = socket.Connected;
                while (clientConnected)
                {

                    byte[] dataSize = new byte[4];
                    int received, size, total, dataLeft;

                    //receive adapted message(message prepared for verification)
                    received = socket.Receive(dataSize, 0, 4, 0);
                    size = BitConverter.ToInt32(dataSize, 0);
                    total = 0;
                    dataLeft = size;
                    byte[] adaptedMessage = new byte[size];

                    while (total < size)
                    {
                        received = socket.Receive(adaptedMessage, total, dataLeft, 0);
                        if (received == 0)
                        {
                            break;
                        }
                        total += received;
                        dataLeft -= received;
                    }


                    //receive signature
                    received = socket.Receive(dataSize, 0, 4, 0);
                    size = BitConverter.ToInt32(dataSize, 0);
                    total = 0;
                    dataLeft = size;
                    byte[] signature = new byte[size];

                    while (total < size)
                    {
                        received = socket.Receive(signature, total, dataLeft, 0);
                        if (received == 0)
                        {
                            break;
                        }
                        total += received;
                        dataLeft -= received;
                    }


                    //receive EPID groupID
                    byte[] groupIDByteArray = new byte[4];
                    socket.Receive(groupIDByteArray, 0, 4, 0);
                    int groupID = Utils.ByteArrayToInt(groupIDByteArray);


                    //groupCert contains the SIGMA1_0 certificate for the specific EPID group ID
                    byte[] groupCert = Utils.GetSpecificEpidCertificate_SIGMA_1_0((uint)groupID);
                    //epidParamsCert contains the mathematic parameters
                    byte[] epidParamsCert = File.ReadAllBytes(DataStructs.DEBUG_SIGNED_BIN_PARAMS_CERT_FILE);

                    // taskInfoArray is a data structure defined in the DAL implementation. It is prepended to the message by DAL prior to signing,
                    // and so has to be prepended by us prior to verification
                    byte[] taskInfoArray = Utils.StructureToByteArray(DataStructs.GetTaskInfo());
                    byte[] infoNonceMessage = new byte[taskInfoArray.Length + adaptedMessage.Length]; //TaskInfo || Nonce || Message


                    taskInfoArray.CopyTo(infoNonceMessage, 0);//concatenate the info to infoNonceMessage
                    adaptedMessage.CopyTo(infoNonceMessage, taskInfoArray.Length);//concatenate the adapted message(including the nonce) to infoNonceMessage

                    //Verify the signature
                    //When we call the MessageVerifyPch function, we can send
                    //baseName - the basename that will be signed as part of the signature
                    //privateKeyRevList - list of the platforms that were revoked based on the platform’s private key
                    //SignatureRevList - list of the platforms that were revoked based on the signature generated by the platform
                    //GroupRevList - list of the EPID groups that were revoked based on the group public key
                    //as parameters.
                    CdgResult retStatus;
                    CdgStatus status = CryptoDataGenWrapper_1_1.MessageVerifyPch(groupCert, groupCert.Length, epidParamsCert, infoNonceMessage, infoNonceMessage.Length, null, 0, signature, signature.Length, out retStatus, null, null, null);

                    int res;
                    if (status != CdgStatus.CdgStsOk)
                        res = ERROR;
                    else
                        if (retStatus != CdgResult.CdgValid)
                            res = VERIFYNG_FAILED;
                        else
                            res = VERIFYNG_SUCCESS;

                    byte[] result = new byte[4];
                    result = BitConverter.GetBytes(res);
                    //Send the signature verification result to the client
                    socket.Send(result);
                }
                Console.WriteLine("EPID Signing Sample Client disconnected.\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

    }
}












