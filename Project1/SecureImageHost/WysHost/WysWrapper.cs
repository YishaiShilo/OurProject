﻿/**
***
*** Copyright  (C) 2013-2014 Intel Corporation. All rights reserved.
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

namespace WysHost
{
    class WysWrapper
    {
       public static UInt32 SAMPLE_CODE_SUCCESS	                = 0;

       public static Byte WYS_IMAGE_TYPE_PINPAD = 1;
       public static Byte WYS_IMAGE_TYPE_OKBUTTON = 2;
       public static Byte WYS_IMAGE_TYPE_CAPTCHA = 3;


       [DllImport("secureimagelibrary", EntryPoint = "doWysSequence", CallingConvention = CallingConvention.Cdecl)]
       public static extern UInt32 doWysSequence(IntPtr hwnd, Byte wysImageType);

       [DllImport("secureimagelibrary", EntryPoint = "onMouseDown", CallingConvention = CallingConvention.Cdecl)]
       public static extern bool onMouseDown(IntPtr hwnd, UInt16 x, UInt16 y);

       [DllImport("secureimagelibrary", EntryPoint = "onMouseUp", CallingConvention = CallingConvention.Cdecl)]
       public static extern bool onMouseUp(UInt16 x, UInt16 y);

       [DllImport("secureimagelibrary", EntryPoint = "onClickSubmit", CallingConvention = CallingConvention.Cdecl)]
       public static extern bool onClickSubmit([MarshalAs(UnmanagedType.LPWStr)]string userInput, UInt16 inputLength);

       [DllImport("secureimagelibrary", EntryPoint = "onClickClear", CallingConvention = CallingConvention.Cdecl)]
       public static extern bool onClickClear();

       [DllImport("secureimagelibrary", EntryPoint = "getOtp", CallingConvention = CallingConvention.Cdecl)]
       public static extern bool getOtp(IntPtr outArr, int arrLength);

       [DllImport("secureimagelibrary", EntryPoint = "close", CallingConvention = CallingConvention.Cdecl)]
       public static extern bool Close();

       [DllImport("secureimagelibrary", EntryPoint = "closePavpSession", CallingConvention = CallingConvention.Cdecl)]
       public static extern bool closePavpSession();
    }
}
