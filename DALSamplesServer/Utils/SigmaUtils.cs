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
using System.Security.Cryptography.X509Certificates;

namespace DALSamplesServer.Utils
{
    class SigmaUtils
    {
        private const string OCSP_CER_FILE = "ocspFile.cer";
        private const string OCSP_CER_FROM_CHAIN_FILE = "certFromOcspChain.cer";
        private const string OCSP_SERVER_NAME = "http://upgrades.intel.com/ocsp";

        /**
         * Returns a byte array containing all EPID certificates for SIGMA 1.0
         */
        public static byte[] GetAllEpidCerts_SIGMA_1_0()
        {
            // Concatenate all the SIGMA 1.0 certificates to certsList
            List<byte> certsList = new List<byte>();
            for (int i = 0; i < EPIDDataStructs.AllEpid1_0Certs.Length; i++)
            {
                byte[] currentCert = File.ReadAllBytes(EPIDDataStructs.AllEpid1_0Certs[i]);
                for (int j = 0; j < currentCert.Length; j++)
                {
                    certsList.Add(currentCert[j]);
                }
            }
            return certsList.ToArray();
        }

        /**
         * Returns a byte array containing all EPID certificates for SIGMA 1.1
         */
        public static byte[] GetAllEpidCerts_SIGMA_1_1()
        {
            // Concatenate all the SIGMA 1.1 certificates to certsList
            List<byte> certsList = new List<byte>();
            for (int i = 0; i < EPIDDataStructs.AllEpid1_1Certs.Length; i++)
            {
                byte[] currentCert = File.ReadAllBytes(EPIDDataStructs.AllEpid1_1Certs[i]);
                for (int j = 0; j < currentCert.Length; j++)
                {
                    certsList.Add(currentCert[j]);
                }
            }
            return certsList.ToArray();
        }

        /**
         * Returns a byte array containing the SIGMA 1.0 certificate for the specified group ID 
         */
        public static byte[] GetSpecificEpidCertificate_SIGMA_1_0(uint groupID)
        {
            byte[] groupIDByteArray = BitConverter.GetBytes(groupID);
            byte[] certificatesBytes;

            // Get all EPID certificates' bytes for SIGMA 1.0
            certificatesBytes = GetAllEpidCerts_SIGMA_1_0();
            // Certificates array
            byte[][] certificates = new byte[certificatesBytes.Length / EPIDDataStructs.EPID_CERT_LEN][];
            // Reverse the group ID
            byte[] reversedGroupID = new byte[groupIDByteArray.Length];
            for (int i = 0; i < groupIDByteArray.Length; i++)
            {
                reversedGroupID[i] = groupIDByteArray[groupIDByteArray.Length - i - 1];
            }
            // Search for the certificate that fits the current group ID   
            for (int i = 0; i < certificates.Length; i++)
            {
                certificates[i] = new byte[EPIDDataStructs.EPID_CERT_LEN];
                Array.Copy(certificatesBytes, i * EPIDDataStructs.EPID_CERT_LEN, certificates[i], 0, EPIDDataStructs.EPID_CERT_LEN);
                if (GeneralUtils.CompareArray(reversedGroupID, 0, certificates[i], EPIDDataStructs.EPID_GID_OFFSET_IN_CERT, EPIDDataStructs.EPID_GID_LEN))
                    return certificates[i];
            }
            return null;
        }

        /**
         * Returns a byte array containing the SIGMA 1_1 certificate for the specified group ID 
         */
        public static byte[] GetSpecificEpidCertificate_SIGMA_1_1(uint groupID)
        {
            int certGroupID = 0;
            // Get all EPID certificates for SIGMA 1_1
            byte[] certificatesBytes = GetAllEpidCerts_SIGMA_1_1();
            int cumulativeCertLen = 0;
            int currCertLen = 0;

            // Search for the certificate that fits the current group ID   
            while (cumulativeCertLen < certificatesBytes.Length)
            {
                // Calculate current cert length
                currCertLen = int.Parse((certificatesBytes[cumulativeCertLen + 2] * 100).ToString(), System.Globalization.NumberStyles.HexNumber) + (certificatesBytes[cumulativeCertLen + 3]) + 4;

                // Copy current cert
                byte[] currCertBytes = new byte[currCertLen];
                Array.Copy(certificatesBytes, cumulativeCertLen, currCertBytes, 0, currCertLen);
                cumulativeCertLen += currCertLen;

                X509Certificate cert = new X509Certificate(currCertBytes);

                // Search the group ID in the certificate
                string subject = cert.Subject.Remove(0, cert.Subject.IndexOf("OU"));
                string[] editedSubject = subject.Remove(subject.IndexOf(',')).Split(' ');
                subject = editedSubject[editedSubject.Length - 1];
                certGroupID = int.Parse(subject);

                // Check whether certificate group ID equals the specified group ID 
                if (groupID == certGroupID)
                    return currCertBytes;
            }
            return null;
        }

        public static DataStructs.VlrHeader GenerateVLR(DataStructs.VlrIdType vlrType, int dataLen)
        {
            DataStructs.VlrHeader vlr;
            vlr.ID = (byte)vlrType;
            vlr.PaddedBytes = (byte)((4 - (DataStructs.VLR_HEADER_LEN + dataLen) % 4) % 4);
            vlr.VLRLength = (short)(DataStructs.VLR_HEADER_LEN + dataLen + vlr.PaddedBytes);
            return vlr;
        }

