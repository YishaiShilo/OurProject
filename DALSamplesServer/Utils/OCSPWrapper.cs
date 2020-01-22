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
    class OCSPWrapper
    {
        [DllImport("TestOcsp.dll", EntryPoint = "GetOCSPResponse", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint GetOCSPResponse(ref SigmaDataStructs.OCSPRequestInfo pReqInfo,
            string pOCSPRespCertName);
    }
}
