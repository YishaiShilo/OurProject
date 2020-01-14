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
	private static final int CMD_GET_S3_MESSAGE_LEN 		= 2;
	private static final int CMD_PROCESS_S2_AND_GET_S3 	= 3;

	//Error codes
	private static final int UNRECOGNIZED_COMMAND				= -10;	
	private static final int FAILED_TO_GET_PUBLIC_KEY			= -20;
	private static final int FAILED_TO_INITIALIZE_SIGMA		= -30;
	private static final int INCORRECT_S2_BUFFER				= -40;
	private static final int FAILED_TO_DISPOSE_SIGMA			= -50;
	private static final int WRONG_INTEL_SIGNED_CERT_TYPE		= -60;
	private static final int FAILED_TO_GET_S3_LEN				= -70;
	private static final int FAILED_TO_PROCESS_S2				= -80;
	private static final int FAILED_TO_GET_SESSION_PARAMS		= -90;
	
	
	private int InitializeSigmaInstance() {
		try {
			_sigmaAlgEx.initialize();
		} catch (Throwable e) {
			return FAILED_TO_INITIALIZE_SIGMA;
		}
		return APPLET_SUCCESS;
	}
	
	
	private int CreateS1Message() {
		byte[] s1Msg = null;
		int s1MsgLen;
		
		try {
			s1MsgLen = _sigmaAlgEx.getS1MessageLength();
			//Create S1 message bytes array
			s1Msg = new byte[s1MsgLen];
			/*
			Get S1 message - This message contains: 
			 	*EPID Group ID of this platform
			 	*Public part of the Diffie-Hellman key (g^a) generated for the current Sigma session
			 	*OCSP request generated for this session
			*/
			int s1Len = _sigmaAlgEx.getS1Message(s1Msg, (short) ZERO_INDEX);
			if (s1Len != s1MsgLen)
				return FAILED_TO_GET_PUBLIC_KEY;
		} catch (Throwable e) {
			return FAILED_TO_GET_PUBLIC_KEY;
		}

		//Copy S1 message to the reply buffer
		_sigmaReplyBuffer = new byte[s1MsgLen];
		ArrayUtils.copyByteArray(s1Msg, ZERO_INDEX, _sigmaReplyBuffer, ZERO_INDEX, s1Msg.length);
		
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
		switch(commandId)
		{						
			case CMD_INIT_AND_GET_S1:
			{
				result = InitializeSigmaInstance();
				if (result == APPLET_SUCCESS)
					result = CreateS1Message();
				break;
			}
			// we need to add here the next cases.
			
			
			
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
