/**
***
*** Copyright  (C) 2013-2014 Intel Corporation. All rights reserved.
***
*** The information and source code contained herein is the exclusive
*** property of Intel Corporation. and may not be disclosed, examined
*** or reproduced in whole or in part without explicit written authorization
*** from the company.
***
*** ----------------------------------------------------------------------------
**/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;

namespace WysHost
{
    public partial class WysForm : Form
    {
        public byte[] pin;
        public WysForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets the selected WYS image type from the GUI radio buttons.
        /// </summary>
        /// <returns>One of the LogicInterface.WYS_IMAGE_TYPE_ constants.</returns>
        private Byte getWysImageType()
        {
           return WysWrapper.WYS_IMAGE_TYPE_PINPAD;
        }

        /// <summary>
        /// Performs initialization of the JHI/WYS libraries and renders 
        /// the WYS image using the selected radio button as the image type.
        /// </summary>
        private void button_initAndRender_Click(object sender, EventArgs e)
        {
            //Grey out the GUI:
            

            UInt32 ret = WysWrapper.doWysSequence(panel_wysView.Handle, getWysImageType());
            alertOnFailure(ret, WysWrapper.SAMPLE_CODE_SUCCESS, "WYS", "WYS operations failed");
        }

        /// <summary>
        /// Handles mouse-down events on the WYS image area.
        /// </summary>
        private void panel_wysView_MouseDown(object sender, MouseEventArgs e)
        {
            UInt16 x = (UInt16)e.X;
            UInt16 y = (UInt16)e.Y;
            bool ret = WysWrapper.onMouseDown(panel_wysView.Handle, x, y);
        }

        /// <summary>
        /// Handles mouse-up events on the WYS image area.
        /// </summary>
        private void panel_wysView_MouseUp(object sender, MouseEventArgs e)
        {
            UInt16 x = (UInt16)e.X;
            UInt16 y = (UInt16)e.Y;
            bool ret = WysWrapper.onMouseUp(x, y);
        }

        /// <summary>
        /// Handles a "submit" request. The request will be passed on to the WYS library.
        /// Exact behavior depends on the selected WYS image type.
        /// </summary>
        private void button_submit_Click(object sender, EventArgs e)
        {
            bool ret;
           
            //To be called on "submit" events. Not to be used in CAPTCHA mode.
            ret = WysWrapper.onClickSubmit(null, 0);
            
            //const int otpLength = 4;
            //byte[] otpBytes = new byte[otpLength];
            //IntPtr outArr = Marshal.AllocHGlobal(otpLength);
            //if (WysWrapper.getOtp(outArr, otpLength))
            //{
            //    Marshal.Copy(outArr, otpBytes, 0, otpLength);
            //    Marshal.FreeHGlobal(outArr);

            //    string otp = BitConverter.ToString(otpBytes).Replace("-", "");
            //    MessageBox.Show(string.Format("Success! The generated OTP is: {0}", otp));
            //}
            //else
            //{
            //    MessageBox.Show("Failed to verify user input.");
            //}

           
            radioButton_pinPad.Visible = true;
            radioButton_pinPad.Checked = true;
#if AMULET
            radioButton_pinPad.Enabled = true;         
#else
            radioButton_pinPad.Enabled = false;
            button_clear.Enabled = false;
            button_initAndRender.Enabled = false;
            button_submit.Enabled = false;
#endif
        }

        /// <summary>
        /// Handles a "clear" request. The request will be passed on to the WYS library.
        /// Exact behavior depends on the selected WYS image type.
        /// </summary>
        private void button_clear_Click(object sender, EventArgs e)
        {
            if (getWysImageType() == WysWrapper.WYS_IMAGE_TYPE_CAPTCHA)
            {
                MessageBox.Show("This operation is not supported in CAPTCHA mode.");
            }
            else
            {
                bool ret = WysWrapper.onClickClear();
                String msg;

                if (!ret)
                {
                    msg = "Success!";
                }
                else
                {
                    msg = "Failed!";
                }
                MessageBox.Show(msg);
            }
        }

        /// <summary>
        /// Called when the form is closing. De-inits the libraries used.
        /// </summary>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {

            this.Hide();
            //WysWrapper.Close();
            //Application.Exit();
        }

        /// <summary>
        /// Checks if errorCode is not successErrorCodeToCompareTo. If they are different, 
        /// a message box is displayed, with the text "Error: "+errorDescription+"!\n"+errorProviderName+" code = 0x[errorCode, in hex]".
        /// </summary>
        private void alertOnFailure(UInt32 errorCode, UInt32 successErrorCodeToCompareTo, string errorProviderName, string errorDescription)
        {
            if (errorCode != successErrorCodeToCompareTo)
            {
                MessageBox.Show(String.Format("Error: " + errorDescription + "!\n" + errorProviderName + " code = 0x{0:X}", errorCode));
            }
        }

        private void button_send_Click(object sender, EventArgs e)
        {
            const int pinLength = 16; // encrypted, block size minimum 16B (128 bits)
            pin = new byte[pinLength];
            IntPtr outArr = Marshal.AllocHGlobal(pinLength);
            if (WysWrapper.getPin(outArr, pinLength))
            {
                Marshal.Copy(outArr, pin, 0, pinLength);
                Marshal.FreeHGlobal(outArr);

                Console.Write("encrypted pin: ");
                Console.WriteLine(pin);
            }
            else
            {
                MessageBox.Show("Failed to get encrypted pin.");
            }
        }
    }
}
