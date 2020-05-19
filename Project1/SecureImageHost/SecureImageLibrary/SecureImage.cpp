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
#include <fstream>
#include <iostream>
#include <map>
//Prototype for the delay load function
FARPROC WINAPI delayHook(unsigned dliNotify, PDelayLoadInfo pdli);
PfnDliHook __pfnDliNotifyHook2 = delayHook;

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
	encryptedBitmap=new UINT8[encryptedBitmapSize];
	//copy the encrypted bitmap buffer from the server data
	memcpy(encryptedBitmap,&ServerData[16+keySize],encryptedBitmapSize);
	//call the show function to display the image
	return PavpHandler::Session()->ShowImage(encryptedBitmap,encryptedBitmapSize,targetControl);
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

int SecureImage::parseMetaData(byte *rawData)
{
	//extract the size of the encrypted private key, mod and exponent
	mod_size = (short)((rawData[0] << 8) | rawData[1]); // should be 256
	signed_mod_size = (short)((rawData[2] << 8) | rawData[3]); 
	e_size = (short)((rawData[4] << 8) | rawData[5]); // should be 4
	signed_e_size = (short)((rawData[6] << 8) | rawData[7]); 
	d_size = (short)((rawData[8] << 8) | rawData[9]); // should be 256

	//allocate buffers for encrypted private key, mod and exponent
	signed_mod = new byte[signed_mod_size];
	signed_e = new byte[signed_e_size];
	mod = new byte[mod_size];
	e = new byte[e_size];
	d = new byte[d_size];

	//init the data members of encrypted private key, mod and exponent
	int currInd=10;
	memcpy(mod,rawData+currInd,mod_size);
	currInd+=mod_size;
	memcpy(signed_mod,rawData+currInd,signed_mod_size);
	currInd+=signed_mod_size;
	memcpy(e,rawData+currInd,e_size);
	currInd+=e_size;
	memcpy(signed_e,rawData+currInd,signed_e_size);
	currInd+=signed_e_size;
	memcpy(d,rawData+currInd,d_size);
	currInd+=d_size;
	return currInd;
}


SecureImage::~SecureImage(void)
{
	
}

