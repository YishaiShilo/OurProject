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
using System.Runtime.InteropServices;

namespace DALSamplesServer
{
    class EPIDDataStructs
    {
        #region Constants

        public const int EPID_CERT_LEN = 392;
        public const int EPID_GID_LEN = 4;
        public const short EPID_GID_OFFSET_IN_CERT = 4;
        public const int LEGACY_GROUP_CERT_1_1_FILE_LEN = 392;
        public const int PARAM_CERT_1_1_FILE_LEN = 876;

        #endregion

        #region EPID Certificates files

        /**
         * All certificates and mathematic parameters for all group IDs and for each Intel platform 
         * are stored locally in files that contain all group certificates 
         */

        // EPID public keys x509 format
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

        public static string PAVP_Group_GLKEPIDPROD_Public_Keys = "..\\..\\epid_data\\PAVP_Group_GLKEPIDPROD_Public_Keys.cer";                                  //GID 5496-5545
        public static string PAVP_Group_GLKEPIDTEST_Public_Keys = "..\\..\\epid_data\\PAVP_Group_GLKEPIDTEST_Public_Keys.cer";                                  //GID 5546-5595
        public static string PAVP_Group_LBGEPIDPROD_Public_Keys = "..\\..\\epid_data\\PAVP_Group_LBGEPIDPROD_Public_Keys.cer";                                  //GID 4469-4568
        public static string PAVP_Group_LBGEPIDTEST_Public_Keys = "..\\..\\epid_data\\PAVP_Group_LBGEPIDTEST_Public_Keys.cer";                                  //GID 4569
        public static string PAVP_Group_CNLEPIDA0TEST_EPID1_Public_Keys = "..\\..\\epid_data\\PAVP_Group_CNLEPIDA0TEST_EPID1_Public_Keys.cer";                  //GID 5601
        public static string PAVP_Group_CNLEPIDPROD_EPID1_Public_Keys = "..\\..\\epid_data\\PAVP_Group_CNLEPIDPROD_EPID1_Public_Keys.cer";                      //GID 5604-6003
        public static string PAVP_Group_ICLEPIDA0TEST_EPID1_Public_Keys = "..\\..\\epid_data\\PAVP_Group_ICLEPIDA0TEST_EPID1_Public_Keys.cer";
        public static string X509_Group_SVN1_BXT_KBL_EPID1_1_Public_Keys = "..\\..\\epid_data\\Group_10001_SVN_1_BROXTON_X509Certificate.cer";					//GID 10001
        public static string X509_Group_SVN2_BXT_KBL_EPID1_1_Public_Keys = "..\\..\\epid_data\\Group_10002_SVN_2_BROXTON_X509Certificate.cer";					//GID 10002
        public static string X509_Group_SVN3_BXT_KBL_EPID1_1_Public_Keys = "..\\..\\epid_data\\Group_10003_SVN_3_BROXTON_X509Certificate.cer";					//GID 10003
        public static string X509_Group_SVN4_BXT_KBL_EPID1_1_Public_Keys = "..\\..\\epid_data\\Group_10004_SVN_4_BROXTON_X509Certificate.cer";                  // GID 10004
        public static string Group_8058_EPID1_Public_Key = "..\\..\\epid_data\\Group_8058_EPID1_Public_Key.cer";												//GID 8058
        public static string PAVP_Group_SPTHEPIDPROD_EPID1_Public_Keys = "..\\..\\epid_data\\PAVP_Group_SPTHEPIDPROD_EPID1_Public_Keys.cer";                    //GID 8066 - 8465
        public static string PAVP_Group_CNLEPIDPOSTB1LPPROD2_EPID11_Public_Keys = "..\\..\\epid_data\\PAVP_Group_CNLEPIDPOSTB1LPPROD2_EPID11_Public_Keys.cer";  //GID 10415 - 10814

        public static string[] AllEpid1_1Certs = new string[]
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
        // EPID public keys legacy format 
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

