/**
***
*** Copyright  (C) 1985-2015 Intel Corporation. All rights reserved.
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

#include "SecureImage.h"
#include "atlimage.h"
#include <fstream>
#include <iostream>
#include <map>
//Prototype for the delay load function
FARPROC WINAPI delayHook(unsigned dliNotify, PDelayLoadInfo pdli);
PfnDliHook __pfnDliNotifyHook2 = delayHook;

// Instance of utility class
WysUtil Wysutil;

WYSCONFIG	gWysConfig = { 0 };
WYSCAPS		gWysCaps = { 0 };

using namespace std;

SecureImage::SecureImage(void)
{
	try
	{
		initialized=true;

		//This is the path to the Intel DAL Trusted Application that was created in Eclipse.
		taPath = "C:\\Project\\OurProject\\Project1\\SecureImageApplet\\bin\\SecureImageApplet.dalp";
		//taPath = "C:\\Users\\USER\\Desktop\\project\\OurProject\\OurProject\\Project1\\SecureImageApplet\\bin\\SecureImageApplet.dalp";

		taId = "33ad29312dd14387b073e9895fb9a5ef";

		cout << "taPath" << endl;
		

		//Initialize the JHI
		//Note: this is the first call to JHI function.
		//JHI.dll will now be delayed -load
		//The function delayHook will be called and will perform the load from a trusted path with signature verification
		JHI_RET ret = JHI_Initialize(&handle, NULL, 0);
		if(ret != JHI_SUCCESS)
		{
			strcpy(initializationError, "Failed to init JHI. Error: ");
			strcat(initializationError,getJHIRet(ret)); 
			initialized=false;
		}
		else
		{
			wstring ws(taPath.begin(),taPath.end());
			//Install the Intel DAL trusted application
			ret=JHI_Install2(handle,taId.c_str(),ws.c_str());	
			if(ret != JHI_SUCCESS)
			{
				strcpy(initializationError, "Failed to install TA. Error: ");
				strcat(initializationError,getJHIRet(ret)); 
				initialized=false;
			}
			else
			{
				//open session with the TA
				ret= JHI_CreateSession(handle, taId.c_str(), 0, NULL, &session);
				if(ret != JHI_SUCCESS)
				{
					strcpy(initializationError, "Failed to open session. Error: ");
					strcat(initializationError,getJHIRet(ret)); 
					initialized=false;
					session=NULL;
				}
			}
		}
	}
	catch(std::exception e)
	{
		strcpy(initializationError, e.what());
		initialized=false;
	}
}

SecureImage* SecureImage::Session()
{
	//initialized at the first time
	static SecureImage instance;
	//return pointer
	return &instance;
}

bool SecureImage::showImage(UINT8* ServerData, HWND targetControl,char* errorMsg)
{
	//if initialization error occured - return failure with the error
	if(!initialized)
	{
		strcpy(errorMsg,initializationError);
		return false;
	}
	UINT32 width,height,keySize,encryptedBitmapSize;
	
	//load the encrypted symetric key size
	memcpy(&keySize, &ServerData[8], 4);
	//allocate buffer in the size of encrypted symetric key
	UINT8* request=new UINT8[keySize];
	//copy the encrypted key
	memcpy(&request[0],&ServerData[12],keySize);
	//request the trusted application to decrypt the key
	JVM_COMM_BUFFER commBuf;
	commBuf.TxBuf->buffer = request;
	commBuf.TxBuf->length = keySize;
	commBuf.RxBuf->buffer = NULL;
	commBuf.RxBuf->length = 0;
	if(!callJHI(&commBuf,CMD_DECRYPT_SYMETRIC_KEY,errorMsg))
	{
		//failure
		delete[] request;
		return false;
	}
	delete[] request;

	//first four bytes are UINT32 representing width of the picture
	memcpy(&width, &ServerData[0], 4);
	//second four bytes are UINT32 representing height of the picture
	memcpy(&height, &ServerData[4], 4);
	//set the size, width and height, of the picture
	PavpHandler::Session()->SetControlSize(width,height);
	//initialize pavp with the window handle
	PavpHandler::Session()->Initialize(targetControl);
	//get ME handle from pavp,
	UINT ME_handle = 0;
	PavpHandler::Session()->GetMEhandle(&ME_handle);
	//init the TA request data
	UINT8 PAVPKeyRequest[4];
	//copy the ME handle to the buffer
	PAVPKeyRequest[0]=(byte)(ME_handle >> 3 * 8);
	PAVPKeyRequest[1]=(byte)((ME_handle << 1 * 8) >> 3 * 8);
	PAVPKeyRequest[2]=(byte)((ME_handle << 2 * 8) >> 3 * 8);
	PAVPKeyRequest[3]=(byte)((ME_handle << 3 * 8) >> 3 * 8);
	byte pavpKey[16];
	commBuf.TxBuf->buffer = PAVPKeyRequest;
	commBuf.TxBuf->length = 4;
	commBuf.RxBuf->buffer = pavpKey; // we get (S1)Kb
	commBuf.RxBuf->length = 16;
	//perform call to the Trusted Application to re-encrypt the symetric key with PAVP key
	if(!callJHI(&commBuf,CMD_GENERATE_PAVP_SESSION_KEY,errorMsg))
	{
		PavpHandler::Session()->ClosePAVPSession();
		return false;
	}
	// for print in hexa
	char buffer[33];
	buffer[32] = 0;
	for (int j = 0; j < 16; j++)
		sprintf(&buffer[2 * j], "%02X", pavpKey[j]);
	std::cout << "pavpKey: " << buffer << std::endl;
	//set the new key we got from the Trusted Application
	PavpHandler::Session()->SetNewKey(pavpKey);  // set (S1)Kb as pavpKey
	//next four bytes are UINT32 representing siz of the encrypted bitmap buffer size
	memcpy(&encryptedBitmapSize, &ServerData[12+keySize], 4);
	//allocate buffer for the encrypted bitmap
	encryptedCodeBitmap=new UINT8[encryptedBitmapSize];
	//copy the encrypted bitmap buffer from the server data
	memcpy(encryptedCodeBitmap,&ServerData[16+keySize],encryptedBitmapSize);
	//call the show function to display the image
	return PavpHandler::Session()->ShowImage(encryptedCodeBitmap,encryptedBitmapSize,targetControl);
}

bool SecureImage::refresh()
{
	//call PAVP refresh
	return PavpHandler::Session()->DisplayVideo()==0;
}

bool SecureImage::closePavpSession()
{
	bool res=true;
	//close PAVP session
	if(!PavpHandler::Session()->ClosePAVPSession())
		res= false;
	return res;
}

bool SecureImage::close(char* errorMsg)
{
	//save metadata
	//bool res = saveData(errorMsg);
	bool res = true;
	//close PAVP session
	if (!PavpHandler::Session()->ClosePAVPSession())
	{
		if (res)
			strcpy(errorMsg, "Failed to close PAVP session");
		res = false;
	}
	if (handle != NULL)
	{
		JHI_RET ret;
		//Close Trusted Application session
		if (session != NULL)
		{
			ret = JHI_CloseSession(handle, &session);
			if (ret != JHI_SUCCESS)
			{
				if (res)
				{
					strcpy(errorMsg, "Failed to close session. Error: ");
					strcat(errorMsg, getJHIRet(ret));
				}
				res = false;
			}
		}

		//Uninstall the Trusted Application
		ret = JHI_Uninstall(handle, const_cast<char*>(taId.c_str()));
		if (ret != JHI_SUCCESS)
		{
			if (res)
			{
				strcpy(errorMsg, "Failed to uninstall TA. Error: ");
				strcat(errorMsg, getJHIRet(ret));
			}
			res = false;
		}

		//Deinit the JHI handle
		ret = JHI_Deinit(handle);
		if (ret != JHI_SUCCESS)
		{
			if (res)
			{
				strcpy(errorMsg, "Failed to de-init JHI. Error: ");
				strcat(errorMsg, getJHIRet(ret));
			}
			res = false;
		}
	}
	////release all memory allocated
	//if (mod != NULL)
	//	delete[] mod;
	//if (e != NULL)
	//	delete[] e;
	//if (signed_mod != NULL)
	//	delete[] signed_mod;
	//if (signed_e != NULL)
	//	delete[] signed_e;
	//if (d != NULL)
	//	delete[] d;
	//if (nonce != NULL)
	//	delete[] nonce;
	//if (encryptedCodeBitmap != NULL)
	//	delete[] encryptedCodeBitmap;

	//set initializations flag to false
	initialized = false;
	isAnyData = false;

	return res;
}

int SecureImage::getRemainingTimes()
{
	//if initialization error occured - return failure
	if(!initialized)
		return false;

	INT32 responseCode;
	//return value is an integer - therefore size of 4 bytes
	byte rcvBuf[4]={0};
	JVM_COMM_BUFFER commBuf;
	commBuf.TxBuf->buffer = NULL;
	commBuf.TxBuf->length = 0;
	commBuf.RxBuf->buffer = rcvBuf;
	commBuf.RxBuf->length = 4;

	//get remaining times from the trusted application
	JHI_RET ret = JHI_SendAndRecv2(handle,session,CMD_GET_REMAINING_TIMES,&commBuf,&responseCode);
	if(ret != JHI_SUCCESS || responseCode!=0)
	{
		return -1;
	}	
	//conevrt the array to int
	int size = (int)((rcvBuf[0] << 24) | (rcvBuf[1] << 16) | (rcvBuf[2] << 8) | rcvBuf[3]);  
	return size;
}

bool SecureImage::resetAll(char* errorMsg)
{
	//check if there is a seeion with the TA
	//not checking initialization flag since a case where th metadata was not loaded is OK for a reset
	if(session==NULL)
	{
		strcpy(errorMsg, initializationError);
		return false;
	}
	//check if metadata file exists
	ifstream file(metaDataPath,ios::binary |ios::in|ios::ate);
	if(file.good())
	{
		file.close();
		//delete the file
		int ret_code=std::remove(metaDataPath.c_str());
		if(ret_code!=0)
		{
			strcpy(errorMsg,"Failed to delete meta-data file.");
			return false;
		}
	}

	//request the TA to reset (will reset MTC and TA memory)
	JVM_COMM_BUFFER commBuf;
	commBuf.TxBuf->buffer = NULL;
	commBuf.TxBuf->length = 0;
	commBuf.RxBuf->buffer = NULL;
	commBuf.RxBuf->length = 0;
	if(!callJHI(&commBuf,CMD_RESET,errorMsg))
		return false;

	//state is initialized - no data to load ans session is opened
	initialized=true;
	//no keys exist
	isAnyData=false;
	return true;
}


// Sigma functions

//This function fills the S1 message byte array with the S1 message it gets from the trusted application
//S1 message contains: Ga (The prover's ephemeral DH public key) || EPID GID || OCSPRequest(An (optional) OCSP Request from the prover(the ME))
//Returns STATUS_SUCCEEDED in case of success, and specific error code otherwise
int SecureImage::GetS1Message(byte *s1Msg)
{
	char* message = "";

	//rcvBuf - byte array that contains the S1 message
	char rcvBuf[S1_MESSAGE_LEN] = { 0 };

	JHI_RET ret;

	//Send and Receive
	JVM_COMM_BUFFER commBuf;
	commBuf.TxBuf->buffer = message;
	commBuf.TxBuf->length = strlen(message) + 1;
	commBuf.RxBuf->buffer = rcvBuf;
	commBuf.RxBuf->length = S1_MESSAGE_LEN;
	INT32 responseCode;
	//perform a call to the Trusted Application in order to get S1 message
	ret = JHI_SendAndRecv2(handle, session, CMD_INIT_AND_GET_S1, &commBuf, &responseCode);
	if (ret != STATUS_SUCCEEDED)
	{
		cout << "ret failed" << endl;
		return ret;
	}
	if (responseCode != STATUS_SUCCEEDED)
	{
		cout << "response failed" << endl;
		return responseCode;
	}
	//copy the S1 message received from the trusted application to the client bytes array	
	copy((byte*)commBuf.RxBuf->buffer, (byte*)(commBuf.RxBuf->buffer) + S1_MESSAGE_LEN, s1Msg);
	return STATUS_SUCCEEDED;
}

int SecureImage::GetS3MessageLen(byte *s2Msg, int s2MsgLen, byte *s3MsgLen)
{
	byte* message = s2Msg;

	//rcvBuf four bytes are INT32 representing the S3 message length
	byte rcvBuf[INT_SIZE] = { 0 };

	//Send and Receive
	JHI_RET ret;
	JVM_COMM_BUFFER commBuf;
	//place inside sendBuff the buffer containing S2 message
	commBuf.TxBuf->buffer = message;
	commBuf.TxBuf->length = s2MsgLen;
	commBuf.RxBuf->buffer = rcvBuf;
	commBuf.RxBuf->length = INT_SIZE;
	INT32 responseCode;
	//perform call to the Trusted Application to get S3 message length
	ret = JHI_SendAndRecv2(handle, session, CMD_GET_S3_MESSAGE_LEN, &commBuf, &responseCode);
	if (ret != STATUS_SUCCEEDED)
		return STATUS_FAILED;
	if (responseCode != STATUS_SUCCEEDED)
		return responseCode;
	//copy the S3 message length received from the trusted application to the client bytes array	
	copy((byte*)commBuf.RxBuf->buffer, (byte*)(commBuf.RxBuf->buffer) + INT_SIZE, s3MsgLen);
	return STATUS_SUCCEEDED;
}

//This function fills the S3 message byte array with the S3 message it gets from the trusted application
//S2 message contains: [EPIDCERTprover || TaskInfo || Ga(The prover's ephemeral DH public key) || EPIDSIG(Ga || Gb)]SMK
//Returns STATUS_SUCCEEDED in case of success, and specific error code otherwise
int SecureImage::GetS3Message(byte *s2Msg, int s2MsgLen, int s3MessageLen, byte *s3Msg)
{
	byte* message = s2Msg;
	//rcvBuf represents the S3 message
	char *rcvBuf = new char[s3MessageLen];

	//Send and Receive
	JHI_RET ret;
	JVM_COMM_BUFFER commBuf;
	//place inside sendBuff the buffer containing S2 message
	commBuf.TxBuf->buffer = message;
	commBuf.TxBuf->length = s2MsgLen;
	commBuf.RxBuf->buffer = rcvBuf;
	commBuf.RxBuf->length = s3MessageLen;
	INT32 responseCode;
	//perform call to the Trusted Application to get S3 message
	ret = JHI_SendAndRecv2(handle, session, CMD_PROCESS_S2_AND_GET_S3, &commBuf, &responseCode);
	if (ret != STATUS_SUCCEEDED)
	{
		cout << "ret:"  <<  ret << endl;
		return STATUS_FAILED;
	}
	if (responseCode != STATUS_SUCCEEDED)
		return responseCode;
	//copy the S3 message received from the trusted application to the client bytes array	
	copy((byte*)commBuf.RxBuf->buffer, (byte*)(commBuf.RxBuf->buffer) + s3MessageLen, s3Msg);
	delete[]rcvBuf;
	return STATUS_SUCCEEDED;
}

//A function that sends a pin to the applet. the pin's length is 8 bytes, which is
//4 string digits.
// there are Unnecessary things here that needs to be deleted.
int SecureImage::sendAuthenticationId(byte *AuthenticationId, int Len, byte *encryptedId)
{
	// represents id the host send to applet
	byte* message = AuthenticationId;
	//rcvBuf represents the encrypted Id from applet to deliver to server
	char *rcvBuf = new char[16];
	//Send and Receive
	JHI_RET ret;
	JVM_COMM_BUFFER commBuf;
	//place inside sendBuff the buffer containing S2 message
	commBuf.TxBuf->buffer = message;
	commBuf.TxBuf->length = Len;
	commBuf.RxBuf->buffer = rcvBuf;
	commBuf.RxBuf->length = 16;
	INT32 responseCode;
	//perform call to the Trusted Application to give it the authentication code
	ret = JHI_SendAndRecv2(handle, session, CMD_SEND_AUTHENTICATION_ID, &commBuf, &responseCode);
	if (ret != STATUS_SUCCEEDED)
		return STATUS_FAILED;
	if (responseCode != STATUS_SUCCEEDED)
		return responseCode;
	copy((byte*)commBuf.RxBuf->buffer, (byte*)(commBuf.RxBuf->buffer) + 16, encryptedId);



	////instead of the code below, we need to send the authentication code to the server as well!    






	////copy the S3 message received from the trusted application to the client bytes array	
	//copy((byte*)commBuf.RxBuf->buffer, (byte*)(commBuf.RxBuf->buffer) + s3MessageLen, s3Msg);
	//delete[]rcvBuf;


	return STATUS_SUCCEEDED;
}


//private functions:

bool SecureImage::getGroupId(byte *groupId)
{
	//if initialization error occured - return failure
	if(!initialized)
		return false;

    char* message = "";

	//rcvBuf four bytes are INT32 representing the EPID Group ID for this platform
	char rcvBuf[GROUP_ID_LENGTH] = {0};

	JHI_RET ret;

	//Send and Receive
	JVM_COMM_BUFFER commBuf;
	commBuf.TxBuf->buffer = message;
	commBuf.TxBuf->length = strlen(message) + 1;
	commBuf.RxBuf->buffer = rcvBuf;
	commBuf.RxBuf->length = GROUP_ID_LENGTH;
	INT32 responseCode;
	//perform call to the Trusted Application to get the EPID Group ID for this platform
    ret = JHI_SendAndRecv2(handle, session, CMD_GET_GROUP_ID,  &commBuf, &responseCode);
    if (ret != 0 ||responseCode != 0)
	{
		return false;
	}
    //copy the group ID received from the trusted application to the client bytes array	
	copy((byte*) commBuf.RxBuf->buffer, (byte*) (commBuf.RxBuf->buffer) + GROUP_ID_LENGTH, groupId);
	return true;
}

bool SecureImage::callJHI(JVM_COMM_BUFFER* commBuf, int command, char* errorMsg)
{
	//if initialization error occured - return failure
	if(!initialized)
		return false;

	INT32 responseCode;
	byte error[ERROR_MESSAGE_LEN]={0};
	//in case there is no receiving buffer - send one to be filled with error
	if((*commBuf).RxBuf->buffer ==NULL)
	{	
		(*commBuf).RxBuf->buffer = error;
		(*commBuf).RxBuf->length = ERROR_MESSAGE_LEN;
	}
	//perform the call
	JHI_RET ret = JHI_SendAndRecv2(handle,session,command,commBuf,&responseCode);
	//JHI call has failed - fill the error
	if(ret != JHI_SUCCESS)
	{
		strcpy(errorMsg, "Failed to perform JHI call. Error: ");
		strcat(errorMsg,getJHIRet(ret)); 
		return false;
	}
	//JHI call succeeded but the TA return failure code - - fill the error
	else if( responseCode != 0)
	{
		if((*commBuf).RxBuf->length < ERROR_MESSAGE_LEN)
		{
			copy((byte*)(*commBuf).RxBuf->buffer,(byte*)((*commBuf).RxBuf->buffer) + (*commBuf).RxBuf->length,(byte*)errorMsg);
			errorMsg[(*commBuf).RxBuf->length]='\0';
		}
		return false;
	}	
	//Call was completed succesfully
	return true;
}

const char* SecureImage::getJHIRet(JHI_RET ret)
{
	static std::map<JHI_RET,const char*> codes;
	static bool mapInited=false;
	//check if map was initialized
	if(!mapInited)
	{	
		codes[JHI_FILE_MISSING_SRC]="JHI_FILE_MISSING_SRC";
		codes[JHI_FILE_ERROR_AUTH]="JHI_FILE_ERROR_AUTH";
		codes[JHI_FILE_ERROR_DELETE]="JHI_FILE_ERROR_DELETE";	
		codes[JHI_FILE_INVALID]="JHI_FILE_INVALID";
		codes[JHI_FILE_ERROR_OPEN]="JHI_FILE_ERROR_OPEN";
		codes[JHI_FILE_UUID_MISMATCH]="JHI_FILE_UUID_MISMATCH";
		codes[JHI_FILE_IDENTICAL]="JHI_FILE_IDENTICAL";
		codes[JHI_INVALID_COMMAND]="JHI_INVALID_COMMAND";
		codes[JHI_ILLEGAL_VALUE]="JHI_ILLEGAL_VALUE";
		codes[JHI_COMMS_ERROR]="JHI_COMMS_ERROR";	
		codes[JHI_SERVICE_INVALID_GUID]="JHI_SERVICE_INVALID_GUID";		
		codes[JHI_APPLET_TIMEOUT]="JHI_APPLET_TIMEOUT";
		codes[JHI_APPID_NOT_EXIST]="JHI_APPID_NOT_EXIST";	
		codes[JHI_JOM_FATAL]="JHI_JOM_FATAL";
		codes[JHI_JOM_OVERFLOW]="JHI_JOM_OVERFLOW";	
		codes[JHI_JOM_ERROR_DOWNLOAD]="JHI_JOM_ERROR_DOWNLOAD";	
		codes[JHI_JOM_ERROR_UNLOAD]="JHI_JOM_ERROR_UNLOAD";
		codes[JHI_ERROR_LOGGING]="JHI_ERROR_LOGGING";	
		codes[JHI_UNKNOWN_ERROR]="JHI_UNKNOWN_ERROR";
		codes[JHI_SUCCESS]="JHI_SUCCESS";			
		codes[JHI_INVALID_HANDLE]="JHI_INVALID_HANDLE";			
		codes[JHI_INVALID_PARAMS]="JHI_INVALID_PARAMS";				
		codes[JHI_SERVICE_UNAVAILABLE]="JHI_SERVICE_UNAVAILABLE";			
		codes[JHI_ERROR_REGISTRY]="JHI_ERROR_REGISTRY";				
		codes[JHI_ERROR_REPOSITORY_NOT_FOUND]="JHI_ERROR_REPOSITORY_NOT_FOUND";				
		codes[JHI_INTERNAL_ERROR]="JHI_INTERNAL_ERROR";			
		codes[JHI_INVALID_BUFFER_SIZE]="JHI_INVALID_BUFFER_SIZE";			
		codes[JHI_INVALID_COMM_BUFFER]="JHI_INVALID_COMM_BUFFER";			
		codes[JHI_INVALID_INSTALL_FILE]="JHI_INVALID_INSTALL_FILE";			
		codes[JHI_READ_FROM_FILE_FAILED]="JHI_READ_FROM_FILE_FAILED";												
		codes[JHI_INVALID_PACKAGE_FORMAT]="JHI_INVALID_PACKAGE_FORMAT";			
		codes[JHI_FILE_ERROR_COPY]="JHI_FILE_ERROR_COPY";				
		codes[JHI_INVALID_INIT_BUFFER]="JHI_INVALID_INIT_BUFFER";				
		codes[JHI_INVALID_FILE_EXTENSION]="JHI_INVALID_FILE_EXTENSION";			
		codes[JHI_INSTALL_FAILURE_SESSIONS_EXISTS]="JHI_INSTALL_FAILURE_SESSIONS_EXISTS";			
		codes[JHI_INSTALL_FAILED]="JHI_INSTALL_FAILED";			
		codes[JHI_UNINSTALL_FAILURE_SESSIONS_EXISTS]="JHI_UNINSTALL_FAILURE_SESSIONS_EXISTS";					
		codes[JHI_INCOMPATIBLE_API_VERSION]="JHI_INCOMPATIBLE_API_VERSION";		
		codes[JHI_MAX_SESSIONS_REACHED]="JHI_MAX_SESSIONS_REACHED";
		codes[JHI_SHARED_SESSION_NOT_SUPPORTED]="JHI_SHARED_SESSION_NOT_SUPPORTED";		
		codes[JHI_MAX_SHARED_SESSION_REACHED]="JHI_MAX_SHARED_SESSION_REACHED";		
		codes[JHI_INVALID_SESSION_HANDLE]="JHI_INVALID_SESSION_HANDLE";
		codes[JHI_INSUFFICIENT_BUFFER]="JHI_INSUFFICIENT_BUFFER";
		codes[JHI_APPLET_FATAL]="JHI_APPLET_FATAL";
		codes[JHI_SESSION_NOT_REGISTERED]="JHI_SESSION_NOT_REGISTERED";
		codes[JHI_SESSION_ALREADY_REGSITERED]="JHI_SESSION_ALREADY_REGSITERED";				
		codes[JHI_EVENTS_NOT_SUPPORTED]="JHI_EVENTS_NOT_SUPPORTED";						
		codes[JHI_APPLET_PROPERTY_NOT_SUPPORTED]="JHI_APPLET_PROPERTY_NOT_SUPPORTED";
		codes[JHI_SPOOLER_NOT_FOUND]="JHI_SPOOLER_NOT_FOUND";				
		codes[JHI_INVALID_SPOOLER]="JHI_INVALID_SPOOLER";
		//set initialization flag to true
		mapInited=true;
	}
	//return teh matching string to the received error code
	return codes[ret];
}

bool SecureImage::isProvisioned(char* errorMsg)
{
	//if initialization error occured - return failure
	if(!initialized)
		return false;
	char rcvBuf[ERROR_MESSAGE_LEN] = {0};
	//Send and Receive
	JVM_COMM_BUFFER commBuf;
	commBuf.TxBuf->buffer = NULL;
	commBuf.TxBuf->length = 0;
	commBuf.RxBuf->buffer = rcvBuf;
	commBuf.RxBuf->length = ERROR_MESSAGE_LEN;
	INT32 responseCode;
	//perform a call to the Trusted Application in order to check whether the EPID 1.1 provisioning process was already done on this platform
	bool res = callJHI(&commBuf,CMD_IS_PROVISIONED,errorMsg);
	if(res)
	{
		//first byte indiactes whether platform is provisioned or not
		byte *resp = (byte*) commBuf.RxBuf->buffer;
		if(resp[0]==PROVISIONED)
			return true;
		else
		{
			strcpy(errorMsg,"Platform is not EPID provisioned.");
		}
	}
	return false;
}

//int secureimage::parsemetadata(byte *rawdata)
//{
//	//extract the size of the encrypted private key, mod and exponent
//	mod_size = (short)((rawdata[0] << 8) | rawdata[1]); // should be 256
//	signed_mod_size = (short)((rawdata[2] << 8) | rawdata[3]); 
//	e_size = (short)((rawdata[4] << 8) | rawdata[5]); // should be 4
//	signed_e_size = (short)((rawdata[6] << 8) | rawdata[7]); 
//	d_size = (short)((rawdata[8] << 8) | rawdata[9]); // should be 256
//
//	//allocate buffers for encrypted private key, mod and exponent
//	signed_mod = new byte[signed_mod_size];
//	signed_e = new byte[signed_e_size];
//	mod = new byte[mod_size];
//	e = new byte[e_size];
//	d = new byte[d_size];
//
//	//init the data members of encrypted private key, mod and exponent
//	int currind=10;
//	memcpy(mod,rawdata+currind,mod_size);
//	currind+=mod_size;
//	memcpy(signed_mod,rawdata+currind,signed_mod_size);
//	currind+=signed_mod_size;
//	memcpy(e,rawdata+currind,e_size);
//	currind+=e_size;
//	memcpy(signed_e,rawdata+currind,signed_e_size);
//	currind+=signed_e_size;
//	memcpy(d,rawdata+currind,d_size);
//	currind+=d_size;
//	return currind;
//}


SecureImage::~SecureImage(void)
{
	
}



// WYS funcs

/* Display an encrypted image using WYS */
WYSRESULT SecureImage::doWysSequence(HWND windowHandle, unsigned char wysImageType)
{
	WYSRESULT wysRet = WYS_S_SUCCESS;

	wysWindowProps.windowBackgroundColor = COLOR_WHITE;
	wysWindowProps.WindowHandle = windowHandle;

	wysWindowProps.taInstanceId = session;

	//if there is an existing PAVP session - close it
	if (sessionExists)
	{
		ReleaseImageX();
		closePavpSession();
		sessionExists = false;
	}

	//initialize pavp with the window handle
	if (handler.EstablishSession(windowHandle, wysWindowProps.windowBackgroundColor))
	{
		sessionExists = true;
		pavpSessionHandle = handler.GetPAVPSlotHandle();
		//display the image 
		return doWysDisplay(&pavpSessionHandle, wysImageType);
	}
	return wysRet;
}



