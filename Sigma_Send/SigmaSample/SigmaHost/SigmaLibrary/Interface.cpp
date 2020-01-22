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
#include "Interface.h"
#include "Sigma.h"
#include <iostream>

int GetS1Message(byte *s1Msg)
{
	int status = STATUS_SUCCEEDED;
	status = Sigma::Session()->Init();
	if(status == STATUS_SUCCEEDED)
		return Sigma::Session()->GetS1Message(s1Msg);
	return status;
}

int GetS3MessagLen(byte *s2Msg, int s2MsgLen, byte *s3MsgLen)
{
	return Sigma::Session()->GetS3MessagLen(s2Msg, s2MsgLen, s3MsgLen);
}

int GetS3Message(byte *s2Msg, int s2MsgLen, int s3MessageLen, byte *s3Msg)
{
	return Sigma::Session()->GetS3Message(s2Msg, s2MsgLen, s3MessageLen, s3Msg);
}

int Close()
{
	return Sigma::Session()->Close();
}

void GetErrorMessage(byte *errorMsg, byte *errorMsgLen)
{
	return Sigma::Session()->getErrorMessage(errorMsg, errorMsgLen);
}