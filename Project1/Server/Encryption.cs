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
using System.Drawing;

namespace DALSamplesServer
{
    class Encryption
    {
        private const int STATUS_SUCCEEDED = 0;
        private const int STATUS_FAILED = -1;

        private bool clientConnected;
        private const int S1_MSG_LEN = 104;

        private byte[] Ga = new byte[DataStructs.SIGMA_PUBLIC_KEY_LEN];//The prover's ephemeral DH public key
        private byte[] Gb = new byte[DataStructs.SIGMA_PUBLIC_KEY_LEN];//The verfier's ephemeral DH public key.
        private byte[] _gagb = new byte[DataStructs.SIGMA_PUBLIC_KEY_LEN * 2];//property

        private byte[] Sk = new byte[DataStructs.SIGMA_SESSION_KEY_LEN];//Session Confidentiality Key: 128 bit key derived from SMK
        private byte[] Mk = new byte[DataStructs.SIGMA_MAC_KEY_LEN];//Session Integrity Key: 128bit key derived from SMK.  
        private Aes aesIns = null;

        private DataStructs.OCSP_REQ OCSPReq;//An (optional) OCSP Request from the prover(the ME)
        private byte[] OCSPResp = null;//Online Certificate Status Protocol response
        private byte[] SMK = new byte[DataStructs.SIGMA_SMK_LENGTH];//Session Message Key
        private uint groupID;
        private byte[] BK;

        private const string OCSP_CER_FILE = "ocspFile.cer";
        private const string OCSP_CER_FROM_CHAIN_FILE = "certFromOcspChain.cer";
        private const string OCSP_SERVER_NAME = "http://upgrades.intel.com/ocsp";

        private Socket socket;

        //property
        private byte[] GaGb
        {
            get
            {
                Ga.CopyTo(_gagb, 0);
                Gb.CopyTo(_gagb, DataStructs.SIGMA_PUBLIC_KEY_LEN);
                return _gagb;
            }
            set { value = _gagb; }
        }



        //Process S1 message
        private CdgStatus ProcessS1Message(byte[] s1Msg)
        {
            //Convert S1 message from byte array into the compatible structure
            object s1Message = new DataStructs.SIGMA_S1_MESSAGE();
            Utils.ByteArrayToStructure(s1Msg, ref s1Message);

            //Extract S1 message
            DataStructs.SIGMA_S1_MESSAGE message = (DataStructs.SIGMA_S1_MESSAGE)s1Message;
            Ga = message.Ga;//Prover's ephemeral DH public key
            OCSPReq = message.OcspReq;//An (optional) OCSP Request from the prover
            groupID = message.Gid;//platform EPID group ID

            //Derive SK(Session Confidentiality Key: 128 bit), MK(Session Integrity Key: 128bit) and SMK(Session Message Key)
            CdgStatus status = CryptoDataGenWrapper_1_1.DeriveSigmaKeys(Ga, Ga.Length, Gb, Gb.Length, Sk, Sk.Length, Mk, Mk.Length, SMK, SMK.Length);
            return status;
        }

        //Split the certificate chain
        private static List<byte[]> SplitCertificateChain(byte[] origCert)
        {
            List<byte[]> resCert = new List<byte[]>();

            int cumulativeCertLen = 0;
            int currCertLen = 0;

            for (int i = 0; cumulativeCertLen < origCert.Length; i += cumulativeCertLen)
            {
                //Parse the current certificate length
                currCertLen = int.Parse((origCert[cumulativeCertLen + 2] * 100).ToString(), System.Globalization.NumberStyles.HexNumber) + (origCert[cumulativeCertLen + 3]) + DataStructs.INT_SIZE;
                byte[] currCertBytes = new byte[currCertLen];
                //Copy the current certificate from the original certificate
                Array.Copy(origCert, cumulativeCertLen, currCertBytes, 0, currCertLen);
                //Add the current certificate to the whole certificates list
                resCert.Add(currCertBytes);
                cumulativeCertLen += currCertLen;
            }
            return resCert;
        }