/* Reports a mouse-down event on the WYS image. */
WYSRESULT SecureImage::onMouseDown(HWND windowHandle, UINT16 x, UINT16 y)
{
	XY_PAIR_BYTES mouseDownXY;
	WYSRESULT retExt = WYS_E_INVALID_MOUSE_INPUT;
	WysImage *pWysImageObj;

	mouseDownXY.xw.value = x;
	mouseDownXY.yh.value = y;
	if (!sessionExists)
		retExt = WYS_E_INVALID_SESSION;
	else
	{
		if (!handler.IsPointInRect(&mouseDownXY))
			return retExt;
		pWysImageObj = getWysImageObject(imageId);
		if (pWysImageObj == NULL)
		{
			retExt = WYS_E_INVALID_IMAGE_ID;
		}
		else if (pWysImageObj->m_bValid)
		{
			if (pWysImageObj->m_wysImageType != WysStandardImageCaptcha)
				do
				{
					if (pWysImageObj->m_clickRecords.size() > pWysImageObj->m_WysImgConfig.maxAllowedClicks)
					{
						retExt = WYS_E_CLICK_INPUT_COUNT_MAX;
						break;
					}

					if ((mouseDownXY.xw.value >= (pWysImageObj->m_wysImgRenderRect.left)) &&
						(mouseDownXY.xw.value <= (pWysImageObj->m_wysImgRenderRect.right)) &&
						(mouseDownXY.yh.value >= (pWysImageObj->m_wysImgRenderRect.top)) &&
						(mouseDownXY.yh.value <= (pWysImageObj->m_wysImgRenderRect.bottom)))
					{
						if (pWysImageObj->m_nMetadataObjs == 0)
						{
							pWysImageObj->m_WysButtonDown = mouseDownXY;
							pWysImageObj->m_nWysButtonDown = 1;
							retExt = WYS_E_VALID_MOUSE_INPUT;
							break;
						}
					}
					else
					{
						break;
					}
					pWysImageObj->m_nWysButtonDown = WYS_INVALID_WYS_BUTTON;
					for (int i = 0; i < pWysImageObj->m_nMetadataObjs; i++)
					{
						if ((mouseDownXY.xw.value >= (pWysImageObj->m_wysImgRenderRect.left + pWysImageObj->m_pMetadataObjs[i].objBounds.x)) &&
							(mouseDownXY.xw.value <= (pWysImageObj->m_wysImgRenderRect.left + pWysImageObj->m_pMetadataObjs[i].objBounds.x + pWysImageObj->m_pMetadataObjs[i].objBounds.w)) &&
							(mouseDownXY.yh.value >= (pWysImageObj->m_wysImgRenderRect.top + pWysImageObj->m_pMetadataObjs[i].objBounds.y)) &&
							(mouseDownXY.yh.value <= (pWysImageObj->m_wysImgRenderRect.top + pWysImageObj->m_pMetadataObjs[i].objBounds.y + pWysImageObj->m_pMetadataObjs[i].objBounds.h)))
						{

							pWysImageObj->m_nWysButtonDown = i; // remember index of button over which mouse button was down

							if (pWysImageObj->m_wysObjActionRenderType == StandardWys || pWysImageObj->m_wysObjActionRenderType == CustomAlphaImage)
							{
								drawClickEffect(windowHandle, pWysImageObj->m_wysImgRenderRect.left + pWysImageObj->m_pMetadataObjs[i].objBounds.x, pWysImageObj->m_wysImgRenderRect.top + pWysImageObj->m_pMetadataObjs[i].objBounds.y,
									pWysImageObj->m_pMetadataObjs[i].objBounds.w, pWysImageObj->m_pMetadataObjs[i].objBounds.h, pWysImageObj->m_actionObjColor);
							}
							retExt = WYS_E_VALID_MOUSE_INPUT;
							break;
						}
					}
				} while (false);
		}
		else
		{
			retExt = WYS_E_INVALID_IMAGE;
		}
	}
	return retExt;
}

