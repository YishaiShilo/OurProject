package SecureImageApplet;

import com.intel.util.*;

import java.util.Hashtable;

import com.intel.crypto.*;
import com.intel.crypto.IllegalParameterException;
import com.intel.crypto.NotInitializedException;
import com.intel.langutil.ArrayUtils;
import com.intel.langutil.TypeConverter;
import com.intel.ui.ProtectedOutput;

//
// Implementation of DAL Trusted Application: SecureImageApplet 
//
// **************************************************************************************************
// NOTE:  This default Trusted Application implementation is intended for API Level 1.1 and above
// **************************************************************************************************

public class SecureImageApplet extends IntelApplet {

	/**
	 * 
	 * Y: these comments were before:
	 * This method will be called by the VM when a new session is opened to the Trusted Application 
	 * and this Trusted Application instance is being created to handle the new session.
	 * This method cannot provide response data and therefore calling
	 * setResponse or setResponseCode methods from it will throw a NullPointerException.
	 * 
	 * @param	request	the input data sent to the Trusted Application during session creation
	 * 
	 * @return	APPLET_SUCCESS if the operation was processed successfully, 
	 * any other error status code otherwise (note that all error codes will be
	 * treated similarly by the VM by sending "cancel" error code to the SW application).
	 */
	private SigmaAlgEx  _sigmaAlgEx;
	private byte[]      _sigmaReplyBuffer;
	private int 		s3DataLen;
	
	private byte[]     skey;
	
	private byte[]     mkey;
	
	/*
	 * This is the object that encrypt and decrypt data.
	 */
	private SymmetricBlockCipherAlg cryptoObject;
	private byte[] userAuthenticationId = null;                      // added this for authentication stage
	
	private static final int INTEL_SIGNED_CERT_TYPE	= 4;
	private static final int ZERO_INDEX				= 0;
	
	// Commands
	private static final int CMD_GET_GROUP_ID = 0;
	private static final int CMD_INIT_AND_GET_S1 		= 1;
	private static final int CMD_GET_S3_MESSAGE_LEN 	= 2;
	private static final int CMD_PROCESS_S2_AND_GET_S3 	= 3;
	private static final int CMD_DECRYPT_SYMETRIC_KEY = 5;

	private static final int CMD_GET_AUTHENTICATION_ID = 13;         // added this for authentication stage
	private static final int CMD_GENERATE_PAVP_SESSION_KEY = 6;
	private static final int CMD_SET_NONCE = 10;
	private static final int CMD_IS_PROVISIONED = 11;

	private static final int CMD_RESET = 12;

	
	//Error codes
	private static final int UNRECOGNIZED_COMMAND			= -10;	
	private static final int FAILED_TO_GET_PUBLIC_KEY		= -20;
	private static final int FAILED_TO_INITIALIZE_SIGMA		= -30;
	private static final int INCORRECT_S2_BUFFER			= -40;
	private static final int FAILED_TO_DISPOSE_SIGMA		= -50;
	private static final int WRONG_INTEL_SIGNED_CERT_TYPE	= -60;
	private static final int FAILED_TO_GET_S3_LEN			= -70;
	private static final int FAILED_TO_PROCESS_S2			= -80;
	private static final int FAILED_TO_GET_SESSION_PARAMS	= -90;
	private static final int FAILED_TO_ENCRYPT_DATA	        = -100;
	
	private static final int ERROR_REPLAY = -2;
	private static final int ERROR_EXCEED = -3;

	
	final static int PROVISIONED = 0;
	final static int NOT_PROVISIONED = 1;
	
	byte[] DecryptedSymmetricKey = null;
	boolean dataLoaded;
	boolean shouldSave;
	int limitation, imageId;
	Hashtable map;

	
			
