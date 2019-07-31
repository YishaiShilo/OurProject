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
    public class DataStructs
    {
        // Constants
        public static int APPLET_ID_LEN = 16;
        public const int INT_SIZE = 4;
        public static readonly byte VLR_HEADER_LEN = (byte)Marshal.SizeOf(typeof(VlrHeader));

        /**
         * Visitor Location Register ID type
         */
        public enum VlrIdType : byte
        {
            X509_GROUP_CERTIFICATE_VLR_ID = 30,
            VERIFIER_CERTIFICATE_CHAIN_VLR_ID = 31,
            SIG_RL_VLR_ID = 32,
            OCSP_RESP_VLR_ID = 33,
            EPID_SIGNATURE_VLR_ID = 34,
            NRPROOFS_VLR_ID = 35
        }

        /**
         * Visitor Location Register header
         */
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct VlrHeader
        {
            public byte ID;
            public byte PaddedBytes; //Padding the certificate so it will be word aligned
            public short VLRLength;
        }

        /**
         * A data structure that is defined in the DAL implementation and prepended 
         * to the signed message by DAL prior to EPID signing
         */
        public enum TaskInfoType
        {
            MeTask = 0,
            SeTask,
            MaxTask
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SigmaTaskInfoHeader
        {
            public TaskInfoType Type;
            public uint TaskInfoLen;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct IntelMETaskInfo
        {
            public SigmaTaskInfoHeader Hdr;
            public uint TaskId;
            public uint SubTaskId;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] RsvdMECore;
        }
    }
}