        //Get the real OCSP response according to the OCSP request we've got from the proover
        private static byte[] GetOCSPResponseFromRealResponder(byte[] origCert, byte[] ocspNonce)
        {
            List<byte> combinedOcspResponse = new List<byte>();
            List<byte[]> certChain = SplitCertificateChain(origCert);
            foreach (byte[] cert in certChain)
            {
                File.WriteAllBytes(OCSP_CER_FROM_CHAIN_FILE, cert);
                //Create an OCSP request
                DataStructs.OCSPRequestInfo ocspRequest = new DataStructs.OCSPRequestInfo();
                //Fill the request fields
                ocspRequest.certName = OCSP_CER_FROM_CHAIN_FILE;
                ocspRequest.urlOcspResponder = OCSP_SERVER_NAME;
                ocspRequest.issuerName = DataStructs.EPID_ROOT_SIGNING_FILE;
                ocspRequest.ocspResponderCertName = DataStructs.SIGNED_OCSP_CERT_FILE;
                ocspRequest.ocspNonce = ocspNonce;
                // proxy to operate inside intel network
                ocspRequest.proxyHostName = "";// "proxy-us.intel.com:911";//Resource.OcspProxy;
                //Get the compatible OCSP response
                uint status = OCSPWrapper.GetOCSPResponse(ref ocspRequest, OCSP_CER_FILE);

                if (status != 0)
                    return null;
                //Concatenate the current response to the list
                combinedOcspResponse.AddRange(File.ReadAllBytes(OCSP_CER_FILE));
            }
            //Return the whole response
            return combinedOcspResponse.ToArray();
        }


        //Create S2 structure
        DataStructs.SIGMA_S2_MESSAGE GenerateS2Structure()
        {
            DataStructs.SIGMA_S2_MESSAGE S2Message = new DataStructs.SIGMA_S2_MESSAGE();
            //Fill the S2 fields
            S2Message.Basename = new byte[DataStructs.BASE_NAME_LENGTH];//The base name, chosen by the verifier, to be used in name based signatures. here you can use baseName, in case you want to do it.
            S2Message.Gb = Gb;//The Verifier's ephemeral DH public key
            S2Message.OcspReq = OCSPReq;//An (optional) OCSP Request from the prover
            return S2Message;
        }

        //Build the OCSP response
        private byte[] BuildOCSPResp(byte[] cert, byte[] ocspResp)
        {
            //Generate the VLR structure
            DataStructs.VLRHeader vlrHeader = Utils.GenerateVLR(DataStructs.VLR_ID_TYPE.OCSP_RESP_VLR_ID, ocspResp.Length);
            byte[] ret = new byte[vlrHeader.VLRLength];

            Utils.StructureToByteArray(vlrHeader).CopyTo(ret, 0);
            ocspResp.CopyTo(ret, DataStructs.VLR_HEADER_LEN);

            return ret;

        }


        //Generate X509 verifier certificate
        private byte[] GenerateX509VerifierCert(byte[] cert)
        {
            DataStructs.X509VerifierCert x509VerifierCert = new DataStructs.X509VerifierCert();
            x509VerifierCert.vlrHeader = new DataStructs.VLRHeader();
            x509VerifierCert.CertData = cert;
            x509VerifierCert.vlrHeader = Utils.GenerateVLR(DataStructs.VLR_ID_TYPE.VERIFIER_CERTIFICATE_CHAIN_VLR_ID, cert.Length);
            byte[] x509VerifierCertArray = new byte[x509VerifierCert.vlrHeader.VLRLength];
            Utils.StructureToByteArray(x509VerifierCert.vlrHeader).CopyTo(x509VerifierCertArray, 0);
            x509VerifierCert.CertData.CopyTo(x509VerifierCertArray, DataStructs.VLR_HEADER_LEN);
            return x509VerifierCertArray;
        }

