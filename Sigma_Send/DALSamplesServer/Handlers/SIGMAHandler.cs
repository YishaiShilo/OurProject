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
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using DALSamplesServer.Utils;
using System.Drawing;

namespace DALSamplesServer.Handlers
{
    class SIGMAHandler : SampleHandler
    {
        private bool isClientConnected;
        // Status codes 
        private const int STATUS_SUCCEEDED = 0;
        private const int STATUS_FAILED = -1;

        #region SIGMA parameters

        private uint epidGroupID;
        private SigmaDataStructs.OcspRequest OCSPReq; // An (optional) OCSP Request from the prover (the Intel FW)
        private byte[] SMK = new byte[SigmaDataStructs.SIGMA_SMK_LENGTH]; // Session Message Key
        private byte[] Ga = new byte[SigmaDataStructs.SIGMA_PUBLIC_KEY_LEN]; // The prover's ephemeral DH public key
        private byte[] Gb = new byte[SigmaDataStructs.SIGMA_PUBLIC_KEY_LEN]; // The verfier's ephemeral DH public key.
        private byte[] _gagb = new byte[SigmaDataStructs.SIGMA_PUBLIC_KEY_LEN * 2];
        private byte[] GaGb
        {
            get
            {
                Ga.CopyTo(_gagb, 0);
                Gb.CopyTo(_gagb, SigmaDataStructs.SIGMA_PUBLIC_KEY_LEN);
                return _gagb;
            }
            set { value = _gagb; }
        }

        #endregion

        #region SIGMA Protocol

        private CdgStatus ProcessS1Message(byte[] s1Msg)
        {
            // Convert S1 message from byte array into the compatible structure
            object s1Message = new SigmaDataStructs.SigmaS1Message();
            GeneralUtils.ByteArrayToStructure(s1Msg, ref s1Message);

            // Extract S1 message
            SigmaDataStructs.SigmaS1Message message = (SigmaDataStructs.SigmaS1Message)s1Message;
            Ga = message.Ga; // Prover's ephemeral DH public key
            OCSPReq = message.OcspReq; // An (optional) OCSP Request from the prover
            epidGroupID = message.Gid; // Platform EPID group ID

            // Derive SK (Session Confidentiality Key: 128 bit derived from SMK), MK(Session Integrity Key: 128bit derived from SMK) and SMK(Session Message Key)
            byte[] Sk = new byte[SigmaDataStructs.SIGMA_SESSION_KEY_LEN];
            byte[] Mk = new byte[SigmaDataStructs.SIGMA_MAC_KEY_LEN];
            CdgStatus status = CryptoDataGenWrapper.DeriveSigmaKeys(Ga, Ga.Length, Gb, Gb.Length, Sk, Sk.Length, Mk, Mk.Length, SMK, SMK.Length);
            return status;
        }

        private SigmaDataStructs.SigmaS2Message GenerateS2Structure()
        {
            SigmaDataStructs.SigmaS2Message S2Message = new SigmaDataStructs.SigmaS2Message();
            // Fill the S2 fields
            S2Message.Basename = new byte[SigmaDataStructs.BASE_NAME_LENGTH]; // The base name, chosen by the verifier, to be used in name based signatures. here you can use baseName, in case you want to do it.
            S2Message.Gb = Gb; // The Verifier's ephemeral DH public key
            S2Message.OcspReq = OCSPReq; // An (optional) OCSP Request from the prover
            return S2Message;
        }

