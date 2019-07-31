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

/*
Prior to Visual Studio 2015 Update 3, the delay load function hooks were non - const.
They were made const to improve security.
For backwards compatibility with previous Visual Studio versions, we define
the macro DELAYIMP_INSECURE_WRITABLE_HOOKS prior to including DelayImp header
(See details in the DelayImp header file before __pfnDliNotifyHook2 declaration).
*/
#define DELAYIMP_INSECURE_WRITABLE_HOOKS

#include <windows.h>
#include <DelayImp.h>
#include "Sigma.h"

#pragma comment (lib, "wintrust")
#pragma comment(lib, "delayimp")

//Prototype for the delay load function
FARPROC WINAPI delayHook(unsigned dliNotify, PDelayLoadInfo pdli);
PfnDliHook __pfnDliNotifyHook2 = delayHook;

// Factory method to get the Sigma class single instance
Sigma* Sigma::Session()
{
	static Sigma instance;
	return &instance;
}

Sigma::Sigma(void)
{
	initialized = false;
	errorMessage = NULL;
}

// Initializes the JHI, installs the TA and opens a session with the TA 
// Returns STATUS_SUCCEEDED in case of a success, or a compatible error otherwise
int Sigma::Init()
{
	try
	{
		// This is the path to the Intel(R) DAL SigmaTA trusted application
		taPath = getenv("DALSDK");
		taPath.append("\\Samples\\SigmaSample\\SigmaTA\\bin\\SigmaTA.dalp");
		// This is the UUID of this trusted application.
		// This is the same value as the UUID field in the trusted application manifest.
		taId = "21baa750f298482e8ea1a9b529fd3273";

		handle = NULL;
		JHI_RET ret;

		try
		{
			// Initialize the JHI
			// Note: this is the first call to JHI function => JHI.dll will now be delay-loaded
			// The function delayHook will be called and will perform the load from a trusted path with signature verification
			ret = JHI_Initialize(&handle, NULL, 0);
			if (ret != JHI_SUCCESS)
				return INITIALIZE_FAILED;
		}
		catch (exception ex)
		{
			setErrorMessage(ex.what());
			return INITIALIZE_FAILED;
		}

		// Convert the TA path from char to wchar_t:
		wstring ws(taPath.begin(), taPath.end());

		// Install the Intel(R) DAL trusted application
		ret = JHI_Install2(handle, taId.c_str(), ws.c_str());
		if (ret != JHI_SUCCESS)
			return INSTALL_FAILED;

		// Start a session with the applet
		ret = JHI_CreateSession(handle, taId.c_str(), 0, NULL, &session);
		if (ret != JHI_SUCCESS)
			return OPEN_SESSION_FAILED;

		// Initialization accomplished
		initialized = true;
	}
	catch (exception ex)
	{
		setErrorMessage(ex.what());
		return STATUS_FAILED;
	}
	catch (...)
	{
		setErrorMessage("An exception has occurred during init flow.");
		return STATUS_FAILED;
	}

	return STATUS_SUCCEEDED;
}


// Fills the S1 message byte array with the S1 message it gets from the trusted application
// S1 message contains: Ga (The prover's ephemeral DH public key) || EPID GID || OCSPRequest(An (optional) OCSP Request from the prover (the Intel ME FW))
// Returns STATUS_SUCCEEDED in case of a success, or a compatible error code otherwise
int Sigma::GetS1Message(byte *s1Msg)
{
	// An empty message to the applet
	char* message = "";
	// A byte array that will contain the S1 message
	char rcvBuf[S1_MESSAGE_LEN] = { 0 };

	// Perform a Send and Receive operation with the Trusted Application in order to get S1 message
	JVM_COMM_BUFFER commBuf;
	commBuf.TxBuf->buffer = message;
	commBuf.TxBuf->length = strlen(message) + 1;
	commBuf.RxBuf->buffer = rcvBuf;
	commBuf.RxBuf->length = S1_MESSAGE_LEN;
	INT32 responseCode;
	JHI_RET ret = JHI_SendAndRecv2(handle, session, CMD_INIT_SIGMA_AND_GET_S1_MSG, &commBuf, &responseCode);
	if (ret != STATUS_SUCCEEDED)
	{
		setErrorMessage("Failed to perform send and receive operation.");
		return ret;
	}

	// Fill errorMsg in case of an exception in the trusted application code
	if (responseCode != STATUS_SUCCEEDED)
	{
		setErrorMessage((char*)commBuf.RxBuf->buffer, commBuf.RxBuf->length);
		return responseCode;
	}
	
	// Copy the S1 message received from the trusted application to this method caller' byte array	
	copy((byte*)commBuf.RxBuf->buffer, (byte*)(commBuf.RxBuf->buffer) + S1_MESSAGE_LEN, s1Msg);

	return STATUS_SUCCEEDED;
}

