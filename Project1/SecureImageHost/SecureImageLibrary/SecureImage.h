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

using namespace std;

//TA command IDs
static int CMD_GET_GROUP_ID = 0;
static int CMD_GENERATE_KEYS				= 1;
static int CMD_LOAD_PRIVATE_KEY				= 2;
static int CMD_LOAD_PUBLIC_KEY_EXPONENT		= 3;
static int CMD_LOAD_PUBLIC_KEY_MODULUS		= 4;
static int CMD_DECRYPT_SYMETRIC_KEY			= 5;
static int CMD_GENERATE_PAVP_SESSION_KEY	= 6;
static int CMD_LOAD_METADATA				= 7;
static int CMD_GET_TA_DATA					= 8;
static int CMD_GET_REMAINING_TIMES			= 9;
static int CMD_SET_NONCE					= 10;
static int CMD_IS_PROVISIONED				= 11;
static int CMD_RESET						= 12; 


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

	int GetS3MessagLen(byte *s2Msg, int s2MsgLen, byte *s3MsgLen);

	int GetS3Message(byte *s2Msg, int s2MsgLen, int s3MessageLen, byte *s3Msg);

private:
	//functions:

	SecureImage(void);
	~SecureImage(void);
	//helper function to perform JHI calls and return errors
	bool callJHI(JVM_COMM_BUFFER* buff, int command, char* errorMsg);
	//load the RSA key pair to the TA
	bool loadKeys(char* errorMsg);
	//Ask the TA to generate a new RSA key pair
	bool generateKeys(char* errorMsg);
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

	//data members:

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
	UINT8* encryptedBitmap;

	//exponent
	byte* e;
	//encrypted private key
	byte* d;
	//mod
	byte* mod;
	//exponent signature
	byte* signed_e;
	//mod signature
	byte* signed_mod;
	//signature none
	byte* nonce;
	//buffer that stores the keys as they are recieved from the TA
	byte keysData[1664];

	//size of the key mod
	short mod_size;// should be 256
	//size of the key exponent
    short e_size;// should be 4
	//size of the encrypted private key
    short d_size;// should be 256
	//size of the signature of the key mod
	short signed_mod_size;
	//size of the signature of the key exponent
    short signed_e_size;
};