        public static string BIN_SAFEID_CERT_VLV = "..\\..\\epid_data\\1_0\\PAVP_Group_VLV2EPIDPROD_Public_Keys.bin"; //GID 2167 to 2266
        public static string BIN_SAFEID_CERT_VLV2 = "..\\..\\epid_data\\1_0\\PAVP_Group_BYTEPIDPROD_Public_Keys_2668_2768.bin"; //GID 2668 to 2768
        public static string BIN_SAFEID_CERT_CHV2 = "..\\..\\epid_data\\1_0\\PAVP_Group_CHVEPIDPROD_Public_Keys.bin"; // GID 3341 to 3340 
        public static string BIN_SAFEID_CERT_WBG = "..\\..\\epid_data\\1_0\\PAVP_Group_WBGB1EPIDPROD_Public_Keys.bin";//GID 2772 to 2841
        public static string BIN_SAFEID_CERT_WPT = "..\\..\\epid_data\\1_0\\PAVP_Group_WPTLPEPIDProd_Public_Keys.bin";//GID 2268
        public static string BIN_SAFEID_CERT_WPT_test = "..\\..\\epid_data\\1_0\\PAVP_Group_WPTLPEPIDTest_Public_Keys.bin";//GID 2667
        public static string BIN_SAFEID_CERT_SPT_test = "..\\..\\epid_data\\1_0\\PAVP_Group_SPTHLPEPIDProd_Public_Keys.bin";//GID 3443
        public static string BIN_SAFEID_CERT_SPT_H_test = "..\\..\\epid_data\\1_0\\PAVP_Group_SPTHEPIDPROD_Public_Keys.bin";//GID 3969
        public static string BIN_SAFEID_CERT_BXT = "..\\..\\epid_data\\1_0\\PAVP_Group_BXTEPIDA0PROD_Public_Keys.bin";
        public static string BIN_SAFEID_CERT_GLKEPIDPROD = "..\\..\\epid_data\\1_0\\PAVP_Group_GLKEPIDPROD_EPID1_Public_Keys.bin";
        public static string BIN_SAFEID_CERT_GLKEPIDTEST = "..\\..\\epid_data\\1_0\\PAVP_Group_GLKEPIDTEST_EPID1_Public_Keys.bin";
        public static string BIN_SAFEID_CERT_LBGEPIDPROD = "..\\..\\epid_data\\1_0\\PAVP_Group_LBGEPIDPROD_EPID1_Public_Keys.bin";
        public static string BIN_SAFEID_CERT_LBGEPIDTEST = "..\\..\\epid_data\\1_0\\PAVP_Group_LBGEPIDTEST_EPID1_Public_Keys.bin";
        public static string BIN_SAFEID_CERT_CNLEPIDA0TEST_EPID1_Public_Keys = "..\\..\\epid_data\\1_0\\PAVP_Group_CNLEPIDA0TEST_EPID1_Public_Keys.bin";
        public static string BIN_SAFEID_CERT_CNLEPIDA0TEST_EPID2_Public_Keys = "..\\..\\epid_data\\1_0\\PAVP_Group_CNLEPIDPROD_EPID1_Public_Keys.bin";
        public static string BIN_SAFEID_CERT_ICLEPIDA0TEST_EPID1_Public_Keys = "..\\..\\epid_data\\1_0\\PAVP_Group_LBGEPIDTEST_EPID1_Public_Keys.bin";
        public static string BIN_SAFEID_CERT_BXT_KBL_SVN1_Public_Keys = "..\\..\\epid_data\\1_0\\Group_10001_SVN_1_BROXTON_LegacyCertificate.bin";				//GID 10001
        public static string BIN_SAFEID_CERT_BXT_KBL_SVN2_Public_Keys = "..\\..\\epid_data\\1_0\\Group_10002_SVN_2_BROXTON_LegacyCertificate.bin";				//GID 10002
        public static string BIN_SAFEID_CERT_BXT_KBL_SVN3_Public_Keys = "..\\..\\epid_data\\1_0\\Group_10003_SVN_3_BROXTON_LegacyCertificate.bin";				//GID 10003
        public static string BIN_SAFEID_CERT_BXT_KBL_SVN4_Public_Keys = "..\\..\\epid_data\\1_0\\Group_10004_SVN_4_BROXTON_LegacyCertificate.bin";				//GID 10004
        public static string BIN_SAFEID_CERT_CNL_8058_EPID1_Legacy_Public_Key = "..\\..\\epid_data\\1_0\\Group_8058_EPID1_Legacy_Public_Key.bin";				//GID 8058
        public static string PAVP_Group_SPTHLPEPIDProd_EPID1_Public_Keys = "..\\..\\epid_data\\1_0\\PAVP_Group_SPTHLPEPIDProd_EPID1_Public_Keys.bin";			//GID 8066 - 8465
        public static string PAVP_Group_CNLEPIDPOSTB1LPPROD2_EPID1_Public_Keys = "..\\..\\epid_data\\1_0\\PAVP_Group_CNLEPIDPOSTB1LPPROD2_EPID1_Public_Keys.bin";//GID 10415 - 10814