// Fills the S3 message length byte array with the S3 message length it gets from the trusted application
// Returns STATUS_SUCCEEDED in case of a success, or a compatible error code otherwise
int Sigma::GetS3MessagLen(byte *s2Msg, int s2MsgLen, byte *s3MsgLen)
{
	// The S2 message to be sent to the TA
	byte* message = s2Msg;
	// A byte array that will contain the S3 message length as INT_SIZE bytes
	byte rcvBuf[INT_SIZE] = { 0 };

	// Perform a call to the Trusted Application to get S3 message length
	JVM_COMM_BUFFER commBuf;
	commBuf.TxBuf->buffer = message;
	commBuf.TxBuf->length = s2MsgLen;
	commBuf.RxBuf->buffer = rcvBuf;
	commBuf.RxBuf->length = INT_SIZE;
	INT32 responseCode;
	JHI_RET ret = JHI_SendAndRecv2(handle, session, CMD_GET_S3_MSG_LEN, &commBuf, &responseCode);
	if (ret != STATUS_SUCCEEDED)
	{
		setErrorMessage("Failed to perform send and receive operation.");
		return STATUS_FAILED;
	}

	if (responseCode != STATUS_SUCCEEDED)
	{
		setErrorMessage("Failed to get EPID Provisioning status.");
		return responseCode;
	}
	
	// Copy the S3 message length received from the trusted application to this method caller' byte array	
	copy((byte*)commBuf.RxBuf->buffer, (byte*)(commBuf.RxBuf->buffer) + INT_SIZE, s3MsgLen);

	return STATUS_SUCCEEDED;
}


// Fills the S3 message byte array with the S3 message it gets from the trusted application
// S2 message contains: [EPIDCERTprover || TaskInfo || Ga(The prover's ephemeral DH public key) || EPIDSIG(Ga || Gb)]SMK
// Returns STATUS_SUCCEEDED in case of success, and specific error code otherwise
int Sigma::GetS3Message(byte *s2Msg, int s2MsgLen, int s3MessageLen, byte *s3Msg)
{
	// The S2 message to be sent to the TA
	byte* message = s2Msg;
	// A byte array that will contain the S3 message
	char *rcvBuf = new char[s3MessageLen];

	JVM_COMM_BUFFER commBuf;
	commBuf.TxBuf->buffer = message;
	commBuf.TxBuf->length = s2MsgLen;
	commBuf.RxBuf->buffer = rcvBuf;
	commBuf.RxBuf->length = s3MessageLen;
	INT32 responseCode;
	//perform call to the Trusted Application to get S3 message
	JHI_RET ret = JHI_SendAndRecv2(handle, session, CMD_PROCESS_S2_MSG_AND_GET_S3_MSG, &commBuf, &responseCode);
	if (ret != STATUS_SUCCEEDED)
	{
		setErrorMessage("Failed to perform send and receive operation.");
		return STATUS_FAILED;
	}

	// Fill errorMsg in case of an exception in the trusted application code
	if (responseCode != STATUS_SUCCEEDED)
	{
		setErrorMessage((char*)commBuf.RxBuf->buffer, commBuf.RxBuf->length);
		return responseCode;
	}

	// copy the S3 message received from the trusted application to this method caller' byte array	
	copy((byte*)commBuf.RxBuf->buffer, (byte*)(commBuf.RxBuf->buffer) + s3MessageLen, s3Msg);

	delete[] rcvBuf;

	return STATUS_SUCCEEDED;
}


// Closes the opened session with the TA, uninstalls the TA and deinits the JHI
// Returns STATUS_SUCCEEDED in case of a success, and STATUS_FAILED otherwise
int Sigma::Close()
{
	if (handle != NULL)
	{
		JHI_RET ret;
		// Close the session with the applet
		if (session != NULL)
		{
			ret = JHI_CloseSession(handle, &session);
			if (ret != JHI_SUCCESS)
				return STATUS_FAILED;
		}

		// Uninstall the applet
		ret = JHI_Uninstall(handle, const_cast<char*>(taId.c_str()));
		if (ret != JHI_SUCCESS)
			return STATUS_FAILED;

		// Deinit the JHI 
		ret = JHI_Deinit(handle);
		if (ret != JHI_SUCCESS)
			return STATUS_FAILED;
	}

	return STATUS_SUCCEEDED;
}
Sigma::~Sigma(void)
{
}

void Sigma::setErrorMessage(const char* msg)
{
	setErrorMessage(msg, strlen(msg));
}

void Sigma::setErrorMessage(const char* msg, int msgLen)
{
	errorMessage = new char[msgLen + 1];
	std::copy(msg, msg + msgLen, errorMessage);
	errorMessage[msgLen] = '\0';
}

void Sigma::getErrorMessage(byte *errorMsg, byte *errorMsgLen)
{
	if (errorMessage == NULL)
	{
		setErrorMessage("");
	}
	int errMsgLen = strlen(errorMessage);
	std::copy((byte*)errorMessage, (byte*)errorMessage + errMsgLen + 1, errorMsg);
	std::copy(static_cast<const char*>(static_cast<const void*>(&errMsgLen)), static_cast<const char*>(static_cast<const void*>(&errMsgLen)) + sizeof errMsgLen, errorMsgLen);
	delete[] errorMessage;
	errorMessage = NULL;
}
