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
#pragma once
#include <windows.h>
#include <stdio.h>
#include <iostream>
#include <jhi.h>
#include <DelayImp.h>
#include "PavpHandler.h"
#include "PavpWysHandler.h"
#include "WysImage.h"


using namespace std;

//TA command IDs
static int CMD_GET_GROUP_ID = 0;
static int CMD_DECRYPT_SYMETRIC_KEY			= 5;
static int CMD_GENERATE_PAVP_SESSION_KEY	= 6;
static int CMD_LOAD_METADATA				= 7;
static int CMD_GET_TA_DATA					= 8;
static int CMD_GET_REMAINING_TIMES			= 9;
static int CMD_SET_NONCE					= 10;
static int CMD_IS_PROVISIONED				= 11;
static int CMD_RESET						= 12; 

//Authentication command IDs:
static int CMD_SEND_AUTHENTICATION_ID = 22;

// Sigma command IDs
static int CMD_INIT_AND_GET_S1 = 1;
static int CMD_GET_S3_MESSAGE_LEN = 2;
static int CMD_PROCESS_S2_AND_GET_S3 = 3;
static int STATUS_SUCCEEDED = 0;
static int STATUS_FAILED = -1;
static int INITIALIZE_FAILED = -2;
static int INSTALL_FAILED = -3;
static int OPEN_SESSION_FAILED = -4;



//convention definitions
static int PROVISIONED = 0;
static int NOT_PROVISIONED = 1;

//length definitions
#define EPID_NONCE_LEN 32
#define EPID_SIGNATURE_LEN 569
#define ERROR_MESSAGE_LEN 200
#define GROUP_ID_LENGTH 4
#define S1_MESSAGE_LEN 104
#define INT_SIZE 4


// WYS.h
/**********************************************************************************************//**
 * 	Values that represent WYSRESULT value. 
 **************************************************************************************************/
#define COLOR_WHITE 0xFFFFFFFF
#define COLOR_BLACK 0x00000000
#define COLOR_RED   0x0000FFFF
#define COLOR_GREEN 0x00FF0000
#define COLOR_BLUE  0xFF000000

#define COMMAND_ID_CHECK_INPUT_STATUS 20
#define COMMAND_ID_GET_OTP 21
#define RESP_CODE_APPLET_SUCCESS 0

#define LOFF	0
#define LOFF2	1
#define LOFF3	2

/**********************************************************************************************//**
 * Using WYS_STANDARD_COMMAND_ID will forward the command to the WYS standard commands processor. 
 * Applications using standard local and remote WYS features provided by Intel are expected to use 
 * WYS_STANDARD_COMMAND_ID while using WYSSendRecv() to communicate with the applet.
 * To communicate with a non-intel provided applet, the application and applet communication 
 * need to be configured with WYS_COMMSMODE_DIRECT enabled in the commsConfig parameter passed to WYSInit()
**************************************************************************************************/
#define WYS_STANDARD_COMMAND_ID  0xFFFF0001 // WYS standard command ID, the command ID will 
											// indicate that the command should be forwarded by 
											// applet to WYS standard commands processor

/**********************************************************************************************//**
 * 	Values that represent valid user defined WYS Image Ids.
 * 	User provided WYS Image Ids range is 0x80000001 - 0xFFFFFFFE
 **************************************************************************************************/
#define WYS_IMAGE_ID_ALL				0xFFFFFFFF	// used to specify all wys images
#define WYS_USER_IMAGE_ID_RANGE_START	0x80000001	// user provided image id range start
#define WYS_USER_IMAGE_ID_RANGE_END		0xFFFFFFFE	// user provided image id range end

#define WYS_MAX_ALLOWED_CLICKS_ON_IMAGE		127 
#define WYS_MAX_CAPTCHA_INPUT_LENGTH		127 
#define WYS_MAX_CREATE_WYSIMAGE_SIZE		65000

#pragma pack(1)

typedef JVM_COMM_BUFFER APPLETSENDRECVDATAPARAMS;

