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
#include "ProtectedOutput.h"
#include <iostream>

#include "crypt_data_gen.h"
#include "sigma_crypto_utils.h"

int SigmaCryptoUtils::current_gid;

BOOL showImage(UINT8* ServerData, HWND targetControl,char* errorMsg)
{
	return ProtectedOutput::Session()->showImage(ServerData,targetControl,errorMsg);
}

BOOL getPublicKey(UINT8* mod,UINT8* exponent,UINT8* signed_modulus, UINT8* signed_exponent, UINT8* signature_nonce, char* errorMsg)
{
	BOOL res = ProtectedOutput::Session()->getPublicKey(mod,exponent,signed_modulus, signed_exponent, signature_nonce, errorMsg);
	return res;
}

BOOL getGroupId(byte *groupId)
{
	return ProtectedOutput::Session()->getGroupId(groupId);
}

BOOL refresh()
{
	return ProtectedOutput::Session()->refresh();
}

int getRemainingTimes()
{
	return ProtectedOutput::Session()->getRemainingTimes();
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
	return ProtectedOutput::Session()->closePavpSession();
}

BOOL resetSolution(char* errorMsg)
{
	return ProtectedOutput::Session()->resetAll(errorMsg);
}

BOOL close(char* errorMsg)
{
	return ProtectedOutput::Session()->close(errorMsg);
}