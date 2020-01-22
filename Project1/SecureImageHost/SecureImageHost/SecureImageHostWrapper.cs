/**
***
*** Copyright  (C) 1985-2014 Intel Corporation. All rights reserved.
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

namespace CSharpClientUI
{
    public class SecureImageHostWrapper
    {
        [DllImport("secureimagelibrary", EntryPoint = "refresh", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool refresh();

        [DllImport("secureimagelibrary", EntryPoint = "getRemainingTimes", CallingConvention = CallingConvention.Cdecl)]
        public static extern int getRemainingTimes();

        [DllImport("secureimagelibrary", EntryPoint = "close", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool close(StringBuilder errorMsg);

        [DllImport("secureimagelibrary", EntryPoint = "closePavpSession", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool closePavpSession();

        [DllImport("secureimagelibrary", EntryPoint = "getGroupId", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool getGroupId(IntPtr groupId);

        [DllImport("secureimagelibrary", EntryPoint = "installApplet", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool installApplet();

        [DllImport("secureimagelibrary", EntryPoint = "resetSolution", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool resetSolution(StringBuilder errorMsg);

        [DllImport("secureimagelibrary", EntryPoint = "getPublicKey", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool getPublicKey(IntPtr mod, IntPtr exponent, IntPtr signed_modulus, IntPtr signed_exponent, IntPtr signature_nonce, StringBuilder errorMsg);

        public static bool getPublicKey(byte[] mod, byte[] exponent, byte[] signed_modulus, byte[] signed_exponent, byte[] signature_nonce, StringBuilder errorMsg)
        {
            IntPtr modPtr = Marshal.AllocHGlobal(mod.Length);
            Marshal.Copy(mod, 0, modPtr, mod.Length);

            IntPtr expPtr = Marshal.AllocHGlobal(exponent.Length);
            Marshal.Copy(exponent, 0, expPtr, exponent.Length);

            IntPtr signedModPtr = Marshal.AllocHGlobal(signed_modulus.Length);
            Marshal.Copy(signed_modulus, 0, signedModPtr, signed_modulus.Length);

            IntPtr signedExpPtr = Marshal.AllocHGlobal(signed_exponent.Length);
            Marshal.Copy(signed_exponent, 0, signedExpPtr, signed_exponent.Length);

            IntPtr noncePtr = Marshal.AllocHGlobal(signature_nonce.Length);
            Marshal.Copy(signature_nonce, 0, noncePtr, signature_nonce.Length);

            bool ret = getPublicKey(modPtr, expPtr, signedModPtr, signedExpPtr, noncePtr, errorMsg);

            Marshal.Copy(modPtr, mod, 0, mod.Length);
            Marshal.Copy(expPtr, exponent, 0, exponent.Length);
            Marshal.Copy(signedModPtr, signed_modulus, 0, signed_modulus.Length);
            Marshal.Copy(signedExpPtr, signed_exponent, 0, signed_exponent.Length);
            Marshal.Copy(noncePtr, signature_nonce, 0, signature_nonce.Length);

            Marshal.FreeHGlobal(modPtr);
            Marshal.FreeHGlobal(expPtr);
            Marshal.FreeHGlobal(signedModPtr);
            Marshal.FreeHGlobal(signedExpPtr);
            Marshal.FreeHGlobal(noncePtr);

            return ret;
        }

        [DllImport("secureimagelibrary", EntryPoint = "showImage", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool showImage(IntPtr ServerData, IntPtr targetControl, StringBuilder errorMsg);

        public static bool showImage(byte[] ServerData, IntPtr targetControl, StringBuilder errorMsg)
        {
            IntPtr bitstrPtr = Marshal.AllocHGlobal(ServerData.Length);
            Marshal.Copy(ServerData, 0, bitstrPtr, ServerData.Length);

            bool ret = showImage(bitstrPtr, targetControl, errorMsg);

            Marshal.FreeHGlobal(bitstrPtr);

            return ret;
        }


        // Sigma functions
        [DllImport("secureimagelibrary", EntryPoint = "GetS1Message", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetS1Message(IntPtr s1Msg);

        [DllImport("secureimagelibrary", EntryPoint = "GetS3MessageLen", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetS3MessagLen(byte[] s2Msg, int s2MsgLen, IntPtr s3MsgLen);


        [DllImport("secureimagelibrary", EntryPoint = "GetS3Message", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetS3Message(byte[] s2Msg, int s2MsgLen, int s3MessageLen, IntPtr s3Msg);
    }
}

