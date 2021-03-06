package SecureImageApplet;

import com.intel.crypto.SigmaAlgEx;
import com.intel.crypto.SymmetricBlockCipherAlg;
import com.intel.langutil.ArrayUtils;
import com.intel.langutil.TypeConverter;
import com.intel.util.DebugPrint;


public class Sigma {
	private static final int CMD_INIT_AND_GET_S1 		= 1;
	private static final int CMD_GET_S3_MESSAGE_LEN 	= 2;
	private static final int CMD_PROCESS_S2_AND_GET_S3 	= 3;
	private static final int APPLET_SUCCESS = com.intel.util.IntelApplet.APPLET_SUCCESS;
	private static final int UNRECOGNIZED_COMMAND = -10;
	
	private static final int FAILED_TO_GET_S3_LEN = -70;
	private static final int FAILED_TO_GET_PUBLIC_KEY = -20;
	private static final int WRONG_INTEL_SIGNED_CERT_TYPE	= -60;
	private static final int INTEL_SIGNED_CERT_TYPE	= 4;
	private static final int FAILED_TO_GET_SESSION_PARAMS	= -90;
	
	private static final char[] HEX_ARRAY = "0123456789ABCDEF".toCharArray();
	public byte[]      _sigmaReplyBuffer;
	
	private static final int FAILED_TO_INITIALIZE_SIGMA		= -30;
	private byte[] skey;
	
	private static final int FAILED_TO_DISPOSE_SIGMA		= -50;
	
	
	private static final int INCORRECT_S2_BUFFER			= -40;
	private int 		s3DataLen;
	private static final int ZERO_INDEX = 0;
	private static final int FAILED_TO_PROCESS_S2			= -80;
	
	
	SigmaAlgEx  _sigmaAlgEx;
	public SymmetricBlockCipherAlg cryptoObject;
	
	
	int SUCCESS = 0;
	int fail = 1;
	

	
	public static String bytesToHex(byte[] bytes) {
	    char[] hexChars = new char[bytes.length * 2];
	    for (int j = 0; j < bytes.length; j++) {
	        int v = bytes[j] & 0xFF;
	        hexChars[j * 2] = HEX_ARRAY[v >>> 4];
	        hexChars[j * 2 + 1] = HEX_ARRAY[v & 0x0F];
	    }
	    return new String(hexChars);
	}
	
	

	
	
	public byte[] encrypt(byte[] data, int dataLength) {
		/*
		 * A function that encrypt the given data with our public key. dataLength is the
		 * length of the data to be encrypted in bytes.
		 */
		byte[] output = new byte[dataLength];
		cryptoObject.encryptComplete(data, (short) 0, (short) dataLength, output, (short) 0);
		return output;
	}

	public byte[] decrypt(byte[] data, int dataLength) {
		/*
		 * A function that decrypt the given data with our public key. dataLength is the
		 * length of the data to be decrypted in bytes.
		 */
		byte[] output = new byte[dataLength];
		cryptoObject.decryptComplete(data, (short) 0, (short) dataLength, output, (short) 0);
		return output;
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
	
	
	
	private void initialyzeCryptoObject() {
		/*
		 * A function that initialize cryptoObject, which is an object that we use for
		 * encryption and decryption. we need to call it somewhere!
		 */
		DebugPrint.printString("before create cipher");
		cryptoObject = SymmetricBlockCipherAlg.create(SymmetricBlockCipherAlg.ALG_TYPE_AES_CBC);
		DebugPrint.printString("after create cipher");

		byte[] ivArray = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
		// TODO: set random IV and send to server.
		cryptoObject.setKey(skey, (short) 0, (short) 16); // the key here is 128 bit long!
		DebugPrint.printString("after set key");
		cryptoObject.setIV(ivArray, (short) 0, (short) 16); // the iv is set to 0 by default so this function
															// doesn't really important
		DebugPrint.printString("after set iv");
	}
	
	
	
	//Verify received S2 message and create S3 message
	private int VerifyS2AndCreateS3(byte[] s2Message) {
		DebugPrint.printString("im in the start of VerifyS2AndCreateS3. s3DataLen = " + Integer.toString(s3DataLen));
		//Received an empty message
		
		if (s2Message == null) {
			DebugPrint.printString("im in the checking of s2 message");
			return INCORRECT_S2_BUFFER;
		}
				
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
			DebugPrint.printString("im in the beggining of the try.");
			int res = _sigmaAlgEx.processS2Message(s2Message, ZERO_INDEX, s2Message.length, s3Data, ZERO_INDEX);
			DebugPrint.printString("im in the try. res = " + Integer.toString(res));
			if (res != s3DataLen) {
				DebugPrint.printString("im in the if. res = " + Integer.toString(res) + "s3DataLen = " + Integer.toString(s3DataLen));
				return FAILED_TO_PROCESS_S2;
			}

		} catch (Throwable e) {
			DebugPrint.printString("im in the catch");
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
	
	
	private int InitializeSigmaInstance() {
		try {
			DebugPrint.printString("im before initialyze alg");
			_sigmaAlgEx.initialize();
			DebugPrint.printString("im after initialyze alg");
		} catch (Throwable e) {
			DebugPrint.printString("im catch 1");
			DebugPrint.printString(e.getMessage());
			DebugPrint.printString("failed init sigma");

			return FAILED_TO_INITIALIZE_SIGMA;
		}
		DebugPrint.printString("init sigma success");
		return APPLET_SUCCESS;
	}
	
	int handleCommand(int commandId, byte[] request) {
		
		if (_sigmaAlgEx == null) {
			_sigmaAlgEx = SigmaAlgEx.createInstance(SigmaAlgEx.SIGMA_PROTOCOL_VERSION_1_1);
		}
		int result = SUCCESS;
		
		
		DebugPrint.printString("im before the switch");
		switch(commandId) {
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
			
			
			case CMD_PROCESS_S2_AND_GET_S3:
			{
				DebugPrint.printString("im before VerifyS2AndCreateS3");
				result = VerifyS2AndCreateS3(request);	
				break;
			}
			default:
			{
				result = UNRECOGNIZED_COMMAND;
				break;
			}
			
		}
		return result;
	}
	
}








