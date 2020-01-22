/**
***
*** Copyright  (C) 2014-2017 Intel Corporation. All rights reserved.
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
using System.Runtime.InteropServices;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace DALSamplesServer
{
    /**
     * Contains all data structures needed for communication with EPID SIK and with the DAL Applet
     **/
    public class DataStructs
    {
        //constants
        public static int APPLET_ID_LEN = 16;


        //SIGMA constants
        static byte SIGM_API_VERSION_MAJOR_1_1 = 3;
        static byte SIGMA_API_VERSION_MINOR_1_1 = 0;
        public static int SIGMA_API_VERSION_1_1 = ((SIGM_API_VERSION_MAJOR_1_1 << 16) | (SIGMA_API_VERSION_MINOR_1_1));

        public const short SIGMA_KEY_LENGTH = 64;
        public const int ECDSA_SIGNATURE_LEN = 64;
        public const short SIGMA_HMAC_LENGTH = 32;
        public const int SIGMA_PUBLIC_KEY_LEN = 64;
        public const byte BASE_NAME_LENGTH = 32;
        public const byte OCSP_NONCE_LENGTH = 32;
        public const short SIGMA_SMK_LENGTH = 32;
        public const int SIGMA_MAC_LEN = 32;
        public const int SIGMA_SESSION_KEY_LEN = 16;
        public const int SIGMA_MAC_KEY_LEN = 16;
        public const int EPID_SIG_LEN = 569;
        public const byte SIGRL_BK_SIZE = 128;
        public const int INT_SIZE = 4;
        //EPID constants
        
        public const int EPID_CERT_LEN = 392;
        public const int EPID_GID_LEN = 4;
        public const short EPID_GID_OFFSET_IN_CERT = 4;        
        public static readonly byte VLR_HEADER_LEN = (byte)Marshal.SizeOf(typeof(VLRHeader));
        public const int LEGACY_GROUP_CERT_1_1_FILE_LEN = 392;
        public const int PARAM_CERT_1_1_FILE_LEN = 876;



        /**
         * All certificates and mathematic parameters for all group IDs are stored locally in files for each platform that contain all group certificates 
         */

        //EPID public keys new format (x509)
        public static string PAVP_Group_LPTEPIDH_Public_Keys = "..\\..\\epid_data\\PAVP_Group_LPTEPIDH_Public_Keys.cer"; //GID 1242-1643
        public static string PAVP_Group_LPTEPIDKP_Public_Keys = "..\\..\\epid_data\\epid_X509_LPTLP_GID_1690_2089.cer"; //GID 1690-2089
        public static string X509_GROUP_CERT_1_1_FILE = "..\\..\\epid_data\\EPIDGroupX509.cer"; //GID 0,831-1230
        public static string X509_GROUP_1242_LPT_LP = "..\\..\\epid_data\\Group_1242_Public_Key.cer"; //GID 1242
        public static string PAVP_Group_VLV2EPIDPROD_Public_Keys = "..\\..\\epid_data\\PAVP_Group_VLV2EPIDPROD_Public_Keys.cer"; //GID 2167 to 2266
        public static string X509_GROUPS_1668_1768_Public_Keys = "..\\..\\epid_data\\epid_VLV2_Public_Keys_GID_2668_2768.cer"; // GID 2668 to 2768
        public static string PAVP_Group_CHV2EPIDPROD_Public_Keys = "..\\..\\epid_data\\PAVP_Group_CHV2EPIDPROD_Public_Keys_3341_3440.cer"; //GID 3341 to 3440
        public static string PAVP_Group_WBGEPIDPROD_Public_Keys = "..\\..\\epid_data\\PAVP_Group_WBGB1EPIDPROD_Public_Keys.cer";//GID 2772 to 2841
        public static string PAVP_Group_WPTEPIDPROD_Public_Keys = "..\\..\\epid_data\\PAVP_Group_WPTLPEPIDProd_Public_Keys.cer";//GID 2268
        public static string PAVP_Group_WPTEPIDTEST_Public_Keys = "..\\..\\epid_data\\PAVP_Group_WPTLPEPIDtest_Public_Keys.cer";//GID 2268
        public static string PAVP_Group_SPTEPIDTEST_Public_Keys = "..\\..\\epid_data\\PAVP_Group_SPTEPIDPROD_Public_Keys_3443-3842.cer";//GID 3443-3842
        public static string PAVP_Group_SPTEPIDTEST_H_Public_Keys = "..\\..\\epid_data\\PAVP_Group_SPTEPIDPROD_Public_Keys_3969-4368.cer";//GID 3969-4368
        public static string PAVP_Group_BXTEPIDTEST_Public_Keys = "..\\..\\epid_data\\PAVP_Group_BXT_EPIDPROD_Public_Keys.cer";//GID 4571-4695

        public static string PAVP_Group_WPTLPEPIDTest_Public_1_1_Keys = "..\\..\\epid_data\\PAVP_Group_WPTLPEPIDTest_Public_Keys.cer"; // GID 2268 - ~2600

		public static string PAVP_Group_GLKEPIDPROD_Public_Keys = "..\\..\\epid_data\\PAVP_Group_GLKEPIDPROD_Public_Keys.cer";									//GID 5496-5545
		public static string PAVP_Group_GLKEPIDTEST_Public_Keys = "..\\..\\epid_data\\PAVP_Group_GLKEPIDTEST_Public_Keys.cer";									//GID 5546-5595
		public static string PAVP_Group_LBGEPIDPROD_Public_Keys = "..\\..\\epid_data\\PAVP_Group_LBGEPIDPROD_Public_Keys.cer";									//GID 4469-4568
		public static string PAVP_Group_LBGEPIDTEST_Public_Keys = "..\\..\\epid_data\\PAVP_Group_LBGEPIDTEST_Public_Keys.cer";									//GID 4569
		public static string PAVP_Group_CNLEPIDA0TEST_EPID1_Public_Keys = "..\\..\\epid_data\\PAVP_Group_CNLEPIDA0TEST_EPID1_Public_Keys.cer";					//GID 5601
		public static string PAVP_Group_CNLEPIDPROD_EPID1_Public_Keys = "..\\..\\epid_data\\PAVP_Group_CNLEPIDPROD_EPID1_Public_Keys.cer";						//GID 5604-6003
		public static string PAVP_Group_ICLEPIDA0TEST_EPID1_Public_Keys = "..\\..\\epid_data\\PAVP_Group_ICLEPIDA0TEST_EPID1_Public_Keys.cer";
		public static string X509_Group_SVN1_BXT_KBL_EPID1_1_Public_Keys = "..\\..\\epid_data\\Group_10001_SVN_1_BROXTON_X509Certificate.cer";					//GID 10001
        public static string X509_Group_SVN2_BXT_KBL_EPID1_1_Public_Keys = "..\\..\\epid_data\\Group_10002_SVN_2_BROXTON_X509Certificate.cer";					//GID 10002
        public static string X509_Group_SVN3_BXT_KBL_EPID1_1_Public_Keys = "..\\..\\epid_data\\Group_10003_SVN_3_BROXTON_X509Certificate.cer";					//GID 10003
        public static string X509_Group_SVN4_BXT_KBL_EPID1_1_Public_Keys = "..\\..\\epid_data\\Group_10004_SVN_4_BROXTON_X509Certificate.cer";					// GID 10004
		public static string Group_8058_EPID1_Public_Key = "..\\..\\epid_data\\Group_8058_EPID1_Public_Key.cer";												//GID 8058
        public static string PAVP_Group_SPTHEPIDPROD_EPID1_Public_Keys = "..\\..\\epid_data\\PAVP_Group_SPTHEPIDPROD_EPID1_Public_Keys.cer";					//GID 8066 - 8465
		public static string PAVP_Group_CNLEPIDPOSTB1LPPROD2_EPID11_Public_Keys = "..\\..\\epid_data\\PAVP_Group_CNLEPIDPOSTB1LPPROD2_EPID11_Public_Keys.cer";	//GID 10415 - 10814
       
        public static string[] ALL_EPID_CERTS_1_1 = new string[] 
        {
            PAVP_Group_CHV2EPIDPROD_Public_Keys, PAVP_Group_LPTEPIDKP_Public_Keys, X509_GROUP_1242_LPT_LP, PAVP_Group_LPTEPIDH_Public_Keys,
            X509_GROUP_CERT_1_1_FILE, PAVP_Group_VLV2EPIDPROD_Public_Keys, X509_GROUPS_1668_1768_Public_Keys, PAVP_Group_WBGEPIDPROD_Public_Keys,
            PAVP_Group_WPTEPIDPROD_Public_Keys, PAVP_Group_WPTEPIDTEST_Public_Keys, PAVP_Group_SPTEPIDTEST_Public_Keys, PAVP_Group_SPTEPIDTEST_H_Public_Keys,
            PAVP_Group_BXTEPIDTEST_Public_Keys, PAVP_Group_WPTLPEPIDTest_Public_1_1_Keys, PAVP_Group_GLKEPIDPROD_Public_Keys,
            PAVP_Group_GLKEPIDTEST_Public_Keys, PAVP_Group_LBGEPIDPROD_Public_Keys, PAVP_Group_LBGEPIDTEST_Public_Keys,
            PAVP_Group_CNLEPIDA0TEST_EPID1_Public_Keys, PAVP_Group_CNLEPIDPROD_EPID1_Public_Keys,
            PAVP_Group_ICLEPIDA0TEST_EPID1_Public_Keys,X509_Group_SVN1_BXT_KBL_EPID1_1_Public_Keys,
            X509_Group_SVN2_BXT_KBL_EPID1_1_Public_Keys,X509_Group_SVN3_BXT_KBL_EPID1_1_Public_Keys,
            X509_Group_SVN4_BXT_KBL_EPID1_1_Public_Keys, Group_8058_EPID1_Public_Key, PAVP_Group_SPTHEPIDPROD_EPID1_Public_Keys,
            PAVP_Group_CNLEPIDPOSTB1LPPROD2_EPID11_Public_Keys
        };
  
        // GID's are +-1.
        //EPID public keys per generation. legacy format 
        public static string BIN_SAFEID_CERT_IBEXPEAK = "..\\..\\epid_data\\1_0\\PAVP_Group_Ibexpeak_Public_Keys.bin"; //GID 0-400
        public static string BIN_SAFEID_CERT_Cougarpoint = "..\\..\\epid_data\\1_0\\PAVP_Group_Cougarpoint_DAA_Public_Keys.bin"; //GID 400-800
        public static string BIN_SAFEID_CERT_CPTRPPTEPID = "..\\..\\epid_data\\1_0\\PAVP_Group_CPTRPPTEPID_Public_Keys.bin"; //GID 800-1200 
        public static string BIN_SAFEID_CERT_Patsburg = "..\\..\\epid_data\\1_0\\PAVP_Group_Patsburg_DAA_Public_Keys.bin";
        public static string BIN_SAFEID_CERT_LPT_H = "..\\..\\epid_data\\1_0\\PAVP_Group_LPTEPIDH_Public_Keys.bin"; //1244-1644
        public static string BIN_SAFEID_CERT_LPT_H_TEST = "..\\..\\epid_data\\1_0\\PAVP_Group_TESTLPTHB0_Public_Keys.bin"; //GID 1242
        public static string BIN_SAFEID_CERT_LPT_LP = "..\\..\\epid_data\\1_0\\epid_lptlp_cert_db_1690-2089.bin"; //GID 1690-2089
        public static string BIN_SAFEID_CERT_LPT_LP_TEST = "..\\..\\epid_data\\1_0\\epid_lptlp_cert_db_1243.bin"; //GID 1243
        public static string PAVP_Group_WPTLPEPIDTest_Public_Keys = "..\\..\\epid_data\\1_0\\PAVP_Group_WPTLPEPIDTest_Public_Keys.bin"; // GID 2268 - ~2600
        public static string EPID_ROOT_SIGNING_FILE = "..\\..\\epid_data\\EPIDrootsiging_Public_Key.cer"; //epid root cert
        public static string SIGNED_OCSP_CERT_FILE = "..\\..\\epid_data\\signed_ocsp_signed.cer"; //cert of ocsp responder
        public static string SIGNED_CERT_1_1_FILE_NAME = "..\\..\\epid_data\\SignedX509Cert.cer";
        public static string VERIFIER_CERT_X509_FILE = "..\\..\\epid_data\\VerifierPubCert.cer";
        public static string SIGNED_KEY_1_1_FILE_NAME = "..\\..\\epid_data\\SignedX509CertPrivateKey.der";
        public static string VERIFIER_PRIVATE_KEY_X509_FILE = "..\\..\\epid_data\\VerifierPrivKey.dat";

        public static string BIN_SAFEID_CERT_VLV = "..\\..\\epid_data\\1_0\\PAVP_Group_VLV2EPIDPROD_Public_Keys.bin"; //GID 2167 to 2266
        public static string BIN_SAFEID_CERT_VLV2 = "..\\..\\epid_data\\1_0\\PAVP_Group_BYTEPIDPROD_Public_Keys_2668_2768.bin"; //GID 2668 to 2768
        public static string BIN_SAFEID_CERT_CHV2 = "..\\..\\epid_data\\1_0\\PAVP_Group_CHVEPIDPROD_Public_Keys.bin"; // GID 3341 to 3340 
        public static string BIN_SAFEID_CERT_WBG = "..\\..\\epid_data\\1_0\\PAVP_Group_WBGB1EPIDPROD_Public_Keys.bin";//GID 2772 to 2841
        public static string BIN_SAFEID_CERT_WPT = "..\\..\\epid_data\\1_0\\PAVP_Group_WPTLPEPIDProd_Public_Keys.bin";//GID 2268
        public static string BIN_SAFEID_CERT_WPT_test = "..\\..\\epid_data\\1_0\\PAVP_Group_WPTLPEPIDTest_Public_Keys.bin";//GID 2667
        public static string BIN_SAFEID_CERT_SPT_test = "..\\..\\epid_data\\1_0\\PAVP_Group_SPTHLPEPIDProd_Public_Keys.bin";//GID 3443
        public static string BIN_SAFEID_CERT_SPT_H_test = "..\\..\\epid_data\\1_0\\PAVP_Group_SPTHEPIDPROD_Public_Keys.bin";//GID 3969
        public static string BIN_SAFEID_CERT_BXT = "..\\..\\epid_data\\1_0\\PAVP_Group_BXTEPIDA0PROD_Public_Keys.bin"; //GID ?? 
		public static string BIN_SAFEID_CERT_GLKEPIDPROD = "..\\..\\epid_data\\1_0\\PAVP_Group_GLKEPIDPROD_EPID1_Public_Keys.bin";								//GID ??
        public static string BIN_SAFEID_CERT_GLKEPIDTEST = "..\\..\\epid_data\\1_0\\PAVP_Group_GLKEPIDTEST_EPID1_Public_Keys.bin";								//GID ??
        public static string BIN_SAFEID_CERT_LBGEPIDPROD = "..\\..\\epid_data\\1_0\\PAVP_Group_LBGEPIDPROD_EPID1_Public_Keys.bin";								//GID ??
        public static string BIN_SAFEID_CERT_LBGEPIDTEST = "..\\..\\epid_data\\1_0\\PAVP_Group_LBGEPIDTEST_EPID1_Public_Keys.bin";								//GID ??
        public static string BIN_SAFEID_CERT_CNLEPIDA0TEST_EPID1_Public_Keys = "..\\..\\epid_data\\1_0\\PAVP_Group_CNLEPIDA0TEST_EPID1_Public_Keys.bin";		//GID ??
        public static string BIN_SAFEID_CERT_CNLEPIDA0TEST_EPID2_Public_Keys = "..\\..\\epid_data\\1_0\\PAVP_Group_CNLEPIDPROD_EPID1_Public_Keys.bin";			//GID ??
        public static string BIN_SAFEID_CERT_ICLEPIDA0TEST_EPID1_Public_Keys = "..\\..\\epid_data\\1_0\\PAVP_Group_LBGEPIDTEST_EPID1_Public_Keys.bin";			//GID ??
        public static string BIN_SAFEID_CERT_BXT_KBL_SVN1_Public_Keys = "..\\..\\epid_data\\1_0\\Group_10001_SVN_1_BROXTON_LegacyCertificate.bin";				//GID 10001
        public static string BIN_SAFEID_CERT_BXT_KBL_SVN2_Public_Keys = "..\\..\\epid_data\\1_0\\Group_10002_SVN_2_BROXTON_LegacyCertificate.bin";				//GID 10002
        public static string BIN_SAFEID_CERT_BXT_KBL_SVN3_Public_Keys = "..\\..\\epid_data\\1_0\\Group_10003_SVN_3_BROXTON_LegacyCertificate.bin";				//GID 10003
        public static string BIN_SAFEID_CERT_BXT_KBL_SVN4_Public_Keys = "..\\..\\epid_data\\1_0\\Group_10004_SVN_4_BROXTON_LegacyCertificate.bin";				//GID 10004
        public static string BIN_SAFEID_CERT_CNL_8058_EPID1_Legacy_Public_Key = "..\\..\\epid_data\\1_0\\Group_8058_EPID1_Legacy_Public_Key.bin";				//GID 8058
        public static string PAVP_Group_SPTHLPEPIDProd_EPID1_Public_Keys = "..\\..\\epid_data\\1_0\\PAVP_Group_SPTHLPEPIDProd_EPID1_Public_Keys.bin";			//GID 8066 - 8465
        public static string PAVP_Group_CNLEPIDPOSTB1LPPROD2_EPID1_Public_Keys = "..\\..\\epid_data\\1_0\\PAVP_Group_CNLEPIDPOSTB1LPPROD2_EPID1_Public_Keys.bin";//GID 10415 - 10814
		
        // all legacy format EPID public keys.
        public static string PRODUCTION_SIGNED_BIN_PARAMS_CERT_FILE = "..\\..\\epid_data\\signed_epid_paramcert.dat";//"signed_epid_paramcert.dat";//"EPID_Params_Debug_Signed.dat";
        public static string[] ALL_EPID_CERTS_1_0 = new string[] 
        {
            BIN_SAFEID_CERT_IBEXPEAK, BIN_SAFEID_CERT_Cougarpoint, BIN_SAFEID_CERT_CPTRPPTEPID, BIN_SAFEID_CERT_Patsburg,
            BIN_SAFEID_CERT_LPT_H, BIN_SAFEID_CERT_LPT_H_TEST, BIN_SAFEID_CERT_LPT_LP, BIN_SAFEID_CERT_LPT_LP_TEST, PAVP_Group_WPTLPEPIDTest_Public_Keys,
            BIN_SAFEID_CERT_VLV, BIN_SAFEID_CERT_VLV2, BIN_SAFEID_CERT_CHV2, BIN_SAFEID_CERT_WBG, BIN_SAFEID_CERT_WPT, BIN_SAFEID_CERT_WPT_test,
            BIN_SAFEID_CERT_SPT_test, BIN_SAFEID_CERT_SPT_H_test, BIN_SAFEID_CERT_BXT, 
			BIN_SAFEID_CERT_GLKEPIDPROD, BIN_SAFEID_CERT_GLKEPIDTEST, BIN_SAFEID_CERT_LBGEPIDPROD, BIN_SAFEID_CERT_LBGEPIDTEST,
            BIN_SAFEID_CERT_CNLEPIDA0TEST_EPID1_Public_Keys, BIN_SAFEID_CERT_CNLEPIDA0TEST_EPID2_Public_Keys,
            BIN_SAFEID_CERT_ICLEPIDA0TEST_EPID1_Public_Keys,BIN_SAFEID_CERT_BXT_KBL_SVN1_Public_Keys,BIN_SAFEID_CERT_BXT_KBL_SVN2_Public_Keys,
            BIN_SAFEID_CERT_BXT_KBL_SVN3_Public_Keys, BIN_SAFEID_CERT_BXT_KBL_SVN4_Public_Keys,
            BIN_SAFEID_CERT_CNL_8058_EPID1_Legacy_Public_Key, PAVP_Group_SPTHLPEPIDProd_EPID1_Public_Keys,
            PAVP_Group_CNLEPIDPOSTB1LPPROD2_EPID1_Public_Keys
        };
        public static string DEBUG_SIGNED_BIN_PARAMS_CERT_FILE = "..\\..\\epid_data\\EPIDGroupParams.cer";


        //structs

        //Session Manager Command Status 
        public enum SESSMGR_STATUS
        {
            SESSMGR_STATUS_SUCCESS = 0,	//Command completed successfully.
            SESSMGR_STATUS_INCORRECT_API_VERSION = 1,//	Incorrect API version passed in the command header.
            SESSMGR_STATUS_INVALID_FUNCTION = 2,	//Invalid function number specified in the command header.
            SESSMGR_STATUS_INVALID_PARAMS = 3,//	Invalid command specific buffer length specified in the command header.
            SESSMGR_STATUS_INVALID_BUFFER_LENGTH = 4,	//Invalid parameters passed inside the command buffer.
            SESSMGR_STATUS_INTERNAL_ERROR = 5,	//Internal error occurred.
            SESSMGR_STATUS_ALREADY_PROVISIONED = 6
        }


        //Session Manager Command Header
        //Every BLOB begins with the following header.
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SESSMGR_CMD_HEADER
        {
            public uint ApiVersion;
            public uint CommandId;
            public SESSMGR_STATUS Status;
            public uint BufferLength;
        }


        //Provisioning data structure
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct PROVISIONING_DATA_SIGMA_1_1
        {
            public SESSMGR_CMD_HEADER Header;//header
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = DataStructs.PARAM_CERT_1_1_FILE_LEN)]
            public byte[] CryptoContext;//mathematic parameters
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = DataStructs.LEGACY_GROUP_CERT_1_1_FILE_LEN)]
            public byte[] SafeIdCert;//SIGMA_1_0 certificates
            public X509_GROUP_CERTIFICATE X509GroupCert;//EPID group certificate
        }


        

        //Command ID
        public enum COMMAND_ID
        {
            CMD_CHECK_SAFEID_STATUS,
            CMD_SEND_SAFEID_PUBKEY
        }


        //Visitor Location Register header
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct VLRHeader
        {
            public byte ID;
            public byte PaddedBytes;//Padding the cert so it will be dword aligned
            public short VLRLength;
        }


        //EPID group certificate
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct X509_GROUP_CERTIFICATE
        {
            public VLRHeader vlrHeader;
        }


        //// Task Info is a data structure defined in the DAL implementation. It is prepended to the message by DAL prior to signing,
        // and so has to be prepended by us prior to verification

        //Task Info Type
        public enum TASK_INFO_TYPE
        {
            ME_TASK = 0,
            SE_TASK,
            MAX_TASK
        }



        //SIGMA Task Info header
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SIGMA_TASK_INFO_HDR
        {
            public TASK_INFO_TYPE Type;
            public uint TaskInfoLen;
        }


        //Intel ME Task Info
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ME_TASK_INFO
        {
            public SIGMA_TASK_INFO_HDR Hdr;
            public uint TaskId;
            public uint SubTaskId;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] RsvdMECore;
        }


        //Visitor Location Register type
        public enum VLR_ID_TYPE : byte
        {
            X509_GROUP_CERTIFICATE_VLR_ID = 30,
            VERIFIER_CERTIFICATE_CHAIN_VLR_ID = 31,
            SIG_RL_VLR_ID = 32,
            OCSP_RESP_VLR_ID = 33,
            EPID_SIGNATURE_VLR_ID = 34,
            NRPROOFS_VLR_ID = 35
        }


        //OCSP request type
        public enum OCSP_REQ_TYPE
        {
            NoOcsp = 0,
            Cached = 1,
            NonCached = 2,
            MaxOcspType = 3
        }


        //OCSP request
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct OCSP_REQ
        {
            public OCSP_REQ_TYPE ReqType;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = OCSP_NONCE_LENGTH)]
            public byte[] OcspNonce;
        }




        //SIGMA S1 message
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SIGMA_S1_MESSAGE
        {
            // 64 byte Public Key generated by Intel ME for the session
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = SIGMA_KEY_LENGTH)]
            public byte[] Ga;

            // EPID Group the Intel ME belongs to
            public uint Gid;

            // Used to notify the verifier whether cached or non-cached OCSP verification is required
            public OCSP_REQ OcspReq;
        }


        //SIGMA S2 message
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SIGMA_S2_MESSAGE
        {
            // Signed [Ga || Gb] using verifier ECDSA public key 
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = ECDSA_SIGNATURE_LEN)]
            public byte[] SigGaGb;

            //HMAC_SHA256 of [Gb || Basename || OCSP Req || Verifier Cert ||  Sig-RL List ], using SMK
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = SIGMA_HMAC_LENGTH)]
            public byte[] S2Icv;

            //64 byte Public Key generated by verifier for the session
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = SIGMA_PUBLIC_KEY_LEN)]
            public byte[] Gb;

            // Basename is 32 bytes
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = BASE_NAME_LENGTH)]
            public byte[] Basename;

            // OCSP request sent in S1
            public OCSP_REQ OcspReq;
        }


        //SIGMA S3 message
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SIGMA_S3_MESSAGE
        {
            //! HMAC_SHA256 of [TaskInfo || ga || Epid Cert || Epid Signature || Sig-RL header || NrProofs ] using SMK
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = SIGMA_HMAC_LENGTH)]
            public byte[] S3Icv;

            //! Task Info contents for ME. Sigma users can populate RsvdforApp for application specific data.
            public ME_TASK_INFO TaskInfo;

            //! Session Key generated by ME and sent during S1
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = SIGMA_PUBLIC_KEY_LEN)]
            public byte[] Ga;

            //! Data contains EPID Cert + Epid Signature of [Ga || Gb] +  SIG_RL_HEADER  + NrProofs (if there are entries in Sig-RL. Each NR proof is 160 bytes) 
            [MarshalAs(UnmanagedType.ByValArray)]
            public byte[] data;
        }


        //OCSP request info
        public struct OCSPRequestInfo
        {
            public string urlOcspResponder;		// OCSP Responder URL
            public string certName;				// Verifier Certificate Name
            public string issuerName;			    // Verifier Issuer Certificate Name
            public string ocspResponderCertName;	// OCSP Responder Certificate Name to verify OCSP response
            public string proxyHostName;     // OCSP Proxy //proxy-us.intel.com:911
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = OCSP_NONCE_LENGTH)]
            public byte[] ocspNonce;//OCSP nonce
        }



        // X509 3P CERTIFICATE SIGMA1_1
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct X509VerifierCert
        {  
            public VLRHeader vlrHeader;
            [MarshalAs(UnmanagedType.ByValArray)]
            public byte[] CertData;
        }



        //SIGMA 1.1 signed certificates

        //Verifier's certificate
        public static byte[] Sigma11_3PSignedCert
        {
            get
            {
                return File.ReadAllBytes(SIGNED_CERT_1_1_FILE_NAME);
            }
        }

        //Verifier's private key
        public static byte[] Sigma11_Signed3PKey
        {
            get
            {
                return File.ReadAllBytes(SIGNED_KEY_1_1_FILE_NAME);
            }
        }


        public static byte[] Sigma11_3PCert
        {
            get
            {
                return File.ReadAllBytes(VERIFIER_CERT_X509_FILE);
            }
        }

        public static byte[] Cert3P
        {
            get
            {
                    return Sigma11_3PCert;
            }
        }


        //Get Task Info
        public static ME_TASK_INFO GetTaskInfo()
        {
            ME_TASK_INFO ti = new ME_TASK_INFO();
            ti.Hdr.Type = TASK_INFO_TYPE.ME_TASK;
            ti.Hdr.TaskInfoLen = (uint)Marshal.SizeOf(typeof(ME_TASK_INFO)) + (uint)(APPLET_ID_LEN * 2) - (uint)Marshal.SizeOf(typeof(SIGMA_TASK_INFO_HDR));
            ti.TaskId = 8;//(JOM container)
            ti.SubTaskId = 0; //(Unused by us)
            return ti;
        }
    }   
    }