	private int InitializeSigmaInstance() {
		try {
			_sigmaAlgEx.initialize();
		} catch (Throwable e) {
			DebugPrint.printString(e.getMessage());
			DebugPrint.printString("failed init sigma");

			return FAILED_TO_INITIALIZE_SIGMA;
		}
		DebugPrint.printString("init sigma success");
		return APPLET_SUCCESS;
	}
	
	
	private int CreateS1Message() {
		byte[] s1Msg = null;
		int s1MsgLen;
		DebugPrint.printString("S1 start");

		try {
			s1MsgLen = _sigmaAlgEx.getS1MessageLength();
			DebugPrint.printString("S1 2");

			//Create S1 message bytes array
			s1Msg = new byte[s1MsgLen];
			/*
			Get S1 message - This message contains: 
			 	*EPID Group ID of this platform
			 	*Public part of the Diffie-Hellman key (g^a) generated for the current Sigma session
			 	*OCSP request generated for this session
			*/
			int s1Len = _sigmaAlgEx.getS1Message(s1Msg, (short) ZERO_INDEX);
			DebugPrint.printString("S1 3");

			if (s1Len != s1MsgLen)
				return FAILED_TO_GET_PUBLIC_KEY;
		} catch (Throwable e) {
			return FAILED_TO_GET_PUBLIC_KEY;
		}

		//Copy S1 message to the reply buffer
		DebugPrint.printString("S1 after create!");

		_sigmaReplyBuffer = new byte[s1MsgLen];
		ArrayUtils.copyByteArray(s1Msg, ZERO_INDEX, _sigmaReplyBuffer, ZERO_INDEX, s1Msg.length);
		
		return APPLET_SUCCESS;
	}
	
	public static byte[] padding (byte[] input) {
			int len = input.length;
			int padding = 16 - (len % 16);
			int padLen = 16 - (len % 16);
			byte[] pad = new byte[padLen];
			ArrayUtils.fillByteArray(pad, 0, padLen, (byte) padding );
			byte[] padded = new byte[padLen + len];

			System.arraycopy(input, 0, padded, 0, len);
		    System.arraycopy(pad, 0, padded, len, padLen)  ;
			return padded;
		}
		
	public static byte[] unpadding (byte[] input) {
			// TODO: add padding check
			int padLen = input[input.length -1] / 8;
			//System.out.println(padLen);
			int dataLen = input.length - padLen;
            //System.out.println(dataLen);

			byte[] output = new byte[dataLen];
			System.arraycopy(input, 0, output, 0, dataLen);
			return output;
		}
		
	
	
	private int GetS3MessageLen(byte[] s2Message){
		
		try {
			s3DataLen = _sigmaAlgEx.getS3MessageLength(s2Message, ZERO_INDEX, s2Message.length);
		} catch (Throwable e) {
			return FAILED_TO_GET_S3_LEN;
		}
		if (s3DataLen == 0) {
			return FAILED_TO_GET_S3_LEN;
		}
		
		//Copy S3 message length to the reply buffer
		byte[] s3DataLenBytesArray = new byte[TypeConverter.INT_BYTE_SIZE];
		TypeConverter.intToBytes(s3DataLen, s3DataLenBytesArray, ZERO_INDEX);
		_sigmaReplyBuffer = new byte[TypeConverter.INT_BYTE_SIZE];
		ArrayUtils.copyByteArray(s3DataLenBytesArray, ZERO_INDEX, _sigmaReplyBuffer, ZERO_INDEX, s3DataLenBytesArray.length);
		
		return APPLET_SUCCESS;
	}
	
	//Verify received S2 message and create S3 message
	private int VerifyS2AndCreateS3(byte[] s2Message) {
		//Received an empty message
		if (s2Message == null)
			return INCORRECT_S2_BUFFER;
				
		//Create S3 message array
		byte[] s3Data = new byte[s3DataLen];
		
		/*
		 Process S2 message and get S3 message - This message contains: 
			*Task information that identifies the initiator of this Sigma session inside the firmware, including the specific trusted application that created this session (accordig to UUID) 
			*Prover's EPID certificate 
			*Prover's public part of the Diffie-Hellman key (g^a) (equal to the one sent in S1) 
			*HMAC computed using the session MAC key on several fields in the message, as required by the protocol 
			*Prover's EPID signature on several fields in the message, as required by the protocol 
			*Non-Revoked proofs created by the prover based on the Signature Revocation List from S2 
		 */
		try {
			int res = _sigmaAlgEx.processS2Message(s2Message, ZERO_INDEX, s2Message.length, s3Data, ZERO_INDEX);
			if (res != s3DataLen) {
				return FAILED_TO_PROCESS_S2;
			}

		} catch (Throwable e) {
			return FAILED_TO_PROCESS_S2;
		}
		
		//Get Session Parameters
		int ret = GetSessionParameters();
		if(ret != APPLET_SUCCESS)
			return ret;
		
		//Copy S3 message to the reply buffer
		_sigmaReplyBuffer = new byte[s3Data.length];									
		ArrayUtils.copyByteArray(s3Data, ZERO_INDEX, _sigmaReplyBuffer, ZERO_INDEX, s3DataLen);
		
		short secretKeyLen = _sigmaAlgEx.getSecretKeySize();
		byte[] secretKey = new byte[secretKeyLen];
		_sigmaAlgEx.getSecretKey(secretKey, (short) 0);
		skey = secretKey;
		DebugPrint.printString("key Len: " + Integer.toString(secretKeyLen));
		DebugPrint.printString("key : " + bytesToHex(secretKey));
		try
		{
		initialyzeCryptoObject();
		}
		catch (Exception e)
		{
			DebugPrint.printString("fail init crypto");
			return -110;
		}
		

		//Once the SigmaAlgEx instance usage is finished, we have to clean and free the resource
		return CleanSigmaInstance();
	}
	
