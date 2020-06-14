/**
***
*** Copyright  (C) 1985-2014 Intel Corporation. All rights reserved.
***
*** The information and source code contained herein is the exclusive
*** property of Intel Corporation. and may not be disclosed, examined
*** or reproduced in whole or in part without explicit written authorization
*** from the company.
***
*** ----------------------------------------------------------------------------
**/
#include "interface.h"
#include "PavpHandler.h"
#include "SecureImage.h"
#include <iostream>

#include "crypt_data_gen.h"
#include "sigma_crypto_utils.h"

int SigmaCryptoUtils::current_gid;

BOOL showImage(UINT8* ServerData, HWND targetControl,char* errorMsg)
{
	return SecureImage::Session()->showImage(ServerData,targetControl,errorMsg);
}


BOOL installApplet()
{
		SecureImage::Session();
		return true; // TODO return something more relevant

}

BOOL getGroupId(byte *groupId)
{
	return SecureImage::Session()->getGroupId(groupId);
}

BOOL refresh()
{
	return SecureImage::Session()->refresh();
}

int getRemainingTimes()
{
	return SecureImage::Session()->getRemainingTimes();
}

BOOL encryptBitmap(UINT8* plainBitmap,UINT32 plainBitmapSize,UINT8* encryptedBitmap,UINT8* key)
{
	unsigned int dwCounter[4] = {0,0,0,1};

    DWORD temp   = dwCounter[2];
    dwCounter[2] = dwCounter[3];
    dwCounter[3] = temp;

    SwapEndian_8B(dwCounter);
    SwapEndian_8B(dwCounter+2); 

	if (plainBitmapSize <= 54)
		return false;

	CdgStatus status=Aes128CtrEncrypt(plainBitmap + 54,plainBitmapSize - 54,encryptedBitmap,plainBitmapSize - 54,key,16,dwCounter,32);

	return status==0;
}

BOOL closePavpSession()
{
	return SecureImage::Session()->closePavpSession();
}

BOOL resetSolution(char* errorMsg)
{
	return SecureImage::Session()->resetAll(errorMsg);
}

BOOL close(char* errorMsg)
{
	return SecureImage::Session()->close(errorMsg);
}

// Sigma functions
int GetS1Message(byte *s1Msg)
{
	return SecureImage::Session()->GetS1Message(s1Msg);
}


int GetS3MessageLen(byte *s2Msg, int s2MsgLen, byte *s3MsgLen)
{
	//Get S3 message length
	return SecureImage::Session()->GetS3MessageLen(s2Msg, s2MsgLen, s3MsgLen);
}

int GetS3Message(byte *s2Msg, int s2MsgLen, int s3MessageLen, byte *s3Msg)
{
	//Get S3 message
	return SecureImage::Session()->GetS3Message(s2Msg, s2MsgLen, s3MessageLen, s3Msg);
}


// Authentication function
int sendAuthenticationId(byte *AuthenticationId, int Len, byte *encryptedId) 
{
	//Get S3 message
	return SecureImage::Session()->sendAuthenticationId(AuthenticationId, Len, encryptedId);
}



// WYS functions
WYSRESULT doWysSequence(HWND windowHandle, unsigned char wysImageType)
{
	return SecureImage::Session()->doWysSequence(windowHandle, wysImageType);
}


bool onMouseDown(HWND windowHandle, UINT16 x, UINT16 y)
{
	return SecureImage::Session()->onMouseDown(windowHandle, x, y);
}

bool onMouseUp(UINT16 x, UINT16 y)
{
	return SecureImage::Session()->onMouseUp(x, y);
}

bool onClickSubmit(wchar_t* userInput, UINT16 inputLength)
{
	return SecureImage::Session()->onClickSubmit(userInput, inputLength) == 0;
}

bool onClickClear()
{
	return SecureImage::Session()->onClickClear();
}

bool getOtp(void* outArr, int arrLength)
{
	return SecureImage::Session()->getOtp(outArr, arrLength);
}

bool getPin(void* outArr, int arrLength)
{
	return SecureImage::Session()->getPin(outArr, arrLength);
}

bool closePavpWysSession()
{
	return SecureImage::Session()->closePavpWysSession();
}
//
//bool close()
//{
//	return SecureImage::Session()->close();
//}