/* Reports a mouse-up event on the WYS image. */
WYSRESULT SecureImage::onMouseUp(UINT16 x, UINT16 y)
{
	WYSRESULT retExt = WYS_E_INVALID_MOUSE_INPUT;
	bool bClickWithinImageBounds = false;

	XY_PAIR_BYTES mouseDownXY;
	mouseDownXY.xw.value = x;
	mouseDownXY.yh.value = y;

	if (!sessionExists)
		retExt = WYS_E_INVALID_SESSION;
	else
	{
		WysImage *pWysImageObj = getWysImageObject(imageId);
		if (pWysImageObj == NULL)
		{
			retExt = WYS_E_INVALID_IMAGE_ID;
		}
		else if (pWysImageObj->m_bValid)
		{
			if (pWysImageObj->m_wysImageType != WysStandardImageCaptcha)
				do
				{

					if (pWysImageObj->m_clickRecords.size() > pWysImageObj->m_WysImgConfig.maxAllowedClicks)
					{
						retExt = WYS_E_CLICK_INPUT_COUNT_MAX;
						break;
					}
					if (pWysImageObj->m_nWysButtonDown == WYS_INVALID_WYS_BUTTON)
					{
						break;
					}
					// at this point we have a valid mouse down
					if ((mouseDownXY.xw.value >= (pWysImageObj->m_wysImgRenderRect.left)) &&
						(mouseDownXY.xw.value <= (pWysImageObj->m_wysImgRenderRect.right)) &&
						(mouseDownXY.yh.value >= (pWysImageObj->m_wysImgRenderRect.top)) &&
						(mouseDownXY.yh.value <= (pWysImageObj->m_wysImgRenderRect.bottom)))
					{
						bClickWithinImageBounds = true;
						if (pWysImageObj->m_nMetadataObjs == 0)
						{
							pWysImageObj->m_WysButtonClick = mouseDownXY;
							if ((pWysImageObj->m_WysButtonClick.xw.value == pWysImageObj->m_WysButtonDown.xw.value) && (pWysImageObj->m_WysButtonClick.yh.value == pWysImageObj->m_WysButtonDown.yh.value))
							{ // down(x,y) == up(x,y) ?
								pWysImageObj->m_WysButtonClick.xw.value = (mouseDownXY.xw.value - (UINT16)pWysImageObj->m_wysImgRenderRect.left);
								pWysImageObj->m_WysButtonClick.yh.value = (mouseDownXY.yh.value - (UINT16)pWysImageObj->m_wysImgRenderRect.top);
								// keep storing the valid mouse up/down coordinates

								pWysImageObj->m_clickRecords.push_back(pWysImageObj->m_WysButtonClick.xwyh);
								++(pWysImageObj->m_stats.totalClicks);
								retExt = WYS_E_VALID_MOUSE_INPUT;
							}
							break;
						}// else m_nMetadataObjs > 0 and click is within wys image bounds
					} // else m_nMetadataObjs > 0 and click is outside wys image bounds retExt will be WYS_E_INVALID_MOUSE_INPUT
					  // at this point we have a valid mouse button down so erase the click effect first before we next step

					int i;
					eraseClickEffect(pWysImageObj);
					//			m_pCurrWysImage = NULL;

					if (retExt == WYS_E_INVALID_MOUSE_INPUT && !bClickWithinImageBounds)
					{ // mouse up is outside wys image bounds ?
						break;
					}
					for (i = 0; i < pWysImageObj->m_nMetadataObjs; i++)// check for a valid mouse up
					{
						if ((mouseDownXY.xw.value >= (pWysImageObj->m_wysImgRenderRect.left + pWysImageObj->m_pMetadataObjs[i].objBounds.x)) &&
							(mouseDownXY.xw.value <= (pWysImageObj->m_wysImgRenderRect.left + pWysImageObj->m_pMetadataObjs[i].objBounds.x + pWysImageObj->m_pMetadataObjs[i].objBounds.w)) &&
							(mouseDownXY.yh.value >= (pWysImageObj->m_wysImgRenderRect.top + pWysImageObj->m_pMetadataObjs[i].objBounds.y)) &&
							(mouseDownXY.yh.value <= (pWysImageObj->m_wysImgRenderRect.top + pWysImageObj->m_pMetadataObjs[i].objBounds.y + pWysImageObj->m_pMetadataObjs[i].objBounds.h))) {

							if (pWysImageObj->m_nWysButtonDown == i)
							{
								pWysImageObj->m_WysButtonClick.xw.value = (mouseDownXY.xw.value - (UINT16)pWysImageObj->m_wysImgRenderRect.left);
								pWysImageObj->m_WysButtonClick.yh.value = (mouseDownXY.yh.value - (UINT16)pWysImageObj->m_wysImgRenderRect.top);
								pWysImageObj->m_clickRecords.push_back(pWysImageObj->m_WysButtonClick.xwyh);
								pWysImageObj->m_elementIDs.push_back(i);
								++(pWysImageObj->m_stats.totalClicks);
								if (pWysImageObj->m_pMetaClicked[i] == 0)
								{
									pWysImageObj->m_bUniqueMetaClick = true;
								}
								else
								{
									pWysImageObj->m_bUniqueMetaClick = false;
								}
								++(pWysImageObj->m_pMetaClicked[i]);
								retExt = WYS_E_VALID_MOUSE_INPUT;
							}
							break;
						}
					}

					pWysImageObj->m_nWysButtonDown = WYS_INVALID_WYS_BUTTON;

				} while (false);
		}
		else
		{
			retExt = WYS_E_INVALID_IMAGE;
		}
	}
	return retExt;
}


