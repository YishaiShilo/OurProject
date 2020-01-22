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

namespace DALSamplesServer.Handlers
{
    abstract class SampleHandler
    {
        public abstract void HandleClientCommunication(object client);
    }
}