	private int GetAuthenticationId(byte[] IdCode)                   // added this for authentication stage:
	{
		userAuthenticationId = IdCode;
		DebugPrint.printString("IdCode : " + new String(userAuthenticationId));
		// TODO: need to return the encrypted Id to send to server
		try 
		{
			byte[] padded = padding(userAuthenticationId);
			DebugPrint.printString("padded: " + new String(padded));
			DebugPrint.printString("padded: " +bytesToHex(padded));

			byte [] encryptedId = encrypt(padded, padded.length);
			
			_sigmaReplyBuffer = new byte[encryptedId.length];
			DebugPrint.printString("encryptedId: " + new String(encryptedId));
			ArrayUtils.copyByteArray(encryptedId, ZERO_INDEX, _sigmaReplyBuffer, ZERO_INDEX, encryptedId.length);			
			return APPLET_SUCCESS;
		}
		catch (Exception e)
		{
			DebugPrint.printString("exception");
			DebugPrint.printString(e.toString());
			DebugPrint.printString(e.getMessage());

			return FAILED_TO_ENCRYPT_DATA;
		}
		
		
	}
	
	
	private static final char[] HEX_ARRAY = "0123456789ABCDEF".toCharArray();
	public static String bytesToHex(byte[] bytes) {
	    char[] hexChars = new char[bytes.length * 2];
	    for (int j = 0; j < bytes.length; j++) {
	        int v = bytes[j] & 0xFF;
	        hexChars[j * 2] = HEX_ARRAY[v >>> 4];
	        hexChars[j * 2 + 1] = HEX_ARRAY[v & 0x0F];
	    }
	    return new String(hexChars);
	}

