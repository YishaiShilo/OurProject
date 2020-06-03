package SecureImageApplet;


import java.util.Hashtable;


import com.intel.util.*;
import com.intel.crypto.*;
import com.intel.crypto.IllegalParameterException;
import com.intel.crypto.NotInitializedException;
import com.intel.langutil.ArrayUtils;
import com.intel.langutil.TypeConverter;
import com.intel.ui.ProtectedOutput;

import SecureImageApplet.WYS.WYS;

//
// Implementation of DAL Trusted Application: SecureImageApplet 
//
// **************************************************************************************************
// NOTE:  This default Trusted Application implementation is intended for API Level 1.1 and above
// **************************************************************************************************

public class SecureImageApplet extends IntelApplet {

	/**
	 * 
	 * Y: these comments were before: This method will be called by the VM when a
	 * new session is opened to the Trusted Application and this Trusted Application
	 * instance is being created to handle the new session. This method cannot
	 * provide response data and therefore calling setResponse or setResponseCode
	 * methods from it will throw a NullPointerException.
	 * 
	 * @param request the input data sent to the Trusted Application during session
	 *                creation
	 * 
	 * @return APPLET_SUCCESS if the operation was processed successfully, any other
	 *         error status code otherwise (note that all error codes will be
	 *         treated similarly by the VM by sending "cancel" error code to the SW
	 *         application).
	 */

	private byte[] _sigmaReplyBuffer;
	private Sigma sigma;
	private WYS wysIns;


	/*
	 * This is the object that encrypt and decrypt data.
	 */
	
	private byte[] userAuthenticationId = null; // added this for authentication stage

	private static final int ZERO_INDEX = 0;

	// Commands
	private static final int CMD_GET_GROUP_ID = 0;
	private static final int CMD_DECRYPT_SYMETRIC_KEY = 5;
	private static final int CMD_INIT_SIGMA = 1;

	private static final int CMD_GET_AUTHENTICATION_ID = 13; // added this for authentication stage
	private static final int CMD_WYS_STANDARD = 0xFFFF0001;
	private static final int CMD_GENERATE_PAVP_SESSION_KEY = 6;
	private static final int CMD_SET_NONCE = 10;
	private static final int CMD_IS_PROVISIONED = 11;
	private static final int CMD_RESET = 12;

	// Error codes
	private static final int UNRECOGNIZED_COMMAND = -10;
	private static final int FAILED_TO_ENCRYPT_DATA = -100;
	private static final int ERROR_REPLAY = -2;
	private static final int ERROR_EXCEED = -3;

	final static int PROVISIONED = 0;
	final static int NOT_PROVISIONED = 1;

	
	
	byte[] DecryptedSymmetricKey = null;
	boolean dataLoaded;
	boolean shouldSave;
	int limitation, imageId;
	Hashtable map;
	
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


	public static byte[] padding(byte[] input) {
		int len = input.length;
		int padding = 16 - (len % 16);
		int padLen = 16 - (len % 16);
		byte[] pad = new byte[padLen];
		ArrayUtils.fillByteArray(pad, 0, padLen, (byte) padding);
		byte[] padded = new byte[padLen + len];

		System.arraycopy(input, 0, padded, 0, len);
		System.arraycopy(pad, 0, padded, len, padLen);
		return padded;
	}

	public static byte[] unpadding(byte[] input) {
		// TODO: add padding check
		int padLen = input[input.length - 1] / 8;
		// System.out.println(padLen);
		int dataLen = input.length - padLen;
		// System.out.println(dataLen);

		byte[] output = new byte[dataLen];
		System.arraycopy(input, 0, output, 0, dataLen);
		return output;
	}

	private int GetAuthenticationId(byte[] IdCode) // added this for authentication stage:
	{
		userAuthenticationId = IdCode;
		DebugPrint.printString("IdCode : " + new String(userAuthenticationId));
		// TODO: need to return the encrypted Id to send to server
		try {
			byte[] padded = padding(userAuthenticationId);
			DebugPrint.printString("padded: " + new String(padded));
			DebugPrint.printString("padded: " + bytesToHex(padded));

			byte[] encryptedId = sigma.encrypt(padded, padded.length);

			_sigmaReplyBuffer = new byte[encryptedId.length];
			DebugPrint.printString("encryptedId: " + new String(encryptedId));
			ArrayUtils.copyByteArray(encryptedId, ZERO_INDEX, _sigmaReplyBuffer, ZERO_INDEX, encryptedId.length);
			return APPLET_SUCCESS;
		} catch (Exception e) {
			DebugPrint.printString("exception");
			DebugPrint.printString(e.toString());
			DebugPrint.printString(e.getMessage());

			return FAILED_TO_ENCRYPT_DATA;
		}

	}
	
