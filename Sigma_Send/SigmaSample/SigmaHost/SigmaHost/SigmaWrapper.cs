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
using System.Runtime.InteropServices;

namespace SigmaHostGUI
{
    class SigmaWrapper
    {
        [DllImport("sigmalibrary", EntryPoint = "GetS1Message", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetS1Message(IntPtr s1Msg);

        [DllImport("sigmalibrary", EntryPoint = "GetS3MessagLen", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetS3MessagLen(byte[] s2Msg, int s2MsgLen, IntPtr s3MsgLen);

        [DllImport("sigmalibrary", EntryPoint = "GetS3Message", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetS3Message(byte[] s2Msg, int s2MsgLen, int s3MessageLen, IntPtr s3Msg);

        [DllImport("sigmalibrary", EntryPoint = "Close", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Close();

        [DllImport("sigmalibrary", EntryPoint = "GetErrorMessage", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetErrorMessage(IntPtr errorMessage, IntPtr errorMsgLen);
    }
}
