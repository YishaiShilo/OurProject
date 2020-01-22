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

#include <jhi.h>
#include <iostream>

#pragma once

using namespace std;

// Trusted Application commands IDs
static int CMD_INIT_SIGMA_AND_GET_S1_MSG		= 1;
static int CMD_GET_S3_MSG_LEN					= 2;
static int CMD_PROCESS_S2_MSG_AND_GET_S3_MSG	= 3;

// EPID Provisioning status codes
static int PROVISIONED		= 1;
static int NOT_PROVISIONED	= 0;

// SigmaLibrary status codes
static int STATUS_SUCCEEDED		= 0;
static int STATUS_FAILED		= -1;
static int INITIALIZE_FAILED	= -2;
static int INSTALL_FAILED		= -3;
static int OPEN_SESSION_FAILED	= -4;

#define S1_MESSAGE_LEN 104
#define INT_SIZE 4
#define GROUP_ID_LENGTH 4

class Sigma
{
public:
	static Sigma* Session();
	int Init();
	int GetS1Message(byte *s1Msg);
	int GetS3MessagLen(byte *s2Msg, int s2MsgLen, byte *s3MsgLen);
	int GetS3Message(byte *s2Msg, int s2MsgLen, int s3MessageLen, byte *s3Msg);		
	int Close();
	void getErrorMessage(byte *errorMsg, byte *errorMsgLen);

private:
	Sigma(void);
	~Sigma(void);
	bool initialized;	
	string taPath;
	string taId;
	JHI_HANDLE handle;
	JHI_SESSION_HANDLE session;
	int s3MessageLen;

	char* errorMessage;
	void setErrorMessage(const char* msg);
	void setErrorMessage(const char* msg, int msgLen);

};