	private int GetSessionParameters(){
		/*  We are talking with a third party that asked Intel to sign its certificate. Now, after the DAL validation of S2, we know that the third party's certificate
		 *  is signed by Intel, and we can trust the certificate content, and check some of the certificate parameters in order to know exactly who is
		 *  the third party we are talking with.  We get some main session parameters.
		 *	We want a full authentication, and therefore we get the following parameters:

			*The product type of the verifier's certificate received in S2. - this is a mandatory checking. now we know who exactly is the third party. 
			for example: DAL Tests, PAVP and etc.
			The rest of the validation is optional, if you want to know more about the verifier's certificate. 
				*The serial number of the verifier's certificate received in S2. - if the verifier has several signed certificates and we want to know exactly
				*which of them is the specific one.
				*The issuer unique ID as specified in the verifier's certificate received in S2. - if the issuer has an unique ID.
				*The subject unique ID as specified in the verifier's certificate received in S2. - if the subject has an unique ID.
		 */
		
		byte[] sessionParamVerCertProductType = new byte[0];
		
		//--- mandatory validation ---
		//Get product type
		//The product type is a private extension.  It is equivalent to the "Intel signed certificate type" field of the legacy certificate.  It shows the product type,
		//e.g., Media Vault, One-Time-Password, etc.  This extension is mandatory for Intel-signed certificates.  
		try {
			sessionParamVerCertProductType = _sigmaAlgEx.getSessionParameter(SigmaAlgEx.SESSION_PARAM_VERIFIER_CERT_PRODUCT_TYPE);
			int certType = TypeConverter.bytesToInt(sessionParamVerCertProductType, ZERO_INDEX);
			if (certType != INTEL_SIGNED_CERT_TYPE)
				return WRONG_INTEL_SIGNED_CERT_TYPE;
		} catch (Throwable e) {
			return FAILED_TO_GET_SESSION_PARAMS;
		}
		
		
		/* --- optional validation ---
		
		byte[] sessionParamVerCertSerNum = new byte[0];
		byte[] sessionParamVerCertIssuerId = new byte[0];
		byte[] sessionParamVerCertSubId = new byte[0];

		//Get serial number
		try {
			sessionParamVerCertSerNum = _sigmaAlgEx.getSessionParameter(SESSION_PARAM_VERIFIER_CERT_SERIAL_NUMBER);
		} catch (Throwable e) {
			return FAILED_TO_GET_SESSION_PARAMS;
		}
		//Get issuer unique ID
		try {
			sessionParamVerCertIssuerId = _sigmaAlgEx.getSessionParameter(SESSION_PARAM_VERIFIER_CERT_ISSUER_UNIQUE_ID);
		} catch (Throwable e) {
			return FAILED_TO_GET_SESSION_PARAMS;
		}
		//Get subject unique ID
		try {
			sessionParamVerCertSubId = _sigmaAlgEx.getSessionParameter(SESSION_PARAM_VERIFIER_CERT_SUBJECT_UNIQUE_ID);
		} catch (Throwable e) {
			return FAILED_TO_GET_SESSION_PARAMS;
		}*/

		return APPLET_SUCCESS;
	}
	
	private int CleanSigmaInstance() {
		//Once the SigmaAlgEx instance usage is finished, we have to clean and free the resource
		try {
			_sigmaAlgEx.dispose();
		} catch (Throwable e) {
			return FAILED_TO_DISPOSE_SIGMA;
		}
		return APPLET_SUCCESS;
	}
	
	
	
	private void initialyzeCryptoObject() {
		/*
		 * A function that initialize cryptoObject, which is 
		 * an object that we use for encryption and decryption.
		 * we need to call it somewhere! 
		 */
		DebugPrint.printString("before create cipher");
		cryptoObject = SymmetricBlockCipherAlg.create(SymmetricBlockCipherAlg.ALG_TYPE_AES_CBC);
		DebugPrint.printString("after create cipher");

		byte[] ivArray = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
		// TODO: set random IV and send to server.
		cryptoObject.setKey(skey, (short)0, (short)16);                //the key here is 128 bit long!
		DebugPrint.printString("after set key");
		cryptoObject.setIV(ivArray, (short)0, (short)16);               // the iv is set to 0 by default so this function 
																		// doesn't really important
		DebugPrint.printString("after set iv");
	}
		
	
	private byte[] encrypt(byte[] data, int dataLength) {
		/*
		 * A function that encrypt the given data with our public key.
		 * dataLength is the length of the data to be encrypted in bytes.
		 */
		byte [] output = new byte[dataLength];
		cryptoObject.encryptComplete(data, (short)0, (short)dataLength, output, (short)0);
		return output;
	}
	
	
	private byte[] decrypt(byte[] data, int dataLength) {
		/*
		 * A function that decrypt the given data with our public key.
		 * dataLength is the length of the data to be crypted in bytes.
		 */
		byte [] output = new byte[dataLength];
		cryptoObject.decryptComplete(data, (short)0, (short)dataLength, output, (short)0);
		return output;
	}
	