	private int WYS_method(byte[] request) {
		// TODO Auto-generated method stub
		return 0;
	}


	private int decryptSymetricKey(byte[] commandData) {
		int status = IntelApplet.APPLET_ERROR_GENERIC;
		DebugPrint.printString("Decrypting symetric key...");

		try {
			// validate the data was loaded
			if (!dataLoaded) {
				status = ERROR_REPLAY;
				throw new Exception("Data replay.");
			}

			DecryptedSymmetricKey = new byte[256];
			byte DecryptedBuffer[] = new byte[256];
			byte Times[] = new byte[4];

			short count;
			try {
				// TODO: is need to be in length of 24 bytes (4 times, 4 id, 16 key)?. or
				count = sigma.cryptoObject.decryptComplete(commandData, (short) 0, (short) 32, DecryptedBuffer, (short) 0);
			} catch (IllegalParameterException ex) {
				DebugPrint.printString("0");
				throw new Exception("illegalParameterException");
			} catch (ComputationException ex) {
				DebugPrint.printString("1");
				throw new Exception("computationException, error returned from the crypto engine.");
			} catch (NotInitializedException ex) {
				DebugPrint.printString("2");
				throw new Exception("notInitializedException");
			} catch (CryptoException ex) {
				DebugPrint.printString("3");

				throw new Exception("an internal error occurred");
			}
			DebugPrint.printString("after decrypt");
			DebugPrint.printString("decrypt buffer: ");
			DebugPrint.printBuffer(DecryptedBuffer);

			ArrayUtils.copyByteArray(DecryptedBuffer, 0, Times, 0, 4);
			limitation = TypeConverter.bytesToInt(Times, 0);
			DebugPrint.printString("limit:");

			DebugPrint.printInt(limitation);
			ArrayUtils.copyByteArray(DecryptedBuffer, 4, Times, 0, 4);
			// get image Id
			imageId = TypeConverter.bytesToInt(Times, 0);
			DebugPrint.printString("image id:");
			DebugPrint.printInt(imageId);
			// get the times the picture was viewed, the image Id is the key in
			// the map
			Object key = new Integer(imageId);
			Integer timesViewed = (Integer) map.get(key);
			if (timesViewed == null) {
				// this is a new picture, there is still no key and value for it
				timesViewed = new Integer(0);
			}
			// check that the times were not exceeded
			if (timesViewed.intValue() >= limitation) {
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
		} catch (Exception ex) {
			byte[] errorResponse = ("Error: failed to decrypt symetric key - " + ex.getMessage()).getBytes();
			setResponse(errorResponse, 0, errorResponse.length);
			DebugPrint.printString("Error: failed to decrypt symetric key\n" + ex.getMessage() + ex.toString());
			return status;
		}

		DebugPrint.printString("Decrypting symetric key Succeeded!");
		return status;
	}

	private int generatePavpKey(byte[] commandData) {
		int status = IntelApplet.APPLET_ERROR_GENERIC;
		DebugPrint.printString("Generating PAVP key...");

		try {
			// validate the data was loaded
			if (!dataLoaded) {
				status = ERROR_REPLAY;
				throw new Exception("Data replay.");
			}
			// get the times the picture was viewed
			Object key = new Integer(imageId);
			Integer timesViewed = (Integer) map.get(key);
			if (timesViewed == null) {
				// this is a new picture, there is still no key and value for it
				timesViewed = new Integer(0);
			}
			// check that the times were not exceeded
			if (timesViewed.intValue() >= limitation) {
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

			byte response[] = new byte[16];
			// Create the session key
			// commented for fit amulet, uncomment for hardware
			ProtectedOutput pOutput = ProtectedOutput.getInstance(slotHandle, DecryptedSymmetricKey, (short) 0,
					(short) 16);
			// retrieve encrypted key record to be provided to GFX driver for key injection
			pOutput.getEncryptedKeyRecord(response, (short) 0); // we get (S1)Kb (s1 encrypted with Kb)

			// workaround to fit amulet TODO: delete or use host+server workaround
			// ArrayUtils.copyByteArray(DecryptedSymmetricKey, 0, response, 0, 16);
			DebugPrint.printString("decrypted key in generate: ");
			DebugPrint.printBuffer(DecryptedSymmetricKey);
			DebugPrint.printString("encrypted key from gfx: ");
			DebugPrint.printBuffer(response);
			DebugPrint.printString("if encrypted key = decrypted, Workaround for amulet, delete on hardware");
			setResponse(response, 0, response.length);

			status = IntelApplet.APPLET_SUCCESS;
		} catch (Exception ex) {
			byte[] errorResponse = ("Error: failed to generate PAVP session key - " + ex.getMessage()).getBytes();
			setResponse(errorResponse, 0, errorResponse.length);
			DebugPrint.printString("Error: failed to generate PAVP Session key\n" + ex.getMessage());
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
		if (MTC.getMTC() == 0) {
			// first usage of this Trusted Application, no data to load
			dataLoaded = true;
		} else {
			// there is data to load
			dataLoaded = false;
		}
		return APPLET_SUCCESS;
	}

	boolean isSigmaCommand(int commandId) {
		return commandId >= 1 && commandId <= 3;
	}

	/**
	 * This method will be called by the VM to handle a command sent to this Trusted
	 * Application instance.
	 * 
	 * @param commandId the command ID (Trusted Application specific)
	 * @param request   the input data for this command
	 * @return the return value should not be used by the applet
	 */
	public int invokeCommand(int commandId, byte[] request) {
		
		// Reset the reply buffer and result code
		_sigmaReplyBuffer = null;
		int result = APPLET_SUCCESS;

		// Check what is the wanted action
		DebugPrint.printString("command:" + Integer.toString(commandId));

		if (isSigmaCommand(commandId)) {
			if (commandId == CMD_INIT_SIGMA) {
				sigma = new Sigma();
			}
			DebugPrint.printString("command after the if:" + Integer.toString(commandId));
			result = sigma.handleCommand(commandId, request);
			_sigmaReplyBuffer = sigma._sigmaReplyBuffer;
		} else {
			switch (commandId) {
			case CMD_GET_AUTHENTICATION_ID: // added this for authentication stage:
			{
				wysIns = new WYS();
				wysIns.onInit(request);
				
				DebugPrint.printString("in-aut id");
				result = GetAuthenticationId(request);
				break;
			}
			
			case CMD_WYS_STANDARD:
			{
				wysIns = new WYS();
				wysIns.onInit(request);
				result = wysIns.invokeCommand(commandId, request);
				
				_sigmaReplyBuffer = new byte[wysIns.getResponseSize()];
				wysIns.getResponse(_sigmaReplyBuffer, 0);
				DebugPrint.printString("reply buffer:");
				DebugPrint.printBuffer(_sigmaReplyBuffer);
				break;
			}

			case CMD_DECRYPT_SYMETRIC_KEY: {
				result = decryptSymetricKey(request);
				break;
			}
			case CMD_GENERATE_PAVP_SESSION_KEY:
				result = generatePavpKey(request);
				break;

			default: {
				result = UNRECOGNIZED_COMMAND;
				break;
			}
			
			}
		}

		/*
		 * To return the response data to the command, call the setResponse method
		 * before returning from this method. Note that calling this method more than
		 * once will reset the response data previously set.
		 */

		/*
		 * In order to provide a return value for the command, which will be delivered
		 * to the SW application communicating with the Trusted Application,
		 * setResponseCode method should be called. Note that calling this method more
		 * than once will reset the code previously set. If not set, the default
		 * response code that will be returned to SW application is 0.
		 */
		setResponseCode(result);
		if (_sigmaReplyBuffer != null)
			setResponse(_sigmaReplyBuffer, ZERO_INDEX, _sigmaReplyBuffer.length);

		/*
		 * The return value of the invokeCommand method is not guaranteed to be
		 * delivered to the SW application, and therefore should not be used for this
		 * purpose. Trusted Application is expected to return APPLET_SUCCESS code from
		 * this method and use the setResposeCode method instead.
		 */
		return APPLET_SUCCESS;
	}

	


	/**
	 * This method will be called by the VM when the session being handled by this
	 * Trusted Application instance is being closed and this Trusted Application
	 * instance is about to be removed. This method cannot provide response data and
	 * therefore calling setResponse or setResponseCode methods from it will throw a
	 * NullPointerException.
	 * 
	 * @return APPLET_SUCCESS code (the status code is not used by the VM).
	 */
	public int onClose() {
		DebugPrint.printString("Goodbye, DAL!");
		return APPLET_SUCCESS;
	}
}