typedef struct _WYSWINDOW_PROPS {
	
	IN  HWND WindowHandle;			// Cannot be NULL. jhiHandle of the window in which to display the
  									// image. This is a platform specific Window jhiHandle.
  									// e.g. HWND on windows and X11 ID on Unix based systems.
	IN  unsigned int windowBackgroundColor; // XRGB/ARGB
	IN  void *taInstanceId;				// Applet instance id which can be used with other
											// jhiSessionHandles
}WYSWINDOW_PROPS, *PWYSWINDOW_PROPS;


/**********************************************************************************************//**
 * 	Wys Image component RGB colors. 
 **************************************************************************************************/
typedef struct _WYSIMAGE_COMPONENT_COLORS {
	INT_BYTES   wysImageColor;			// wys image background color (RGB)
	INT_BYTES   frameColor;				// internal frame color (RGB)
	INT_BYTES   frameBorderColor;		// internal frame border color (RGB)
	INT_BYTES   buttonColor;			// button color (RGB), relevant only for PIN pad and OK box windows
	INT_BYTES   buttonBorderColor;		// button border color (RGB), relevant only for PIN pad and OK box windows
	INT_BYTES   fontColor;				// font color (RGB)
}WYSIMAGE_COMPONENT_COLORS;

/**********************************************************************************************//**
 * 	Create wys image structure. 
 **************************************************************************************************/
typedef struct _CREATE_WYSIMAGE {

	unsigned char	reserved0;			// Reserved0
	unsigned char   wysImageType;		// WysStandardImagePinPad or WysStandardImageOK or WysStandardImageCaptcha
	INT_BYTES		reserved1;			// Reserved1
	INT_BYTES		appParam;			// Identifier for the image to be used subsequently to 
 										// identify the user input for this instance of the image.
 										// Cannot be WYS_IMAGE_ID_ALL, but can be in the user wys image id range.
	XY_PAIR_BYTES	wysImageSize;		// Size of the WYS Image in pixels. Note this is not the size of the
  										// parent Window in which the WYS image is displayed.
										// It can be equal to the client area of the parent Window in 
  										// which case Wys Image Position <x,y> should be <0,0>
	unsigned char	frameBorderWidth;   // thickness of frame border in pixels. 0 means no border.
	XY_PAIR_BYTES	buttonSize;			// button size in pixels
	unsigned char	buttonBorderWidth;  // thickness of button border in pixels. 0 means no border.
	unsigned char	fontType;			// type of font to use for button and captcha.
	unsigned char	textLength;			// length of the CAPTCHA text to be generated.
	WYSIMAGE_COMPONENT_COLORS colors;	// colors for all the various components on the wys image.
	XY_PAIR_BYTES	logoSize;			// size of the logo image in width and height. specify zero for no logo.
	unsigned char	logoImage[0];		// logo image data (contains width x height RGB [24bpp] values)
										// should follow logoSize.
}CREATE_WYSIMAGE, *PCREATE_WYSIMAGE;

//
// Request message for SUB_COMM_SUBMIT_INPUT for CAPTCHA window 
//
typedef struct _SUBMIT_INPUT_TEXT_MSG
{
	unsigned char  subCommand; // SUB_COMM_SUBMIT_INPUT
	unsigned char  textSize;   // the length of the submitted text
	unsigned char  text[0];    // the text entered by user in ASCII format, length is according to "textSize" field
} SUBMIT_INPUT_TEXT_MSG;

//
// Request message for SUB_COMM_SUBMIT_INPUT for PIN pad window and OK box window 
//
typedef struct _SUBMIT_INPUT_CLICKS_MSG
{
	unsigned char  subCommand;    // SUB_COMM_SUBMIT_INPUT
	unsigned char  clicksCount;   // the number of user's clicks on window
	XY_PAIR_BYTES  clicks[0];     // locations of the clicks, length is according to "clicksCount" field
} SUBMIT_INPUT_CLICKS_MSG;

