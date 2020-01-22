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
#include <Windows.h>
#include <d3d9.h>

#ifdef __cplusplus
extern "C" {
#endif

#define DRM_EXPORT __declspec(dllexport) BOOL
#define SIGMA_EXPORT __declspec(dllexport) int


/*Parses the server data nad presents the image */
DRM_EXPORT showImage(UINT8* ServerData, HWND targetControl, char* errorMsg);

/*Refereshes the view of the image. should be called periodically while image is displayed*/
DRM_EXPORT refresh();

/*Return the TA public key abd signature, at first run or after reset- will generate a new pair of keys*/
DRM_EXPORT getPublicKey(byte* modulus,byte* exponent, byte* signed_modulus, byte* signed_exponent, byte* signature_nonce,  char* errorMsg);

/*Encrypt a bitmap*/
DRM_EXPORT encryptBitmap(UINT8* plainBitmap,UINT32 plainBitmapSize,UINT8* encryptedBitmap,UINT8* key);

/*Close the soltion - will close PAVP session, save metadata and deinit TA and JHI*/
DRM_EXPORT close(char* errorMsg);

/*Close the PAVP session*/
DRM_EXPORT closePavpSession();

/*Reset the solution - will clean the TA keys and metadata and will reset the TA MTC.
Note that in real use cases - this option should not be exposed so easily, as it is a potential security hole.*/
DRM_EXPORT resetSolution(char* errorMsg);

//Get the EPID Group ID for this platform
DRM_EXPORT installApplet();

//Get the EPID Group ID for this platform
DRM_EXPORT getGroupId(byte *groupId);

/*Return the number of times the current image can be viewed. -1 for failure.*/
__declspec(dllexport) int getRemainingTimes(); 


// Sigma functions
SIGMA_EXPORT GetS1Message(byte *s1Msg);


#ifdef __cplusplus
};
#endif