/* Sends user input to the applet, and gets applet's response. */
UINT32 SecureImage::onClickSubmit(wchar_t* userInput, UINT16 inputLength)
{
	UINT32 wysRet = WYS_E_FAILURE;

	if (!sessionExists)
	{
		wysRet = WYS_E_INVALID_SESSION;
	}
	else
	{
		WysImage *pWysImageObj = getWysImageObject(imageId);
		if (pWysImageObj == NULL)
		{
			wysRet = WYS_E_INVALID_IMAGE_ID;
		}
		else if (pWysImageObj->m_bValid)
		{
			do
			{
				if (pWysImageObj->m_wysImageType == WysStandardImagePinPad || pWysImageObj->m_wysImageType == WysStandardImageOK)
				{
					wysRet = submitClickInput(pWysImageObj->m_wysImageType, &pWysImageObj->m_clickRecords);
				}
				else if (pWysImageObj->m_wysImageType == WysStandardImageCaptcha)
				{
					if (inputLength > WYS_MAX_CAPTCHA_INPUT_LENGTH)
					{
						wysRet = WYS_E_CAPTCHA_INPUT_LENGTH_MAX;
						break;
					}
					wysRet = submitCaptchaString(inputLength, userInput);
				}
				else
				{
					wysRet = WYS_E_INVALID_WYSSTDIMAGE_TYPE;
					break;
				}
				if (wysRet == WYS_S_SUCCESS)
				{
					pWysImageObj->m_bValid = false;
					pWysImageObj->ClearClickRecords();
				}
			} while (false);
		}
		else
		{
			wysRet = WYS_E_INVALID_IMAGE;
		}
	}
	return wysRet;
}

