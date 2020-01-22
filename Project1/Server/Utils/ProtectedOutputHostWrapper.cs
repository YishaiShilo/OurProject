/**
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
using System.Runtime.InteropServices;

namespace DALSamplesServer
{

    public class ProtectedOutputHostWrapper
    {
        [DllImport("protectedoutputhost", EntryPoint = "encryptBitmap", CallingConvention = CallingConvention.Cdecl)]
        private static extern uint encryptBitmap(IntPtr plainBitmap, Int32 plainBitmapSize, IntPtr encryptedBitmap, IntPtr key);

        public static byte[] encryptBitmap(byte[] plainBitmap, byte[] key)
        {
            IntPtr plainBmpPtr = Marshal.AllocHGlobal(plainBitmap.Length);
            Marshal.Copy(plainBitmap, 0, plainBmpPtr, plainBitmap.Length);

            IntPtr encryptedTextPtr = Marshal.AllocHGlobal(plainBitmap.Length);

            IntPtr keyPtr = Marshal.AllocHGlobal(key.Length);
            Marshal.Copy(key, 0, keyPtr, key.Length);

            encryptBitmap(plainBmpPtr, plainBitmap.Length, encryptedTextPtr, keyPtr);
            
            byte[] encryptedText = new byte[plainBitmap.Length];
            Marshal.Copy(encryptedTextPtr, encryptedText, 0, plainBitmap.Length);

            Marshal.FreeHGlobal(plainBmpPtr);
            Marshal.FreeHGlobal(encryptedTextPtr);
            Marshal.FreeHGlobal(keyPtr);

            return encryptedText;
        }
    }
}