        //Get S2 message
        private bool GetS2Message(out byte[] S2MessageArray)
        {
            S2MessageArray = new byte[0];
            byte[] cert = DataStructs.Sigma11_3PSignedCert;//Verifier's certificate
            byte[] key = DataStructs.Sigma11_Signed3PKey;//Verifier's private key

            //Get the OCSP response according to the request we've got from the proover
            OCSPResp = GetOCSPResponseFromRealResponder(cert, OCSPReq.OcspNonce);

            //if there is a problem with the OCSP server connection
            if (OCSPResp == null)
            {
                return false;
            }

            try
            {
                //Create S2 structure
                DataStructs.SIGMA_S2_MESSAGE S2Message = GenerateS2Structure();

                //Build the OCSP response
                OCSPResp = BuildOCSPResp(cert, OCSPResp);

                //Composing - Data contains Verifier Cert, SIG_RL_HEADER followed by Revocation List , OCSP Response (If requested) VLRs in this order. SigRL is optional.
                List<byte> VerifierCertAndOCSPRespList = new List<byte>();

                //Generate X509 verifier certificate
                byte[] x509VerifierCertArray = GenerateX509VerifierCert(cert);

                //Add the x509VerifierCert to S2 message
                VerifierCertAndOCSPRespList.AddRange(x509VerifierCertArray);

                //Here you can generate SIG-RL in case you want to use it, and add it to s2List

                //Add the OCSP response to S2 message
                VerifierCertAndOCSPRespList.AddRange(OCSPResp);

                //Convert VerifierCert and OCSPResp list to a byte array
                byte[] VerifierCertAndOCSPResp = VerifierCertAndOCSPRespList.ToArray();

                //Convert OCSP request from structure to a byte array
                byte[] ocspReqArray = Utils.StructureToByteArray(S2Message.OcspReq);


                //Preparing the HMAC
                //Constructing HMAC data - Gb || Basename || OCSP Req || Verifier Cert ||  Sig-RL List || OCSPResp
                byte[] dataForHmac = new byte[DataStructs.SIGMA_PUBLIC_KEY_LEN + DataStructs.BASE_NAME_LENGTH + DataStructs.INT_SIZE + DataStructs.OCSP_NONCE_LENGTH + VerifierCertAndOCSPResp.Length];
                S2Message.Gb.CopyTo(dataForHmac, 0);
                S2Message.Basename.CopyTo(dataForHmac, DataStructs.SIGMA_PUBLIC_KEY_LEN);
                ocspReqArray.CopyTo(dataForHmac, DataStructs.SIGMA_PUBLIC_KEY_LEN + DataStructs.BASE_NAME_LENGTH);
                VerifierCertAndOCSPResp.CopyTo(dataForHmac, DataStructs.SIGMA_PUBLIC_KEY_LEN + DataStructs.BASE_NAME_LENGTH + DataStructs.INT_SIZE + DataStructs.OCSP_NONCE_LENGTH);


                //Create HMAC - HMAC_SHA256 of [Gb || Basename || OCSP Req || Verifier Cert ||  Sig-RL List ], using SMK
                CdgStatus status;
                S2Message.S2Icv = new byte[DataStructs.SIGMA_MAC_LEN];
                status = CryptoDataGenWrapper_1_1.CreateHmac(dataForHmac, dataForHmac.Length, SMK, DataStructs.SIGMA_SMK_LENGTH, S2Message.S2Icv, DataStructs.SIGMA_HMAC_LENGTH);
                if (status != CdgStatus.CdgStsOk)
                    return false;

                //Create Signed [Ga || Gb] using verifier ECDSA public key 
                S2Message.SigGaGb = new byte[DataStructs.ECDSA_SIGNATURE_LEN];
                status = CryptoDataGenWrapper_1_1.MessageSign(key, key.Length, GaGb, DataStructs.SIGMA_KEY_LENGTH * 2, S2Message.SigGaGb, DataStructs.ECDSA_SIGNATURE_LEN);
                if (status != CdgStatus.CdgStsOk)
                    return false;

                //Prepare final S2 message array contains S2 message structure + verifier cert + OCSP response
                int s2MessageLen = Marshal.SizeOf(S2Message);
                S2MessageArray = new byte[s2MessageLen + VerifierCertAndOCSPResp.Length];
                Utils.StructureToByteArray(S2Message).CopyTo(S2MessageArray, 0);
                VerifierCertAndOCSPResp.CopyTo(S2MessageArray, s2MessageLen);
                return true;

            }
            catch (Exception e)
            {
                return false;
            }
        }


