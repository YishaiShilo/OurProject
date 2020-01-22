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

namespace DALSamplesServer.Utils
{
    class GeneralUtils
    {
        public static object ByteArrayToStructure(byte[] bytearray, ref object obj)
        {
            int len = Marshal.SizeOf(obj);
            IntPtr i = Marshal.AllocHGlobal(len);
            Marshal.Copy(bytearray, 0, i, len);
            obj = Marshal.PtrToStructure(i, obj.GetType());
            Marshal.FreeHGlobal(i);
            return obj;

        }

        public static byte[] StructureToByteArray(object obj)
        {
            int len = Marshal.SizeOf(obj);
            byte[] byteArray = new byte[len];
            IntPtr ptr = Marshal.AllocHGlobal(len);
            Marshal.StructureToPtr(obj, ptr, true);
            Marshal.Copy(ptr, byteArray, 0, len);
            Marshal.FreeHGlobal(ptr);
            return byteArray;
        }

        /**
         * Compares two arrays; each starts from a different offset
         * Returns true if the arrays are equal in the given range
         */
        public static bool CompareArray(Array firstArray, int firstArrayOffset, Array secondArray, int secondArrayOffset, int lengthToCompare)
        {
            if (firstArray == secondArray)
                return true;
            if (firstArray == null || secondArray == null)
                return false;
            if (firstArray.Length < firstArrayOffset + lengthToCompare)
                return false;
            if (secondArray.Length < secondArrayOffset + lengthToCompare)
                return false;
            for (int i = 0; i < lengthToCompare; i++)
            {
                if (!firstArray.GetValue(firstArrayOffset + i).Equals(secondArray.GetValue(secondArrayOffset + i)))
                    return false;
            }
            return true;
        }
    }
}
