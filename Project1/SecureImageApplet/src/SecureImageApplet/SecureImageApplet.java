package SecureImageApplet;

import com.intel.util.*;
import com.intel.crypto.*;
import com.intel.langutil.ArrayUtils;
import com.intel.langutil.TypeConverter;

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
	
	private static final int INTEL_SIGNED_CERT_TYPE	= 4;
	private static final int ZERO_INDEX				= 0;
	
	// Commands
	private static final int CMD_INIT_AND_GET_S1 		= 1;
	private static final int CMD_GET_S3_MESSAGE_LEN 	= 2;
	private static final int CMD_PROCESS_S2_AND_GET_S3 	= 3;

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

		//Once the SigmaAlgEx instance usage is finished, we have to clean and free the resource
		return CleanSigmaInstance();
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
	
	public int onInit(byte[] request) {
		DebugPrint.printString("Hello, DAL!");
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
		DebugPrint.printString("command: " + Integer.toString(commandId));

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
