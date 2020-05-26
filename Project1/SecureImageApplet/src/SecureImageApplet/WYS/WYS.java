/*++
   Copyright (c) 2010-2017 Intel Corporation. All Rights Reserved.

   The source code contained or described herein and all documents related
   to the source code ("Material") are owned by Intel Corporation or its
   suppliers or licensors. Title to the Material remains with Intel Corporation
   or its suppliers and licensors. The Material contains trade secrets and
   proprietary and confidential information of Intel or its suppliers and
   licensors. The Material is protected by worldwide copyright and trade secret
   laws and treaty provisions. No part of the Material may be used, copied,
   reproduced, modified, published, uploaded, posted, transmitted, distributed, 
   or disclosed in any way without Intel's prior express written permission.

   No license under any patent, copyright, trade secret or other intellectual
   property right is granted to or conferred upon you by disclosure or delivery
   of the Materials, either expressly, by implication, inducement, estoppel or
   otherwise. Any license under such intellectual property rights must be
   express and approved by Intel in writing.
--*/
package SecureImageApplet.WYS;


import com.intel.crypto.Random;
import com.intel.langutil.ArrayUtils;
import com.intel.langutil.TypeConverter;
import com.intel.util.IntelApplet;
import com.intel.util.DebugPrint;

//
// Implementation of DAL Trusted Application: WYSApplet 
//
// ***************************************
// NOTE:  This Trusted Application is intended for API Level 2 and above
// ***************************************

public class WYS 
{
	static final int COMMAND_ID_CHECK_INPUT_STATUS = 1;
	static final int COMMAND_ID_GET_OTP = 2;
    
	public static final int APPLET_SUCCESS = 0;
		
	private StandardWindow m_standardWindow;
	
	private final byte[] PIN = {1, 7, 1, 7};
	
	/*
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
	public int onInit(byte[] request) {
		DebugPrint.printString("WYS applet entered");

		m_standardWindow = StandardWindow.getInstance();
		
		return APPLET_SUCCESS;
	}
	
	public WYS()
	{
		m_standardWindow = StandardWindow.getInstance();
	}
	
	/*
	 * This method will be called by the VM to handle a command sent to this
	 * Trusted Application instance.
	 * 
	 * @param	commandID	the command ID (Trusted Application specific) 
	 * @param	request		the input data for this command 
	 * @return	the return value should not be used by the applet
	 */
	public byte[] invokeCommand(int commandID, byte[] request) {
		int res = IntelApplet.APPLET_ERROR_NOT_SUPPORTED;
		
		switch (commandID)
		{
			case StandardWindow.STANDARD_COMMAND_ID:
				
				DebugPrint.printString("Processing standard command...");
				res = m_standardWindow.processCommand(commandID, request, 0);
				if ( (res == IntelApplet.APPLET_SUCCESS) &&
					 (m_standardWindow.getResponseSize() > 0) )
				{
					byte[] response = new byte[m_standardWindow.getResponseSize()];
					m_standardWindow.getResponse(response, 0);
					return response;
				}
				break;
				
			case COMMAND_ID_CHECK_INPUT_STATUS:
				
				DebugPrint.printString("Checking user input status...");
				if ( isOtpAllowed() )
				{
					DebugPrint.printString("User input is OK");
					res = IntelApplet.APPLET_SUCCESS;
				}
				else
				{
					DebugPrint.printString("User input is wrong");
					res = IntelApplet.APPLET_ERROR_BAD_PARAMETERS;
				}
				break;
			
			case COMMAND_ID_GET_OTP:
				
				DebugPrint.printString("Getting OTP...");
				res = getOtp(); 
				break;
			
			default:
				break;
		}
		return null;
		
		//setResponseCode(res);
		
		/*
		 * The return value of the invokeCommand method is not guaranteed to be
		 * delivered to the SW application, and therefore should not be used for
		 * this purpose. Trusted Application is expected to return APPLET_SUCCESS code 
		 * from this method and use the setResposeCode method instead.
		 */
//		return APPLET_SUCCESS;
	}
	
	private int getOtp()
	{
		if ( !isOtpAllowed() )
		{
			DebugPrint.printString("OTP is blocked.");
			return IntelApplet.APPLET_ERROR_BAD_STATE;
		}
		
		byte[] otp = new byte[TypeConverter.INT_BYTE_SIZE];
		Random.getRandomBytes(otp, (short)0, (short)otp.length);
		
		setResponse(otp, 0, otp.length);
		
		return IntelApplet.APPLET_SUCCESS;
	}

	private boolean isOtpAllowed()
	{
		if ( m_standardWindow.getUserInputStatus() == true )
		{
			return true;
		}
		
		// try to check PIN number
		byte[] userPIN = m_standardWindow.getPin();
		
		if ( (userPIN != null) && (userPIN.length == PIN.length) && (ArrayUtils.compareByteArray(userPIN, 0, PIN, 0, PIN.length)) )
		{
			return true;
		}
		
		return false;
	}
	
	/*
	 * This method will be called by the VM when the session being handled by
	 * this Trusted Application instance is being closed 
	 * and this Trusted Application instance is about to be removed.
	 * This method cannot provide response data and therefore
	 * calling setResponse or setResponseCode methods from it will throw a NullPointerException.
	 * 
	 * @return APPLET_SUCCESS code (the status code is not used by the VM).
	 */
	public int onClose() {
		DebugPrint.printString("WYS applet exited");
		return APPLET_SUCCESS;
	}
}