//
// Response message to SUB_COMM_BUILD_WINDOW
//
typedef struct _CREATE_WINDOW_RESPONSE_MSG
{
	unsigned char        subCommand;             // SUB_COMM_BUILD_WINDOW
	unsigned char        windowType;             // WINDOW_TYPE_PINPAD, WINDOW_TYPE_OK or WINDOW_TYPE_CATCHA
	INT_BYTES            jhiHandle;                 // jhiHandle to the rendered image that should be used later for SUB_COMM_GET_IMAGE_CHUNK
	unsigned char        S1Kb[KEY_RECORD_SIZE];  // encrypted jhiSessionHandle key record
	unsigned char        CryptoCounter[CRYPTO_COUNTER_SIZE];  // Crypto counter value, generated in FW during the rendering
	INT_BYTES            imageSize;              // total rendered image size
	WIDGET_MAPPING_BYTES map[0];                 // the mapping of the buttons on the window (size = 10), only returned for WINDOW_TYPE_PINPAD
} CREATE_WINDOW_RESPONSE_MSG;

//
// Request message for SUB_COMM_CANCEL 
// 
typedef struct _CANCEL_MSG
{
	unsigned char  subCommand; // SUB_COMM_CANCEL
} CANCEL_MSG;



class SecureImage
{
public:
	//return current instance of the SecureImage class
	static SecureImage* Session();

	/*Return the TA public key abd signature, at first run or after reset- will generate a new pair of keys*/
	bool getPublicKey(byte* modulus,byte* exponent, byte* signed_modulus, byte* signed_exponent, byte* signature_nonce, char* errorMsg);

	/*Parses the server data nad presents the image */
	bool showImage(UINT8* ServerData, HWND targetControl,char* errorMsg);	

	/*Refereshes the view of the image. should be called periodically while image is displayed*/
	bool refresh();

	/*Close the soltion - will close PAVP session, save metadata and deinit TA and JHI*/
	bool close(char* errorMsg);

	/*Close the PAVP session*/
	bool closePavpSession();

	/*Return the number of times the current image can be viewed, -1 for failure.*/
	int getRemainingTimes();

	/*Reset the solution - will clean the TA keys and metadata and will reset the TA MTC.
	Note that in real use cases - this option should not be exposed so easily, as it is a potential security hole.*/
	bool resetAll(char* errorMsg);

	//Get the EPID Group ID for this platform
	bool getGroupId(byte *groupId);

	// Sigma functions:
	int GetS1Message(byte *s1Msg);

	int GetS3MessageLen(byte *s2Msg, int s2MsgLen, byte *s3MsgLen);

	int GetS3Message(byte *s2Msg, int s2MsgLen, int s3MessageLen, byte *s3Msg);


	//Authentication function:
	int sendAuthenticationId(byte *AuthenticationId, int Len, byte *encryptedId);

	// WYS functions
	WYSRESULT doWysSequence(HWND windowHandle, unsigned char wysImageType);

	WYSRESULT onMouseDown(HWND windowHandle, UINT16 x, UINT16 y);
	WYSRESULT onMouseUp(UINT16 x, UINT16 y);

	UINT32 onClickSubmit(wchar_t* userInput, UINT16 inputLength);
	WYSRESULT onClickClear();
	bool closePavpWysSession();

	bool getOtp(void* outArr, int arrLength);

	bool getPin(void* outArr, int arrLength);

private:


	//functions:

	SecureImage(void);
	~SecureImage(void);

	//helper function to perform JHI calls and return errors
	bool callJHI(JVM_COMM_BUFFER* buff, int command, char* errorMsg);

	//load solution and TA metadata
	bool loadData();

	//Save solution and TA metadata
	bool saveData(char* errorMsg);

	//Check if current platform is EPID provisioned
	bool isProvisioned(char* errorMsg);

	//helper function to parse the meta data
	int parseMetaData(byte *rawData);