	private int decryptSymetricKey(byte[] commandData)
	{
		int status = IntelApplet.APPLET_ERROR_GENERIC;
		DebugPrint.printString("Decrypting symetric key...");

		try
		{
			// validate the data was loaded
			if (!dataLoaded)
			{
				status = ERROR_REPLAY;
				throw new Exception("Data replay.");
			}
			
			DecryptedSymmetricKey = new byte[256];
			byte DecryptedBuffer[] = new byte[256];
			byte Times[] = new byte[4];

			

			short count;
			try
			{
				// TODO: is need to be in length of 24 bytes (4 times, 4 id, 16 key)?. or 
				count = cryptoObject.decryptComplete(commandData, (short) 0, (short) 32, DecryptedBuffer, (short) 0);
			}
			catch (IllegalParameterException ex)
			{
				DebugPrint.printString("0");
				throw new Exception("illegalParameterException");
			}
			catch (ComputationException ex)
			{
				DebugPrint.printString("1");
				throw new Exception("computationException, error returned from the crypto engine.");
			}
			catch (NotInitializedException ex)
			{
				DebugPrint.printString("2");
				throw new Exception("notInitializedException");
			}
			catch (CryptoException ex)
			{
				DebugPrint.printString("3");

				throw new Exception("an internal error occurred");
			}
			DebugPrint.printString("after decrypt");
			ArrayUtils.copyByteArray(DecryptedBuffer, 0, Times, 0, 4);
			limitation = TypeConverter.bytesToInt(Times, 0);
			DebugPrint.printString("limit:");

			DebugPrint.printInt(limitation);
			ArrayUtils.copyByteArray(DecryptedBuffer, 4, Times, 0, 4);
			// get image Id
			imageId = TypeConverter.bytesToInt(Times, 0);
			DebugPrint.printString("iamge id:");
			DebugPrint.printInt(imageId);
			// get the times the picture was viewed, the image Id is the key in
			// the map
			Object key = new Integer(imageId);
			Integer timesViewed = (Integer) map.get(key);
			if (timesViewed == null)
			{
				// this is a new picture, there is still no key and value for it
				timesViewed = new Integer(0);
			}
			// check that the times were not exceeded
			if (timesViewed.intValue() >= limitation)
			{
				status = ERROR_EXCEED;
				throw new Exception("Error: Cannot decrypt key. Times exceeded");
			}
			// everything is OK, can save the decrypted key
			ArrayUtils.copyByteArray(DecryptedBuffer, 8, DecryptedSymmetricKey, 0, 248);
			// //////////////
			DebugPrint.printString("Key size (count):" + count);
			DebugPrint.printString("Decrypted Symmetric Key:");
			byte[] debugKey = new byte[16];
			ArrayUtils.copyByteArray(DecryptedSymmetricKey, 0, debugKey, 0, 16);
			DebugPrint.printBuffer(debugKey);
			
			// //////////////

			status = IntelApplet.APPLET_SUCCESS;
		}
		catch (Exception ex)
		{
			byte[] errorResponse = ("Error: failed to decrypt symetric key - " + ex.getMessage()).getBytes();
			setResponse(errorResponse, 0, errorResponse.length);
			DebugPrint.printString("Error: failed to decrypt symetric key\n"
					+ ex.getMessage() + ex.toString());
			return status;
		}

		DebugPrint.printString("Decrypting symetric key Succeeded!");
		return status;
	}
	
	private int generatePavpKey(byte[] commandData)
	{
		int status = IntelApplet.APPLET_ERROR_GENERIC;
		DebugPrint.printString("Generating PAVP key...");

		try
		{
			// validate the data was loaded
			if (!dataLoaded)
			{
				status = ERROR_REPLAY;
				throw new Exception("Data replay.");
			}
			// get the times the picture was viewed
			Object key = new Integer(imageId);
			Integer timesViewed = (Integer) map.get(key);
			if (timesViewed == null)
			{
				// this is a new picture, there is still no key and value for it
				timesViewed = new Integer(0);
			}
			// check that the times were not exceeded
			if (timesViewed.intValue() >= limitation)
			{
				status = ERROR_EXCEED;
				throw new Exception("Error: Cannot create key. Times exceeded");
			}
			// increment the times the picture was viewed
			timesViewed = new Integer(timesViewed.intValue() + 1);
			// update the map with the new value
			map.put(key, (Object) timesViewed);
			// flag that the data should be saved
			shouldSave = true;

			// get the ME Handle
			int slotHandle = TypeConverter.bytesToInt(commandData, 0);

			// Create the session key
			ProtectedOutput pOutput = ProtectedOutput.getInstance(slotHandle, DecryptedSymmetricKey, (short) 0, (short) 16);

			byte response[] = new byte[16];

			// retrieve encrypted key record to be provided to GFX driver for
			// key injection
			pOutput.getEncryptedKeyRecord(response, (short) 0);

			setResponse(response, 0, response.length);

			status = IntelApplet.APPLET_SUCCESS;
		}
		catch (Exception ex)
		{
			byte[] errorResponse = ("Error: failed to generate PAVP session key - " + ex.getMessage()).getBytes();
			setResponse(errorResponse, 0, errorResponse.length);
			DebugPrint.printString("Error: failed to generate PAVP Session key\n"
					+ ex.getMessage());
			return status;
		}

		DebugPrint.printString("Generating PAVP Session key Succeeded!");
		return status;
	}

	
	