        //Check whther BK exists in the signed message, as a p ert of the S3 message validation
        private bool DoesBKExist(DataStructs.SIGMA_S3_MESSAGE S3Message, ref byte[] GaGbSig)
        {
            //Process certificate header in order to get cert length
            byte[] header = new byte[DataStructs.VLR_HEADER_LEN];
            Array.Copy(S3Message.data, header, header.Length);
            object certHeader = new DataStructs.VLRHeader();
            Utils.ByteArrayToStructure(header, ref certHeader);
            int certLen = ((DataStructs.VLRHeader)certHeader).VLRLength;

            //Extract GaGb from the signed message data
            Array.Copy(S3Message.data, certLen + DataStructs.VLR_HEADER_LEN, GaGbSig, 0, GaGbSig.Length);
            BK = Utils.GetBKValuesFromSignedMessage(GaGbSig);
            return BK != null;
        }

        //Verify S3 message
        private bool VerifyS3Message(byte[] s3Message)
        {
            //Convert S3 message from byte array into the compatible structure
            object S3MessageObj = new DataStructs.SIGMA_S3_MESSAGE();
            Utils.ByteArrayToStructure(s3Message, ref S3MessageObj);
            DataStructs.SIGMA_S3_MESSAGE S3Message = (DataStructs.SIGMA_S3_MESSAGE)S3MessageObj;

            //Locate the data index in the message
            int dataInd = Marshal.SizeOf(typeof(DataStructs.SIGMA_S3_MESSAGE)) - 1 + 32;

            //Copy S3 message data from the received message into the S3 strucure data field
            S3Message.data = new byte[s3Message.Length - dataInd];
            Array.Copy(s3Message, dataInd, S3Message.data, 0, S3Message.data.Length);

            //Prepare data for HMAC
            byte[] dataForHmac = new byte[s3Message.Length - S3Message.S3Icv.Length];
            Array.Copy(s3Message, S3Message.S3Icv.Length, dataForHmac, 0, dataForHmac.Length);

            //Verify HMAC
            CdgResult retStat = CdgResult.CdgValid;
            CdgStatus status;
            status = CryptoDataGenWrapper_1_1.VerifyHmac(dataForHmac, dataForHmac.Length, S3Message.S3Icv, DataStructs.SIGMA_MAC_LEN, SMK, DataStructs.SIGMA_SMK_LENGTH, ref retStat);
            if (status != CdgStatus.CdgStsOk || retStat != CdgResult.CdgValid)
            {
                return false;
            }


            //Check whether BK exists in the signed message, as a part of the S3 message validation
            byte[] GaGbSig = new byte[DataStructs.EPID_SIG_LEN];
            if (!DoesBKExist(S3Message, ref GaGbSig))
                return false;


            //groupCert contains the SIGMA1_0 certificate for the specific EPID group ID
            byte[] groupCert = Utils.GetSpecificEpidCertificate_SIGMA_1_0((uint)groupID);
            //epidParamsCert contains the mathematic parameters
            byte[] epidParamsData = File.ReadAllBytes(DataStructs.PRODUCTION_SIGNED_BIN_PARAMS_CERT_FILE);


            //Verify message. In case that a revocation list is used - the dll function will also check that the platform was not revoked.
            status = CryptoDataGenWrapper_1_1.MessageVerifyPch(groupCert, groupCert.Length, epidParamsData, GaGb, GaGb.Length, null, 0, GaGbSig, GaGbSig.Length, out retStat, null);

            if (status != CdgStatus.CdgStsOk || retStat != CdgResult.CdgValid)
            {
                return false;
            }

            return true;
        }

        public byte[] EncryptBytes(byte[] plainInput)
        {
            using (var encryptor = aesIns.CreateEncryptor())
            {
                return PerformCryptography(plainInput, encryptor);
            }
        }