/* Clears user input. */
WYSRESULT SecureImage::onClickClear()
{
	WYSRESULT retExt = WYS_S_SUCCESS;
	if (!sessionExists)
		retExt = WYS_E_INVALID_SESSION;
	else
	{
		WysImage *pWysImageObj;
		if (imageId == WYS_IMAGE_ID_ALL)
		{
			if (!m_wysImageObjs.empty())
			{
				WYSIMAGEOBJS_HASH_MAP::iterator it;
				// iterate through all WysImage objs and delete them one by one
				for (it = m_wysImageObjs.begin(); it != m_wysImageObjs.end(); ++it)
				{
					pWysImageObj = it->second;
					if (pWysImageObj->m_bValid)
						pWysImageObj->ClearClickRecords();
					else
						retExt = WYS_E_INVALID_IMAGE;
				}
			}
		}
		else
		{
			pWysImageObj = getWysImageObject(imageId);
			if (pWysImageObj == NULL)
				retExt = WYS_E_INVALID_IMAGE_ID;
			else if (pWysImageObj->m_bValid)
				pWysImageObj->ClearClickRecords();
			else
				retExt = WYS_E_INVALID_IMAGE;
		}
	}
	return retExt;
}

/*if user input is OK, Create one time password*/
bool SecureImage::getOtp(void* outArr, int arrLength)
{
	JHI_RET ret;
	JVM_COMM_BUFFER commBuff;
	char rBuff[2048];
	commBuff.RxBuf->buffer = rBuff;
	commBuff.RxBuf->length = 2048;
	commBuff.TxBuf->length = 0;

	INT32 resp = 0;
	if (handle != NULL && session != NULL)
	{
		ret = JHI_SendAndRecv2(handle, session, COMMAND_ID_GET_OTP, &commBuff, &resp);
		if (ret == JHI_SUCCESS)
		{
			if (commBuff.RxBuf->length != arrLength)
			{
				return false;
			}
			memcpy(outArr, commBuff.RxBuf->buffer, commBuff.RxBuf->length);
		}
	}
	else resp = -1;

	return resp == RESP_CODE_APPLET_SUCCESS;
}


/*Displays the image using PAVP session handle*/
WYSRESULT SecureImage::doWysDisplay(UINT32* pavpSessionHandle, unsigned char wysImageType)
{
	WYSRESULT res;

	XY_PAIR_BYTES winPos, logoSize;
	COLORREF winColor, buttonColor, fontColor, frameColor, buttonBorderColor, frameBorderColor;
	UINT32 captchaFont = 0;
	UINT8 *pLogoSignature = NULL;
	UINT32 nLogoSignatureLen = 0;
	CImage imgLogo;
	UINT32 logoImageLen;

	CREATE_WYSIMAGE *wysImage;

	winPos.xw.value = (UINT16)0;
	winPos.yh.value = (UINT16)0;
	winColor = COLOR_WHITE;
	buttonColor = COLOR_GREEN;
	buttonBorderColor = COLOR_BLACK;
	fontColor = COLOR_WHITE;
	frameColor = COLOR_BLUE;
	frameBorderColor = COLOR_GREEN;

	//For this example, no logo is used.
	logoImageLen = logoSize.xwyh = 0;

	UINT32 wysImgLen = sizeof(CREATE_WYSIMAGE) + logoImageLen;
	UINT32 wysImageId = WYS_USER_IMAGE_ID_RANGE_START + 1;

	wysImage = (CREATE_WYSIMAGE *)new UINT8[wysImgLen];

	if (wysImage != NULL)
	{
		wysImage->wysImageType = wysImageType;
		wysImage->appParam.value = wysImageId;
		wysImage->wysImageSize.xw.value = (UINT16)256;
		wysImage->wysImageSize.yh.value = (UINT16)256;
		wysImage->buttonSize.xw.value = (UINT16)20;
		wysImage->buttonSize.yh.value = (UINT16)20;
		wysImage->fontType = 0;
		wysImage->textLength = 9;
		wysImage->buttonBorderWidth = 2;
		wysImage->frameBorderWidth = 3;
		wysImage->colors.buttonColor.value = buttonColor;
		wysImage->colors.fontColor.value = fontColor;
		wysImage->colors.wysImageColor.value = winColor;
		wysImage->colors.frameColor.value = frameColor;
		wysImage->colors.buttonBorderColor.value = buttonBorderColor;
		wysImage->colors.frameBorderColor.value = frameBorderColor;
		wysImage->logoSize = logoSize;

		res = createStdImage(*pavpSessionHandle, &winPos, wysImage, wysImgLen, NULL, 0);
		if (res == WYS_S_SUCCESS)
		{
			imageId = wysImage->appParam.value;
			res = repaintImage(pavpSessionHandle, imageId);
		}
	}
	else
	{
		res = WYS_E_MEMORY;
	}

	delete wysImage;
	wysImage = NULL;
	return res;
}

/* Paints the image */
WYSRESULT SecureImage::repaintImage(UINT32* pavpSessionHandle, UINT32 wysImageId)
{
	WYSRESULT retExt = WYS_S_SUCCESS;

	if (!sessionExists)
	{
		retExt = WYS_E_INVALID_SESSION;
	}
	else
	{
		WysImage *pWysImageObj;
		if (wysImageId == WYS_IMAGE_ID_ALL)
		{
			if (!m_wysImageObjs.empty())
			{
				WYSIMAGEOBJS_HASH_MAP::iterator it;
				bool bFirst = true;
				for (it = m_wysImageObjs.begin(); it != m_wysImageObjs.end(); ++it)
				{
					pWysImageObj = it->second;
					if (pWysImageObj != NULL) {
						if (!handler.DisplayVideo(pWysImageObj->m_pDecoderRenderTargets, &pWysImageObj->m_wysImgRenderRect, bFirst))
						{
							retExt = WYS_E_CANNOT_DISPLAY_IMAGE;
						}
						bFirst = false;
					}
				}
			}
			else
			{
				handler.RefreshBackground(wysWindowProps.WindowHandle, NULL, wysWindowProps.windowBackgroundColor);
				retExt = WYS_E_INVALID_IMAGE_ID;
			}
		}
		else
		{
			pWysImageObj = getWysImageObject(wysImageId);
			if (pWysImageObj == NULL)
			{
				retExt = WYS_E_INVALID_IMAGE_ID;
			}
			else
			{                                          //contains value?
				if (handler.DisplayVideo(pWysImageObj->m_pDecoderRenderTargets, &pWysImageObj->m_wysImgRenderRect, true))
				{
					retExt = WYS_E_CANNOT_DISPLAY_IMAGE;
				}
			}
		}
	}
	return retExt;
}

bool SecureImage::drawClickEffect(HWND windowHandle, int btnL, int btnT, int btnW, int btnH, DWORD color)
{
	bool bRet = false;
	HDC hdc = NULL;

	if ((hdc = ::GetDC(windowHandle)) != NULL) {
		do {
			HPEN hPen, hPenOrig, hPenOld;
			int btnR = btnL + btnW;
			int btnB = btnT + btnH;
			hPen = ::CreatePen(PS_SOLID, 1, RGB(0x80, 0x80, 0x80)); // GRAY
			if (hPen == NULL) {
				break;
			}
			hPenOrig = (HPEN)::SelectObject(hdc, hPen);
			if (hPenOrig == NULL) {
				break;
			}

			::MoveToEx(hdc, btnL + LOFF, btnT + LOFF, NULL);
			::LineTo(hdc, btnR - LOFF, btnT + LOFF);
			::LineTo(hdc, btnR - LOFF, btnB - LOFF);
			::LineTo(hdc, btnL + LOFF, btnB - LOFF);
			::LineTo(hdc, btnL + LOFF, btnT + LOFF);

			hPen = ::CreatePen(PS_SOLID, 1, color); // border color
			if (hPen == NULL) {
				break;
			}
			hPenOld = (HPEN)::SelectObject(hdc, hPen);
			if (hPenOld == NULL) {
				break;
			}
			::DeleteObject(hPenOld);
			::MoveToEx(hdc, btnL + LOFF2, btnT + LOFF2, NULL);
			::LineTo(hdc, btnR - LOFF2, btnT + LOFF2);
			::LineTo(hdc, btnR - LOFF2, btnB - LOFF2);
			::LineTo(hdc, btnL + LOFF2, btnB - LOFF2);
			::LineTo(hdc, btnL + LOFF2, btnT + LOFF2);

			hPen = ::CreatePen(PS_SOLID, 1, RGB(0xA0, 0xA0, 0xA0)); // LITE GRAY
			if (hPen == NULL) {
				break;
			}
			hPenOld = (HPEN)::SelectObject(hdc, hPen);
			if (hPenOld == NULL) {
				break;
			}
			::DeleteObject(hPenOld);
			::MoveToEx(hdc, btnL + LOFF3, btnT + LOFF3, NULL);
			::LineTo(hdc, btnR - LOFF3, btnT + LOFF3);
			::LineTo(hdc, btnR - LOFF3, btnB - LOFF3);
			::LineTo(hdc, btnL + LOFF3, btnB - LOFF3);
			::LineTo(hdc, btnL + LOFF3, btnT + LOFF3);

			hPenOld = (HPEN)::SelectObject(hdc, hPenOrig);
			if (hPenOld == NULL) {
				break;
			}
			::DeleteObject(hPenOld);

			bRet = true;

		} while (false);

		::ReleaseDC(windowHandle, hdc);
	}
	else {

	}

	return bRet;
}