	public int onInit(byte[] request) {
		DebugPrint.printString("Protected Output Usage Applet: onInit");
		map = new Hashtable();
		shouldSave = false;
		limitation = 0;
		imageId = 0;
		if (MTC.getMTC() == 0)
		{
			// first usage of this Trusted Application, no data to load
			dataLoaded = true;
		}
		else
		{
			// there is data to load
			dataLoaded = false;
		}
		return APPLET_SUCCESS;
	}
	
	/**
	 * This method will be called by the VM to handle a command sent to this
	 * Trusted Application instance.
	 * 
	 * @param	commandId	the command ID (Trusted Application specific) 
	 * @param	request		the input data for this command 
	 * @return	the return value should not be used by the applet
	 */
	public int invokeCommand(int commandId, byte[] request) {	
		if (_sigmaAlgEx == null)
			_sigmaAlgEx = SigmaAlgEx.createInstance(SigmaAlgEx.SIGMA_PROTOCOL_VERSION_1_1);
		
		//Reset the reply buffer and result code
		_sigmaReplyBuffer = null;
		int result = APPLET_SUCCESS;
		
		//Check what is the wanted action
		DebugPrint.printString("command:" + Integer.toString(commandId));

		switch(commandId)
		{						
			case CMD_INIT_AND_GET_S1:
			{
				result = InitializeSigmaInstance();
				DebugPrint.printString("init inst");
				if (result == APPLET_SUCCESS)
				{
					result = CreateS1Message();
				}
				break;
			}
			
			case CMD_GET_S3_MESSAGE_LEN:
			{
				DebugPrint.printString("try to get s3len");

				result = GetS3MessageLen(request);
				break;
			}	
			
			// we are here. need to write the function: "VerifyS2AndCreateS3".
			case CMD_PROCESS_S2_AND_GET_S3:
			{
				result = VerifyS2AndCreateS3(request);	
				break;
			}		
			
			case CMD_GET_AUTHENTICATION_ID:                 // added this for authentication stage:
			{
				DebugPrint.printString("in-aut id");
				result = GetAuthenticationId(request);
				break;
			}
			
			case CMD_DECRYPT_SYMETRIC_KEY:
			{
				result = decryptSymetricKey(request);
				break;
			}	
			case CMD_GENERATE_PAVP_SESSION_KEY:
				result = generatePavpKey(request);
				break;
			
			
			default:
			{
				result = UNRECOGNIZED_COMMAND;
				break;
			}
		}		
		/*
		 * To return the response data to the command, call the setResponse
		 * method before returning from this method. Note that calling this
		 * method more than once will reset the response data previously set.
		 */

		/*
		 * In order to provide a return value for the command, which will be
		 * delivered to the SW application communicating with the Trusted Application,
		 * setResponseCode method should be called. Note that calling this
		 * method more than once will reset the code previously set. If not set,
		 * the default response code that will be returned to SW application is 0.
		 */
		setResponseCode(result);
		if(_sigmaReplyBuffer != null)
			setResponse(_sigmaReplyBuffer, ZERO_INDEX, _sigmaReplyBuffer.length);

		/*
		 * The return value of the invokeCommand method is not guaranteed to be
		 * delivered to the SW application, and therefore should not be used for
		 * this purpose. Trusted Application is expected to return APPLET_SUCCESS code 
		 * from this method and use the setResposeCode method instead.
		 */
		return APPLET_SUCCESS;
	}

	/**
	 * This method will be called by the VM when the session being handled by
	 * this Trusted Application instance is being closed 
	 * and this Trusted Application instance is about to be removed.
	 * This method cannot provide response data and therefore
	 * calling setResponse or setResponseCode methods from it will throw a NullPointerException.
	 * 
	 * @return APPLET_SUCCESS code (the status code is not used by the VM).
	 */
	public int onClose() {
		DebugPrint.printString("Goodbye, DAL!");
		return APPLET_SUCCESS;
	}
}