        private bool GetS2Message(out byte[] S2MessageArray)
        {
            S2MessageArray = new byte[0];
            byte[] cert = SigmaDataStructs.Sigma11_3PSignedCert; // Verifier's certificate
            byte[] key = SigmaDataStructs.Sigma11_Signed3PKey; // Verifier's private key

            // Get the OCSP (Online Certificate Status Protocol) response according to the request we've got from the prover
            byte[] OCSPResp = SigmaUtils.GetOCSPResponseFromRealResponder(cert, OCSPReq.OcspNonce);

            // If there is a problem with the OCSP server connection
            if (OCSPResp == null)
            {
                return false;
            }

            try
            {
                // Create S2 structure
                SigmaDataStructs.SigmaS2Message S2Message = GenerateS2Structure();

                // Build the OCSP response
                OCSPResp = SigmaUtils.BuildOCSPResp(cert, OCSPResp);

                // Composing - Data contains Verifier Cert, SIG_RL_HEADER followed by Revocation List , OCSP Response (If requested) VLRs in this order. SigRL is optional.
                List<byte> VerifierCertAndOCSPRespList = new List<byte>();

                // Generate X509 verifier certificate
                byte[] x509VerifierCertArray = SigmaUtils.GenerateX509VerifierCert(cert);

                // Add the x509VerifierCert to S2 message
                VerifierCertAndOCSPRespList.AddRange(x509VerifierCertArray);

                // Here you can generate SIG-RL in case you want to use it, and add it to s2List

                // Add the OCSP response to S2 message
                VerifierCertAndOCSPRespList.AddRange(OCSPResp);

                // Convert VerifierCert and OCSPResp list to a byte array
                byte[] VerifierCertAndOCSPResp = VerifierCertAndOCSPRespList.ToArray();

                // Convert OCSP request from structure to a byte array
                byte[] ocspReqArray = GeneralUtils.StructureToByteArray(S2Message.OcspReq);

                // Preparing the HMAC

                // Constructing HMAC data - Gb || Basename || OCSP Req || Verifier Cert ||  Sig-RL List || OCSPResp
                byte[] dataForHmac = new byte[SigmaDataStructs.SIGMA_PUBLIC_KEY_LEN + SigmaDataStructs.BASE_NAME_LENGTH + DataStructs.INT_SIZE + SigmaDataStructs.OCSP_NONCE_LENGTH + VerifierCertAndOCSPResp.Length];
                S2Message.Gb.CopyTo(dataForHmac, 0);
                S2Message.Basename.CopyTo(dataForHmac, SigmaDataStructs.SIGMA_PUBLIC_KEY_LEN);
                ocspReqArray.CopyTo(dataForHmac, SigmaDataStructs.SIGMA_PUBLIC_KEY_LEN + SigmaDataStructs.BASE_NAME_LENGTH);
                VerifierCertAndOCSPResp.CopyTo(dataForHmac, SigmaDataStructs.SIGMA_PUBLIC_KEY_LEN + SigmaDataStructs.BASE_NAME_LENGTH + DataStructs.INT_SIZE + SigmaDataStructs.OCSP_NONCE_LENGTH);

                // Create HMAC - HMAC_SHA256 of [Gb || Basename || OCSP Req || Verifier Cert ||  Sig-RL List ], using SMK
                CdgStatus status;
                S2Message.S2Icv = new byte[SigmaDataStructs.SIGMA_MAC_LEN];
                status = CryptoDataGenWrapper.CreateHmac(dataForHmac, dataForHmac.Length, SMK, SigmaDataStructs.SIGMA_SMK_LENGTH, S2Message.S2Icv, SigmaDataStructs.SIGMA_HMAC_LENGTH);
                if (status != CdgStatus.CdgStsOk)
                    return false;

                // Create Signed [Ga || Gb] using verifier ECDSA private key 
                S2Message.SigGaGb = new byte[SigmaDataStructs.ECDSA_SIGNATURE_LEN];
                status = CryptoDataGenWrapper.MessageSign(key, key.Length, GaGb, SigmaDataStructs.SIGMA_KEY_LENGTH * 2, S2Message.SigGaGb, SigmaDataStructs.ECDSA_SIGNATURE_LEN);
                if (status != CdgStatus.CdgStsOk)
                    return false;

                // Prepare final S2 message array contains S2 message structure + verifier cert + OCSP response
                int s2MessageLen = Marshal.SizeOf(S2Message);
                S2MessageArray = new byte[s2MessageLen + VerifierCertAndOCSPResp.Length];
                GeneralUtils.StructureToByteArray(S2Message).CopyTo(S2MessageArray, 0);
                VerifierCertAndOCSPResp.CopyTo(S2MessageArray, s2MessageLen);
                return true;

            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool VerifyS3Message(byte[] s3Message)
        {
            // Convert S3 message from byte array into the compatible structure
            object S3MessageObj = new SigmaDataStructs.SigmaS3Message();
            GeneralUtils.ByteArrayToStructure(s3Message, ref S3MessageObj);
            SigmaDataStructs.SigmaS3Message S3Message = (SigmaDataStructs.SigmaS3Message)S3MessageObj;

            // Locate the data index in the message
            int dataInd = Marshal.SizeOf(typeof(SigmaDataStructs.SigmaS3Message)) - 1 + 32;

            // Copy S3 message data from the received message into the S3 strucure data field
            S3Message.data = new byte[s3Message.Length - dataInd];
            Array.Copy(s3Message, dataInd, S3Message.data, 0, S3Message.data.Length);

            // Prepare data for HMAC
            byte[] dataForHmac = new byte[s3Message.Length - S3Message.S3Icv.Length];
            Array.Copy(s3Message, S3Message.S3Icv.Length, dataForHmac, 0, dataForHmac.Length);

            // Verify HMAC
            CdgResult retStat = CdgResult.CdgValid;
            CdgStatus status;
            status = CryptoDataGenWrapper.VerifyHmac(dataForHmac, dataForHmac.Length, S3Message.S3Icv, SigmaDataStructs.SIGMA_MAC_LEN, SMK, SigmaDataStructs.SIGMA_SMK_LENGTH, ref retStat);
            if (status != CdgStatus.CdgStsOk || retStat != CdgResult.CdgValid)
            {
                return false;
            }

            // Check whether BK exists in the signed message, as a part of the S3 message validation
            byte[] GaGbSig = new byte[SigmaDataStructs.EPID_SIG_LEN];
            if (!SigmaUtils.DoesBKExist(S3Message, ref GaGbSig))
                return false;

            // groupCert contains the SIGMA 1.0 certificate for the specific EPID group ID
            byte[] groupCert = SigmaUtils.GetSpecificEpidCertificate_SIGMA_1_0(epidGroupID);
            // epidParamsCert contains the mathematic parameters
            byte[] epidParamsData = File.ReadAllBytes(EPIDDataStructs.PRODUCTION_SIGNED_BIN_PARAMS_CERT_FILE);

            // Verify message. If a revocation list is used - the dll function will also check that the platform was not revoked.
            status = CryptoDataGenWrapper.MessageVerifyPch(groupCert, groupCert.Length, epidParamsData, GaGb, GaGb.Length, null, 0, GaGbSig, GaGbSig.Length, out retStat, null);

            if (status != CdgStatus.CdgStsOk || retStat != CdgResult.CdgValid)
            {
                return false;
            }

            return true;
        }

        #endregion

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
                    byte[] s1Msg = socket.ReceiveMessage(SigmaDataStructs.SIGMA_S1_MSG_LEN);

                    // Send S1 message processing status to client
                    if (ProcessS1Message(s1Msg) != CdgStatus.CdgStsOk)
                    {
                        socket.SendInt(STATUS_FAILED);
                        continue;
                    }
                    socket.SendInt(STATUS_SUCCEEDED);

                    // Send S2 message status to client
                    byte[] s2Message;
                    if (!GetS2Message(out s2Message))
                    {
                        socket.SendInt(STATUS_FAILED);
                        continue;
                    }
                    socket.SendInt(STATUS_SUCCEEDED);

                    // Send the S2 message to the client
                    socket.SendMessage(s2Message);

                    // Receive S3 message length from client
                    int s3MessageLen = socket.ReceiveMessageAsInt();
                    // Receive S3 message from client
                    byte[] s3Msg = socket.ReceiveMessage(s3MessageLen);

                    // Send S3 verification status to client                            
                    if (!VerifyS3Message(s3Msg))
                    {
                        socket.SendInt(STATUS_FAILED);
                        continue;
                    }
                    socket.SendInt(STATUS_SUCCEEDED);

                    
                    sendImage(socket);




                }

                Console.WriteLine("Protected Output Sample Client disconnected.\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private int sendImage(Socket socket)
        {
            string path = "C:\\Project\\OurProject\\Sigma_Send\\DALSamplesServer\\Images\\ImageToEncrypt.bmp";
            byte[] cmd = new byte[4];
            socket.Receive(cmd, 0, 4, 0);
            int msg = BitConverter.ToInt32(cmd, 0);
            if (msg != 100)
            {
                return -1;
            }
            flip24BitImage(path);
            byte[] bytesToSend = getFileBytes(path);
            flip24BitImage(path);
            int size = bytesToSend.Length;
            int sent = 0;
            int totalSent = 0;
            int dataLeft = size;
            byte[] dataSize = new byte[4];
            dataSize = BitConverter.GetBytes(size);
            sent = socket.Send(bytesToSend);
            while(totalSent < size)
            {
                sent = socket.Send(bytesToSend, totalSent, dataLeft, SocketFlags.None);
                totalSent += sent;
                dataLeft -= sent;
            }
            
            

            return 0;
        }
        private void flip24BitImage(string filename)
        {
            Image image = Bitmap.FromFile(filename);
            image.RotateFlip(RotateFlipType.RotateNoneFlipY);
            image.Save(filename, System.Drawing.Imaging.ImageFormat.Bmp);
        }

        public static byte[] getFileBytes(string filename)
        {
            const int buffersize = 1024;
            byte[] readBuff = new byte[buffersize];
            int count;
            int bytesWritten = 0;

            // read the file 
            FileStream stream = new FileStream(filename, FileMode.Open);
            BinaryReader reader = new BinaryReader(stream);
            MemoryStream mstream = new MemoryStream();

            do
            {
                count = reader.Read(readBuff, 0, buffersize);
                bytesWritten += count;
                mstream.Write(readBuff, 0, count);
            }
            while (count > 0);


            byte[] file_data = new byte[bytesWritten];

            mstream.Position = 0;
            mstream.Read(file_data, 0, bytesWritten);

            mstream.Close();
            reader.Close();
            stream.Close();

            return file_data;
        }
    }


    
}