        /**
         * Checks if BK exists in the signed message
         */
        public static byte[] GetBKValuesFromSignedMessage(byte[] message)
        {
            if (message.Length < SigmaDataStructs.SIGRL_BK_SIZE + 4)
                return null;

            byte[] BKValue = new byte[SigmaDataStructs.SIGRL_BK_SIZE];

            Buffer.BlockCopy(message, 4, BKValue, 0, SigmaDataStructs.SIGRL_BK_SIZE);

            return BKValue;
        }

        /**
         * Checks whther BK exists in the signed message, as a part of the S3 message validation
         */
        public static bool DoesBKExist(SigmaDataStructs.SigmaS3Message S3Message, ref byte[] GaGbSig)
        {
            // Process certificate header in order to get cert length
            byte[] header = new byte[DataStructs.VLR_HEADER_LEN];
            Array.Copy(S3Message.data, header, header.Length);
            object certHeader = new DataStructs.VlrHeader();
            GeneralUtils.ByteArrayToStructure(header, ref certHeader);
            int certLen = ((DataStructs.VlrHeader)certHeader).VLRLength;

            // Extract GaGb from the signed message data
            Array.Copy(S3Message.data, certLen + DataStructs.VLR_HEADER_LEN, GaGbSig, 0, GaGbSig.Length);
            byte[] BK = GetBKValuesFromSignedMessage(GaGbSig);
            return BK != null;
        }

        private static List<byte[]> SplitCertificateChain(byte[] origCert)
        {
            List<byte[]> resCert = new List<byte[]>();

            int cumulativeCertLen = 0;
            int currCertLen = 0;

            for (int i = 0; cumulativeCertLen < origCert.Length; i += cumulativeCertLen)
            {
                // Parse the current certificate length
                currCertLen = int.Parse((origCert[cumulativeCertLen + 2] * 100).ToString(), System.Globalization.NumberStyles.HexNumber) + (origCert[cumulativeCertLen + 3]) + DataStructs.INT_SIZE;
                byte[] currCertBytes = new byte[currCertLen];
                // Copy the current certificate from the original certificate
                Array.Copy(origCert, cumulativeCertLen, currCertBytes, 0, currCertLen);
                // Add the current certificate to the whole certificates list
                resCert.Add(currCertBytes);
                cumulativeCertLen += currCertLen;
            }
            return resCert;
        }

        /**
         * Gets the real OCSP response according to the OCSP request we've got from the prover
         */
        public static byte[] GetOCSPResponseFromRealResponder(byte[] origCert, byte[] ocspNonce)
        {
            List<byte> combinedOcspResponse = new List<byte>();
            List<byte[]> certChain = SplitCertificateChain(origCert);
            foreach (byte[] cert in certChain)
            {
                File.WriteAllBytes(OCSP_CER_FROM_CHAIN_FILE, cert);
                // Create an OCSP request
                SigmaDataStructs.OCSPRequestInfo ocspRequest = new SigmaDataStructs.OCSPRequestInfo();
                // Fill the request fields
                ocspRequest.certName = OCSP_CER_FROM_CHAIN_FILE;
                ocspRequest.urlOcspResponder = OCSP_SERVER_NAME;
                ocspRequest.issuerName = EPIDDataStructs.EPID_ROOT_SIGNING_FILE;
                ocspRequest.ocspResponderCertName = SigmaDataStructs.SIGNED_OCSP_CERT_FILE;
                ocspRequest.ocspNonce = ocspNonce;
				// Set a proxy when running within Intel network. On external networks, use an empty string
                ocspRequest.proxyHostName = "proxy-us.intel.com:911";
                // Get the compatible OCSP response
                uint status = OCSPWrapper.GetOCSPResponse(ref ocspRequest, OCSP_CER_FILE);

                if (status != 0)
                    return null;
                // Concatenate the current response to the list
                combinedOcspResponse.AddRange(File.ReadAllBytes(OCSP_CER_FILE));
            }
            // Return the whole response
            return combinedOcspResponse.ToArray();
        }

        public static byte[] BuildOCSPResp(byte[] cert, byte[] ocspResp)
        {
            // Generate the VLR structure
            DataStructs.VlrHeader vlrHeader = GenerateVLR(DataStructs.VlrIdType.OCSP_RESP_VLR_ID, ocspResp.Length);
            byte[] ret = new byte[vlrHeader.VLRLength];

            GeneralUtils.StructureToByteArray(vlrHeader).CopyTo(ret, 0);
            ocspResp.CopyTo(ret, DataStructs.VLR_HEADER_LEN);

            return ret;
        }

        public static byte[] GenerateX509VerifierCert(byte[] cert)
        {
            SigmaDataStructs.X509VerifierCert x509VerifierCert = new SigmaDataStructs.X509VerifierCert();
            x509VerifierCert.vlrHeader = new DataStructs.VlrHeader();
            x509VerifierCert.CertData = cert;
            x509VerifierCert.vlrHeader = SigmaUtils.GenerateVLR(DataStructs.VlrIdType.VERIFIER_CERTIFICATE_CHAIN_VLR_ID, cert.Length);
            byte[] x509VerifierCertArray = new byte[x509VerifierCert.vlrHeader.VLRLength];
            GeneralUtils.StructureToByteArray(x509VerifierCert.vlrHeader).CopyTo(x509VerifierCertArray, 0);
            x509VerifierCert.CertData.CopyTo(x509VerifierCertArray, DataStructs.VLR_HEADER_LEN);
            return x509VerifierCertArray;
        }
    }
}