/* Creates the image to be display*/
WYSRESULT SecureImage::createStdImage(IN UINT32 pavpSessionHandle, IN XY_PAIR_BYTES *pwysImagePosition, IN CREATE_WYSIMAGE *pWysImageCreateParams,
	IN UINT32 wysImageCreateParamsLen, IN void *rsvdparam1, IN UINT32 rsvparam2)
{
	WYSRESULT retExt = WYS_E_INVALID_SESSION_HANDLE;

	do {
		if (pwysImagePosition == NULL || pWysImageCreateParams == NULL)
		{
			retExt = WYS_E_INVALID_PARAMS;
		}
		else if (wysImageCreateParamsLen != ((sizeof(CREATE_WYSIMAGE) + (pWysImageCreateParams->logoSize.xw.value * pWysImageCreateParams->logoSize.yh.value * WYS_LOGO_BYTESPERPIXEL))))
		{
			retExt = WYS_E_INVALID_PARAMS;
		}
		else if (wysImageCreateParamsLen > WYS_MAX_CREATE_WYSIMAGE_SIZE)
		{
			retExt = WYS_E_LOGO_IMAGE_SIZE_MAX;
		}
		else
		{
			// do some bounds checking
			unsigned int wysImageId = pWysImageCreateParams->appParam.value;
			if (wysImageId < WYS_USER_IMAGE_ID_RANGE_START || wysImageId > WYS_USER_IMAGE_ID_RANGE_END)
			{
				retExt = WYS_E_INVALID_IMAGE_ID; break;
			}
			unsigned int totalButtonH = pWysImageCreateParams->buttonBorderWidth * 2 + pWysImageCreateParams->buttonSize.yh.value;
			unsigned int totalButtonW = pWysImageCreateParams->buttonBorderWidth * 2 + pWysImageCreateParams->buttonSize.xw.value;
			unsigned int totalframeBorderW = pWysImageCreateParams->frameBorderWidth * 2;

			if (pWysImageCreateParams->wysImageType == WysStandardImagePinPad)
			{
				if (((totalframeBorderW + pWysImageCreateParams->logoSize.xw.value + (totalButtonW * WYS_LOCAL_NUM_METADATA_OBJS_X)) > pWysImageCreateParams->wysImageSize.xw.value) ||
					((totalframeBorderW + pWysImageCreateParams->logoSize.yh.value + (totalButtonH * WYS_LOCAL_NUM_METADATA_OBJS_Y)) > pWysImageCreateParams->wysImageSize.yh.value))
				{
					retExt = WYS_E_INVALID_INPUT; break;
				}
			}
			else if (pWysImageCreateParams->wysImageType == WysStandardImageOK)
			{
				if (((totalframeBorderW + pWysImageCreateParams->logoSize.xw.value + totalButtonW) > pWysImageCreateParams->wysImageSize.xw.value) ||
					((totalframeBorderW + pWysImageCreateParams->logoSize.yh.value + totalButtonH) > pWysImageCreateParams->wysImageSize.yh.value))
				{
					retExt = WYS_E_INVALID_INPUT; break;
				}
			}
			else if (pWysImageCreateParams->wysImageType == WysStandardImageCaptcha)
			{
				if (((totalframeBorderW + pWysImageCreateParams->logoSize.xw.value) > pWysImageCreateParams->wysImageSize.xw.value) ||
					((totalframeBorderW + pWysImageCreateParams->logoSize.yh.value) > pWysImageCreateParams->wysImageSize.yh.value)
					)
				{
					retExt = WYS_E_INVALID_INPUT; break;
				}
			}
			else
			{
				retExt = WYS_E_INVALID_WYSSTDIMAGE_TYPE; break;
			}

			retExt = displayImage(pwysImagePosition, pWysImageCreateParams);
		}
	} while (false);

	return retExt;
}

WYSRESULT SecureImage::displayImage(XY_PAIR_BYTES *pWysImagePosition, CREATE_WYSIMAGE *pWysImage)
{
	WYSRESULT retExt = WYS_S_SUCCESS;
	do {
		UINT32 wysImageId = pWysImage->appParam.value;
		// is wysImageId already in use ?
		if (getWysImageObject(wysImageId) != NULL) {
			retExt = WYS_E_IMAGE_ID_IN_USE;
			break;
		}

		RECT wysWinClientRect;
		// check if wys image fits inside the window client region
		if (::GetClientRect(wysWindowProps.WindowHandle, &wysWinClientRect)) {
			if (((pWysImagePosition->xw.value + pWysImage->wysImageSize.xw.value) > wysWinClientRect.right) ||
				((pWysImagePosition->yh.value + pWysImage->wysImageSize.yh.value) > wysWinClientRect.bottom)
				) {
				retExt = WYS_E_IMAGE_WINDOW_BOUNDS;
				break;
			}
		}
		else {
			retExt = WYS_E_WINAPI_FAILURE;
			break;
		}

		pWysImage->reserved1.value = handler.GetPAVPSlotHandle();

		CREATE_WINDOW_RESPONSE_MSG *windowResp;
		UINT32 windowRespSize;
		METADATA_OBJECT *pMetadataObjs;
		int nMetadataObjs;

		windowRespSize = sizeof(CREATE_WINDOW_RESPONSE_MSG) + (pWysImage->wysImageType == WysStandardImagePinPad ? WYS_LOCAL_NUM_METADATA_OBJS * sizeof(WIDGET_MAPPING_BYTES) : 0);
		windowResp = (CREATE_WINDOW_RESPONSE_MSG *) new UINT8[windowRespSize];
		if (windowResp != NULL)
		{
			memset(windowResp, 0, windowRespSize);
			if ((retExt = sendWYSStdWindowRequest(pWysImage, windowResp, &windowRespSize)) != WYS_S_SUCCESS)
			{

			}
			else if (windowResp->imageSize.value > 0)
			{
				void *pDecoderRenderTargets;
				RECT wysImgRect;

				wysImgRect.left = pWysImagePosition->xw.value;
				wysImgRect.top = pWysImagePosition->yh.value;
				wysImgRect.right = pWysImage->wysImageSize.xw.value;
				wysImgRect.bottom = pWysImage->wysImageSize.yh.value;

				if ((retExt = getLocalWysImageFromFWAndDisplay(windowResp, &wysImgRect, &pDecoderRenderTargets)) == WYS_S_SUCCESS)
				{
					if (pWysImage->wysImageType == WysStandardImagePinPad)
					{
						nMetadataObjs = WYS_LOCAL_NUM_METADATA_OBJS;
						pMetadataObjs = new METADATA_OBJECT[nMetadataObjs];
						if (pMetadataObjs != NULL)
						{
							for (int i = 0; i < nMetadataObjs; i++)
							{
								pMetadataObjs[i].objBounds.x = Wysutil.bytesToShort(windowResp->map[i].location.xw);
								pMetadataObjs[i].objBounds.y = Wysutil.bytesToShort(windowResp->map[i].location.yh);
								pMetadataObjs[i].objBounds.w = Wysutil.bytesToShort(windowResp->map[i].size.xw);
								pMetadataObjs[i].objBounds.h = Wysutil.bytesToShort(windowResp->map[i].size.yh);

							}
							WysImage *pWysImageObj = new WysImage(wysImageId, wysImgRect, pDecoderRenderTargets, &gWysConfig.globalConfig, (WysImageType)pWysImage->wysImageType, StandardWys,
								pWysImage->colors.buttonBorderColor.value);
							if (pWysImageObj != NULL)
							{
								if (pWysImageObj->init(nMetadataObjs, pMetadataObjs))
								{
									m_wysImageObjs[wysImageId] = pWysImageObj;
								}
								else
								{
									delete pWysImageObj;
									pWysImageObj = NULL;
									retExt = WYS_E_MEMORY;
								}
							}
							else
							{
								retExt = WYS_E_MEMORY;
							}
							delete[] pMetadataObjs;
							pMetadataObjs = NULL;
						}
						else
						{
							retExt = WYS_E_MEMORY;
						}
					}
					else // ok or captcha
					{
						WysImage *pWysImageObj = new WysImage(wysImageId, wysImgRect, pDecoderRenderTargets, &gWysConfig.globalConfig, (WysImageType)pWysImage->wysImageType);
						if (pWysImageObj != NULL)
						{
							if (pWysImageObj->init(0, NULL))
							{
								m_wysImageObjs[wysImageId] = pWysImageObj;
							}
							else
							{
								delete pWysImageObj;
								pWysImageObj = NULL;
								retExt = WYS_E_MEMORY;
							}
						}
						else
						{
							retExt = WYS_E_MEMORY;
						}
					}
				}
			}
			else
			{
				retExt = WYS_E_INVALID_INPUT;
			}
			delete[] windowResp;
			windowResp = NULL;
		}
		else
		{
			retExt = WYS_E_MEMORY;
		}
	} while (false);

	return retExt;
}