	//helper function to translate JHI error codes to string
	const char* getJHIRet(JHI_RET ret);

	//**data members**
	//path to the Intel DAL trusted application 
	string taPath;
	//UUID of the trusted application.
	string taId;
	//path of teh metadata file
	string metaDataPath;
	//flag indicating whether the class was initialized succeessflly or not
	bool initialized;
	//char pointer that stores the initialization error(if exists)
	char initializationError[ERROR_MESSAGE_LEN];
	//flag indicating whether there is any data to save or not
	bool isAnyData;

	//handle of JHI session
	JHI_HANDLE handle;
	//handle of TA session
	JHI_SESSION_HANDLE session;

	//current displayed encrypted bitmap
	UINT8* encryptedCodeBitmap;


	

	//buffer that stores the keys as they are recieved from the TA
	byte keysData[1664];

	// WYS members
	UINT32 imageId;
	WYSIMAGEOBJS_HASH_MAP	m_wysImageObjs;
	//flag indicate whether PAVP jhiSessionHandle exists or not
	bool sessionExists;

	//Instance of PavpWysHandler
	PavpWysHandler handler;

	//encrypted bitmap we display
	UINT8* encryptedBitmap;

	UINT32 pavpSessionHandle;

	WYSWINDOW_PROPS wysWindowProps;

	// WYS functions
	WYSRESULT doWysDisplay(UINT32* pavpSessionHandle, unsigned char wysImageType);
	WYSRESULT createStdImage(UINT32 pavpSessionHandle, IN XY_PAIR_BYTES *pwysImagePosition, IN CREATE_WYSIMAGE *pWysImageCreateParams, IN UINT32 wysImageCreateParamsLen, IN void *rsvdparam1, IN UINT32 rsvparam2);
	WYSRESULT displayImage(XY_PAIR_BYTES *pWysImagePosition, CREATE_WYSIMAGE *pWysImage);
	WYSRESULT sendWYSStdWindowRequest(CREATE_WYSIMAGE *pWysWindowParams, CREATE_WINDOW_RESPONSE_MSG *pRespData, UINT32 *pRespDataLength);
	WYSRESULT repaintImage(UINT32* pavpSessionHandle, UINT32 wysImageId);

	void deleteWysImageObject(WysImage *pWysImageObj, unsigned int wysImageId);
	WysImage* getWysImageObject(unsigned int wysImageId);
	WYSRESULT releaseImageX(UINT32 wysImageId = WYS_IMAGE_ID_ALL, BOOL bCancel = FALSE);
	WYSRESULT getLocalWysImageFromFWAndDisplay(CREATE_WINDOW_RESPONSE_MSG *windowResp, RECT *wysImageRect, void **pDecoderRenderTargets);
	WYSRESULT getImageFromFW(UINT32 mejhiHandle, UINT32 ImageSize, UINT8 *pImageBuff);
	WYSRESULT jhiSendRecv(UINT32 reqBufLen, void *pSendReq, UINT32 recvBufLen, void *pRecvBuf, UINT32 *pRespDataLen = NULL, UINT8 *pRespData = NULL);
	bool drawClickEffect(HWND windowHandle, int btnL, int btnT, int btnW, int btnH, DWORD color);

	WYSRESULT submitClickInput(INT32 wysImageType, XYPAIR_VECTOR *Clicks);
	WYSRESULT submitCaptchaString(UINT32 captchaStringLen, const wchar_t *sCaptchaString);
	WYSRESULT ReleaseImageX(UINT32 wysImageId = WYS_IMAGE_ID_ALL, BOOL bCancel = FALSE);
	WYSRESULT wysCancel(unsigned int appParam);
	void DeleteWysImageObject(WysImage *pWysImageObj, unsigned int wysImageId);
	inline bool eraseClickEffect(WysImage *pWysImageObj) { return FAILED(handler.DisplayVideo(pWysImageObj->m_pDecoderRenderTargets, &pWysImageObj->m_wysImgRenderRect, true)); }
};

