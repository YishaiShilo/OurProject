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
#include <Windows.h>

#ifdef __cplusplus
extern "C" {
#endif

#define SIGMA_EXPORT __declspec(dllexport)

SIGMA_EXPORT int GetS1Message(byte *s1Msg);

SIGMA_EXPORT int GetS3MessagLen(byte *s2Msg, int s2MsgLen, byte *s3MsgLen);

SIGMA_EXPORT int GetS3Message(byte *s2Msg, int s2MsgLen, int s3MessageLen, byte *s3Msg);

SIGMA_EXPORT int Close();

SIGMA_EXPORT void GetErrorMessage(byte *errorMsg, byte *errorMsgLen);

#ifdef __cplusplus
};
#endif