/* Gets the encrypted image from the applet and displays it by the GFX.*/
WYSRESULT SecureImage::getLocalWysImageFromFWAndDisplay(CREATE_WINDOW_RESPONSE_MSG *windowResp, RECT *pWysImageRect, void **pDecoderRenderTargets)
{
	WYSRESULT retExt = WYS_S_SUCCESS;
	UINT32 FinalImageSize;
	UINT8 *pImageBuff;
	UINT32 wysImageHandle;

	do
	{
		wysImageHandle = Wysutil.bytesToInt(windowResp->jhiHandle);
		FinalImageSize = Wysutil.bytesToInt(windowResp->imageSize);
		pImageBuff = (UINT8 *)_aligned_malloc(FinalImageSize, sizeof(void *));
		if (pImageBuff == NULL)
		{
			retExt = WYS_E_MEMORY;
			break;
		}
		if ((retExt = getImageFromFW(wysImageHandle, FinalImageSize, pImageBuff)) == WYS_S_SUCCESS)
		{
			HRESULT hr;
			hr = handler.SetNewKey((char *)&windowResp->S1Kb[0]);
			SecureZeroMemory(&windowResp->S1Kb[0], sizeof(windowResp->S1Kb));
			if (FAILED(hr))
			{
				retExt = WYS_E_FAILURE;
				break;
			}
			hr = handler.DoDecryptionBlt(pDecoderRenderTargets, pImageBuff, FinalImageSize, windowResp->CryptoCounter, pWysImageRect, WYS_SRCIMAGE_MEMORY, true);

			if (FAILED(hr))
			{
				retExt = WYS_E_CANNOT_DISPLAY_IMAGE;
			}
		} // else release allocated resources and return
		_aligned_free(pImageBuff);
	} while (false);

	return retExt;
}

/* Gets the encrypted image from the applet.*/
WYSRESULT SecureImage::getImageFromFW(UINT32 meHandle, UINT32 ImageSize, UINT8 *pImageBuff)
{
	WYSRESULT retExt = WYS_S_SUCCESS;
	void *pReq = NULL;
	UINT8 *pResp = NULL;
	void *inData = NULL;
	GET_IMAGE_CHUNK_MSG *ImgMsg = NULL;
	UINT16 respBufLen;
	UINT16 reqBufLen;
	UINT32 dataLength;
	UINT32 NumOfChunks;
	UINT32 LastChunkSize;
	UINT32 ChunkIdx;
	UINT32 ChunkSize;

	do {
		NumOfChunks = ImageSize / READ_IMAGE_CHUNK_SIZE;
		LastChunkSize = ImageSize % READ_IMAGE_CHUNK_SIZE;
		if (LastChunkSize > 0)
		{
			NumOfChunks += 1;
		}
		else
		{
			LastChunkSize = READ_IMAGE_CHUNK_SIZE;
		}

		ChunkSize = READ_IMAGE_CHUNK_SIZE;
		dataLength = sizeof(GET_IMAGE_CHUNK_MSG);
		reqBufLen = dataLength;
		pReq = new UINT8[reqBufLen];
		// Allocate buffer for request and keep reusing it for every Chunk request
		if (pReq == NULL)
		{
			retExt = WYS_E_MEMORY;
			break;
		}
		inData = pReq;
		ImgMsg = (GET_IMAGE_CHUNK_MSG *)inData;
		ImgMsg->subCommand = SUB_COMM_GET_IMAGE_CHUNK;
		WysUtil::intToBytes(ChunkSize, &ImgMsg->size);
		WysUtil::intToBytes(meHandle, &ImgMsg->handle);


		// Allocate buffer for response and keep reusing it for every Chunk response
		pResp = pImageBuff;

		for (ChunkIdx = 0; ChunkIdx < NumOfChunks; ChunkIdx++)
		{
			if ((ChunkIdx + 1) == NumOfChunks)
			{ // need to do some specific initializations for the last chunk
			  // assuming last chunk size is != READ_IMAGE_CHUNK_SIZE
				ChunkSize = LastChunkSize;
				WysUtil::intToBytes(ChunkSize, &ImgMsg->size);
			}
			dataLength = ChunkSize;
			respBufLen = dataLength;

			// Get a Chunk
			if ((retExt = jhiSendRecv(reqBufLen, pReq, respBufLen, pResp, &dataLength, (UINT8 *)pImageBuff)) == WYS_S_SUCCESS)
			{
				pImageBuff += ChunkSize;
				pResp += ChunkSize;
			}
			else
			{
				retExt = WYS_E_FAILURE;
				break;
			}
		}
	} while (false);

	if (pReq != NULL)
	{
		delete[] pReq;
		pReq = NULL;
	}

	return retExt;
}

WYSRESULT SecureImage::jhiSendRecv(UINT32 reqBufLen, void *pSendReq, UINT32 recvBufLen,
	void *pRecvBuf, UINT32 *pRespDataLen, UINT8 *pRespData)
{
	WYSRESULT					retExt = WYS_E_INSUFFICIENT_BUFFER_SIZE;
	int							ret;
	APPLETSENDRECVDATAPARAMS	jhisrdp;

	jhisrdp.TxBuf->length = reqBufLen;
	jhisrdp.TxBuf->buffer = pSendReq;
	jhisrdp.RxBuf->length = recvBufLen;
	jhisrdp.RxBuf->buffer = pRecvBuf;

	if (handle != NULL && session != NULL)
	{
		ret = JHI_SendAndRecv2(handle, session, WYS_STANDARD_COMMAND_ID, &jhisrdp, NULL); //0 = cmdId

		if (ret == JHI_SUCCESS) {
			if (((jhisrdp.RxBuf->buffer == NULL) || (jhisrdp.RxBuf->length == 0))) { //sanity check
				retExt = WYS_S_SUCCESS;
			}
			else {
				if (pRespDataLen != NULL && pRespData != NULL)
					if (pRecvBuf != NULL && jhisrdp.RxBuf->length != 0 && jhisrdp.RxBuf->length <= *pRespDataLen)  // valid buffer to copy response into?
						memcpy_s(pRespData, *pRespDataLen, pRecvBuf, jhisrdp.RxBuf->length);

				retExt = WYS_S_SUCCESS;
			}
		}
		else {
			retExt = WYS_E_FAILURE;
		}
	}
	else
	{
		retExt = WYS_E_FAILURE;
	}

	return retExt;
}

WysImage* SecureImage::getWysImageObject(unsigned int wysImageId)
{
	WYSIMAGEOBJS_HASH_MAP::iterator it;

	it = m_wysImageObjs.find(wysImageId);

	if (it != m_wysImageObjs.end()) {
		return it->second;
	}

	return NULL;
}

WYSRESULT SecureImage::sendWYSStdWindowRequest(CREATE_WYSIMAGE *pWysWindowParams,
	CREATE_WINDOW_RESPONSE_MSG *pRespData, UINT32 *pRespDataLength)
{
	WYSRESULT	retExt = WYS_E_INTERNAL_SERVICE_ERROR;
	void		*pReq = NULL;
	void		*pResp = NULL;
	void		*inData = NULL;
	UINT32	dataLength;
	UINT16	reqBufLen;
	UINT16	respBufLen;


	do {

		dataLength = sizeof(CREATE_WYSIMAGE);

		UINT32 logoImgLen = (WYS_LOGO_BYTESPERPIXEL*pWysWindowParams->logoSize.xw.value*pWysWindowParams->logoSize.yh.value);
		if (logoImgLen > 0) {
			dataLength += logoImgLen;
		}

		reqBufLen = dataLength;
		pReq = new UINT8[reqBufLen];
		if (pReq == NULL) {

			retExt = WYS_E_MEMORY;
			break;
		}
		inData = pReq;

		CREATE_WYSIMAGE *WindowMsg;

		WindowMsg = (CREATE_WYSIMAGE *)inData;
		WindowMsg->reserved0 = SUB_COMM_BUILD_WINDOW;
		WindowMsg->wysImageType = pWysWindowParams->wysImageType;
		WindowMsg->buttonBorderWidth = pWysWindowParams->buttonBorderWidth;
		WindowMsg->frameBorderWidth = pWysWindowParams->frameBorderWidth;
		WindowMsg->fontType = pWysWindowParams->fontType;
		WindowMsg->textLength = pWysWindowParams->textLength;
		Wysutil.intToBytes(pWysWindowParams->appParam.value, &WindowMsg->appParam);
		Wysutil.intToBytes(pWysWindowParams->reserved1.value, &WindowMsg->reserved1);
		Wysutil.shortToBytes(pWysWindowParams->wysImageSize.xw.value, &WindowMsg->wysImageSize.xw);
		Wysutil.shortToBytes(pWysWindowParams->wysImageSize.yh.value, &WindowMsg->wysImageSize.yh);
		Wysutil.shortToBytes(pWysWindowParams->buttonSize.xw.value, &WindowMsg->buttonSize.xw);
		Wysutil.shortToBytes(pWysWindowParams->buttonSize.yh.value, &WindowMsg->buttonSize.yh);

		UINT32 wysImageColor = pWysWindowParams->colors.wysImageColor.value;
		UINT32 buttonColor = pWysWindowParams->colors.buttonColor.value;
		UINT32 buttonBorderColor = pWysWindowParams->colors.buttonBorderColor.value;
		UINT32 fontColor = pWysWindowParams->colors.fontColor.value;
		UINT32 frameColor = pWysWindowParams->colors.frameColor.value;
		UINT32 frameBorderColor = pWysWindowParams->colors.frameBorderColor.value;

		WindowMsg->colors.buttonColor.value = (buttonColor << 8) | (buttonColor >> 24);
		WindowMsg->colors.fontColor.value = (fontColor << 8) | (fontColor >> 24);
		WindowMsg->colors.wysImageColor.value = (wysImageColor << 8) | (wysImageColor >> 24);
		WindowMsg->colors.frameColor.value = (frameColor << 8) | (frameColor >> 24);
		WindowMsg->colors.buttonBorderColor.value = (buttonBorderColor << 8) | (buttonBorderColor >> 24);
		WindowMsg->colors.frameBorderColor.value = (frameBorderColor << 8) | (frameBorderColor >> 24);

		Wysutil.shortToBytes(pWysWindowParams->logoSize.xw.value, &WindowMsg->logoSize.xw);
		Wysutil.shortToBytes(pWysWindowParams->logoSize.yh.value, &WindowMsg->logoSize.yh);
		if (logoImgLen > 0) {
			memcpy_s(&WindowMsg->logoImage[0], logoImgLen, pWysWindowParams->logoImage, logoImgLen);
		}

		dataLength = *pRespDataLength;
		respBufLen = dataLength;
		pResp = new UINT8[respBufLen];

		if (pResp == NULL) {
			retExt = WYS_E_MEMORY;
			break;
		}

		retExt = jhiSendRecv(reqBufLen, pReq, respBufLen, pResp, pRespDataLength, (UINT8 *)pRespData);

	} while (0);

	if (pReq != NULL)
	{
		delete[] pReq;
		pReq = NULL;
	}
	if (pResp != NULL)
	{
		delete[] pResp;
		pResp = NULL;
	}

	return retExt;
}

