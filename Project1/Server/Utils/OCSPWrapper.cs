/**
***
*** Copyright  (C) 2014-2015 Intel Corporation. All rights reserved.
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
    class OCSPWrapper
    {
        [DllImport("TestOCSP.dll", EntryPoint = "Get_OCSPResponse", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint GetOCSPResponse(ref DataStructs.OCSPRequestInfo pReqInfo,
            string pOCSPRespCertName);
    }
}