        private byte[] PerformCryptography(byte[] data, ICryptoTransform cryptoTransform)
        {
            using (var ms = new MemoryStream())
            using (var cryptoStream = new CryptoStream(ms, cryptoTransform, CryptoStreamMode.Write))
            {
                cryptoStream.Write(data, 0, data.Length);
                cryptoStream.FlushFinalBlock();

                return ms.ToArray();
            }
        }

        public byte[] DecryptBytes(byte[] encryptedInput)
        {
            using (var encryptor = aesIns.CreateDecryptor())
            {
                return PerformCryptography(encryptedInput, encryptor);
            }
        }

        public bool handleClientComm(object client)
        {
            try
            {
                TcpClient tcpClient = (TcpClient)client;
                socket = tcpClient.Client;
                clientConnected = socket.Connected;
                byte[] statusByte = new byte[DataStructs.INT_SIZE];
                while (clientConnected)
                {
                    //Receive S1 message from client
                    byte[] s1Msg = new byte[S1_MSG_LEN];
                    socket.Receive(s1Msg, 0, S1_MSG_LEN, 0);

                    //Process S1 message
                    CdgStatus status = ProcessS1Message(s1Msg);
                    //If S1 message processing succeeded
                    if (status == CdgStatus.CdgStsOk)
                    {
                        socket.Send(BitConverter.GetBytes(STATUS_SUCCEEDED));

                        //Get S2 Message
                        byte[] s2Message;

                        if (GetS2Message(out s2Message))
                        {
                            Console.WriteLine("generate s2 success");
                            socket.Send(BitConverter.GetBytes(STATUS_SUCCEEDED));

                            //Send the S2 message to the client
                            int total = 0;
                            int size = s2Message.Length;
                            int dataleft = size;
                            int sent;
                            byte[] datasize = new byte[DataStructs.INT_SIZE];
                            datasize = BitConverter.GetBytes(size);
                            sent = socket.Send(datasize);

                            while (total < size)
                            {
                                sent = socket.Send(s2Message, total, dataleft, SocketFlags.None);
                                total += sent;
                                dataleft -= sent;
                            }


                            //Receive S3 message length from client
                            byte[] s3MsgLen = new byte[DataStructs.INT_SIZE];
                            socket.Receive(s3MsgLen, 0, DataStructs.INT_SIZE, 0);
                            int s3MessageLen = Utils.ByteArrayToInt(s3MsgLen);

                            //Receive S3 message from client
                            byte[] s3Msg = new byte[s3MessageLen];
                            int bytesNeeded = s3MessageLen;
                            int bytesReceived = 0;
                            while (bytesNeeded > 0)
                            {
                                bytesReceived = socket.Receive(s3Msg, s3MessageLen - bytesNeeded, bytesNeeded, 0);
                                bytesNeeded -= bytesReceived;
                            }


                            //Send S3 verification status to client                            
                            if (VerifyS3Message(s3Msg))
                            {
                                socket.Send(BitConverter.GetBytes(STATUS_SUCCEEDED));
                                Console.WriteLine("SK: " + BitConverter.ToString(Sk));
                                Console.WriteLine("MK: " + BitConverter.ToString(Mk));

                                aesIns = AesManaged.Create();
                                aesIns.KeySize = 128;
                                aesIns.Key = Sk;
                                aesIns.BlockSize = 128;
                                aesIns.IV = new byte[16];
                                return true;
                            }
                            else
                            {
                                socket.Send(BitConverter.GetBytes(STATUS_FAILED));

                                return false;
                            }
                        }
                        else
                        {
                            socket.Send(BitConverter.GetBytes(STATUS_FAILED));
                            Console.WriteLine("fail generate s2");

                            return false;
                        }

                    }
                    else
                    {
                        socket.Send(BitConverter.GetBytes(STATUS_FAILED));
                        return false;
                    }
                }
                Console.WriteLine("Protected Output Sample Client disconnected.\n");
                return false;
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
    }
}