WYSRESULT SecureImage::submitClickInput(INT32 wysImageType, XYPAIR_VECTOR *Clicks)
{
	WYSRESULT retExt = WYS_E_INTERNAL_SERVICE_ERROR;
	void *pReq = NULL;
	void *pResp = NULL;
	SUBMIT_INPUT_CLICKS_MSG *ClicksMsg = NULL;
	UINT32 InputSize = 0;
	UINT16 reqBufLen = 0;
	UINT16 respBufLen = 0;
	UINT32 clicksCount = Clicks->size();

	do
	{
		if (wysImageType == WysStandardImageOK && clicksCount > 1)
		{ // for OK button send only the first click
			clicksCount = 1;
		}
		InputSize = sizeof(SUBMIT_INPUT_CLICKS_MSG) + sizeof(XY_PAIR_BYTES) * clicksCount;
		reqBufLen = InputSize;
		pReq = new UINT8[reqBufLen];
		if (pReq == NULL) {
			retExt = WYS_E_MEMORY;
			break;
		}

		//inData = pReq;
		ClicksMsg = (SUBMIT_INPUT_CLICKS_MSG*)pReq;
		memset(ClicksMsg, 0, InputSize);
		ClicksMsg->subCommand = SUB_COMM_SUBMIT_INPUT;
		ClicksMsg->clicksCount = clicksCount;

		XYPAIR_VECTOR::iterator it;
		XY_PAIR_BYTES pointXY;
		XY_PAIR_BYTES wysButtonClick;
		unsigned int i = 0;

		// PT changes for checking NULL value of "it"
		for (it = Clicks->begin(); i<clicksCount && it != Clicks->end(); ++it, ++i)
		{
			wysButtonClick.xwyh = *it;
			pointXY.xw.MSB = (unsigned char)(((unsigned short)wysButtonClick.xw.value & 0xFF00) >> 8);
			pointXY.xw.LSB = (unsigned char)((unsigned short)wysButtonClick.xw.value & 0x00FF);
			pointXY.yh.MSB = (unsigned char)(((unsigned short)wysButtonClick.yh.value & 0xFF00) >> 8);
			pointXY.yh.LSB = (unsigned char)((unsigned short)wysButtonClick.yh.value & 0x00FF);
			ClicksMsg->clicks[i] = pointXY;
		}

		retExt = jhiSendRecv(reqBufLen, pReq, respBufLen, pResp);
	} while (0);

	if (pReq != NULL)
	{
		delete[] pReq;
		pReq = NULL;
	}
	if (pResp != NULL)
	{
		delete[] pResp;
		pResp = NULL;
	}
	return retExt;
}

/* Submits Captcha user input to the applet*/
WYSRESULT SecureImage::submitCaptchaString(UINT32 captchaStringLen, const wchar_t *sCaptchaString)
{
	WYSRESULT retExt = WYS_E_INTERNAL_SERVICE_ERROR;
	void *pReq = NULL;
	void *pResp = NULL;
	void *inData = NULL;
	SUBMIT_INPUT_TEXT_MSG *ptextMsg = NULL;
	UINT32 InputSize = 0;
	UINT16 reqBufLen = 0;
	UINT16 respBufLen = 0;
	char *pCaptchaStr = NULL;

	do {
		pCaptchaStr = new char[captchaStringLen + 1];

		sprintf_s(pCaptchaStr, captchaStringLen + 1, "%S", sCaptchaString);
		InputSize = sizeof(SUBMIT_INPUT_TEXT_MSG) + captchaStringLen;
		reqBufLen = InputSize;
		pReq = new UINT8[reqBufLen];
		if (pReq == NULL) {
			retExt = WYS_E_MEMORY;
			break;
		}

		inData = pReq;
		ptextMsg = (SUBMIT_INPUT_TEXT_MSG*)inData;
		memset(ptextMsg, 0, InputSize);
		memcpy_s(&(ptextMsg->text), captchaStringLen, pCaptchaStr, captchaStringLen);
		delete[] pCaptchaStr; // free the memory allocated for captcha
		pCaptchaStr = NULL;

		ptextMsg->subCommand = SUB_COMM_SUBMIT_INPUT;
		ptextMsg->textSize = captchaStringLen;

		retExt = jhiSendRecv(reqBufLen, pReq, respBufLen, pResp);
	} while (0);

	if (pReq != NULL) {
		delete[] pReq;
		pReq = NULL;
	}
	if (pResp != NULL) {
		delete[] pResp;
		pResp = NULL;
	}

	return retExt;
}

WYSRESULT SecureImage::ReleaseImageX(UINT32 wysImageId, BOOL bCancel)
{
	WYSRESULT retExt = WYS_S_SUCCESS;
	do {
		if (wysImageId == WYS_IMAGE_ID_ALL)
		{
			if (!m_wysImageObjs.empty())
			{
				WYSIMAGEOBJS_HASH_MAP::iterator it;
				WysImage *pWysImageObj;
				// iterate through all WysImage objs and delete them one by one
				for (it = m_wysImageObjs.begin(); it != m_wysImageObjs.end(); ++it)
				{
					pWysImageObj = it->second;
					if (pWysImageObj != NULL)
						wysCancel(wysImageId);
					DeleteWysImageObject(pWysImageObj, wysImageId);
				}
			}
			m_wysImageObjs.clear(); // clear after all objs have been deleted
		}
		else
		{
			WysImage *pWysImageObj = getWysImageObject(wysImageId);

			if (pWysImageObj == NULL)
			{
				retExt = WYS_E_INVALID_IMAGE_ID;
				break;
			}

			if (bCancel) {
				retExt = wysCancel(wysImageId);
			}

			DeleteWysImageObject(pWysImageObj, wysImageId);
		}

	} while (false);

	return retExt;
}

void SecureImage::DeleteWysImageObject(WysImage *pWysImageObj, unsigned int wysImageId)
{
	if (wysImageId != WYS_IMAGE_ID_ALL)
	{
		m_wysImageObjs.erase(wysImageId); // for WYS_IMAGE_ID_ALL clear the map after all objects are deleted
	}
	handler.DeleteImage(&pWysImageObj->m_pDecoderRenderTargets);
	delete pWysImageObj;
}

WYSRESULT SecureImage::wysCancel(unsigned int appParam)
{
	WYSRESULT retExt = WYS_E_INTERNAL_SERVICE_ERROR;
	void *pReq = NULL;
	void *pResp = NULL;
	CANCEL_MSG *pCancelMsg = NULL;
	UINT32 InputSize = 0;
	UINT16 reqBufLen = 0;
	UINT16 respBufLen = 0;

	do
	{
		InputSize = sizeof(CANCEL_MSG);
		reqBufLen = InputSize;
		pReq = new UINT8[reqBufLen];
		if (pReq == NULL)
		{
			retExt = WYS_E_MEMORY;
			break;
		}

		pCancelMsg = (CANCEL_MSG*)pReq;
		pCancelMsg->subCommand = SUB_COMM_CANCEL;

		retExt = jhiSendRecv(reqBufLen, pReq, respBufLen, pResp);
	} while (0);

	if (pReq != NULL)
	{
		delete[] pReq;
		pReq = NULL;
	}
	if (pResp != NULL)
	{
		delete[] pResp;
		pResp = NULL;
	}

	return retExt;
}

bool SecureImage::closePavpWysSession()
{
	bool res = true;
	//close PAVP session
	if (!handler.ClosePavpSession())
		res = false;
	return res;
}