        public static string PRODUCTION_SIGNED_BIN_PARAMS_CERT_FILE = "..\\..\\epid_data\\signed_epid_paramcert.dat";
        public static string DEBUG_SIGNED_BIN_PARAMS_CERT_FILE = "..\\..\\epid_data\\EPIDGroupParams.cer";

        public static string[] AllEpid1_0Certs = new string[]
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

        #endregion

        /**
         *  Session Manager Command Status 
         */
        public enum SessionMgrCmdStatus
        {
            SessionMgrStatusSuccess = 0,    // Command completed successfully.
            SessionMgrStatusIncorrectApiVersion = 1, // Incorrect API version passed in the command header.
            SessionMgrStatusInvalidFunction = 2,	// Invalid function number specified in the command header.
            SessionMgrStatusInvalidParams = 3,  // Invalid command specific buffer length specified in the command header.
            SessionMgrStatusInvalidBuffLen = 4,	// Invalid parameters passed inside the command buffer.
            SessionMgrStatusInternalError = 5,	// Internal error occurred.
            SessionMgrStatusAlreadyProvisioned = 6
        }

        /**
         * Session Manager Command Header
         * Every blob begins with the following header.
         */
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SessionMgrCmdHdr
        {
            public uint ApiVersion;
            public uint CommandId;
            public SessionMgrCmdStatus Status;
            public uint BufferLength;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ProvisioningDataSigma1_1
        {
            public SessionMgrCmdHdr Header;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = PARAM_CERT_1_1_FILE_LEN)]
            public byte[] CryptoContext; // Mathematic parameters
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = LEGACY_GROUP_CERT_1_1_FILE_LEN)]
            public byte[] SafeIdCert; // SIGMA 1.0 certificates
            public X509GroupCertificate X509GroupCert; // EPID group certificate
        }

        public enum CommandId
        {
            CMD_CHECK_SAFEID_STATUS,
            CMD_SEND_SAFEID_PUBKEY
        }

        /**
         * EPID group certificate
         */
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct X509GroupCertificate
        {
            public DataStructs.VlrHeader vlrHeader;
        }

        public static DataStructs.IntelMETaskInfo GetTaskInfo()
        {
            DataStructs.IntelMETaskInfo ti = new DataStructs.IntelMETaskInfo();
            ti.Hdr.Type = DataStructs.TaskInfoType.MeTask;
            ti.Hdr.TaskInfoLen = (uint)Marshal.SizeOf(typeof(DataStructs.IntelMETaskInfo)) + (uint)(DataStructs.APPLET_ID_LEN * 2) - (uint)Marshal.SizeOf(typeof(DataStructs.SigmaTaskInfoHeader));
            ti.TaskId = 8; // DAL container
            ti.SubTaskId = 0; // unused
            return ti;
        }
    }
}
