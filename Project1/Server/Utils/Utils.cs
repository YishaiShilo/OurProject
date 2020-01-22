/**
***
*** Copyright  (C) 2014 Intel Corporation. All rights reserved.
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
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace DALSamplesServer
{
    class Utils
    {

        //Returns a byte array containing all EPID certificates for SIGMA 1_0
        public static byte[] GetAllEpidCerts_SIGMA_1_0()
        {
            //concatenate all the SIGMA 1_0 certificates to certsList
            List<byte> certsList = new List<byte>();
            for (int i = 0; i < DataStructs.ALL_EPID_CERTS_1_0.Length; i++)
            {
                byte[] currentCert = File.ReadAllBytes(DataStructs.ALL_EPID_CERTS_1_0[i]);
                for (int j = 0; j < currentCert.Length; j++)
                {
                    certsList.Add(currentCert[j]);
                }
            }
            return certsList.ToArray();
        }

        //Returns a byte array containing all EPID certificates for SIGMA 1_1
        public static byte[] GetAllEpidCerts_SIGMA_1_1()
        {
            //concatenate all the SIGMA 1_1 certificates to certsList
            List<byte> certsList = new List<byte>();
            for (int i = 0; i < DataStructs.ALL_EPID_CERTS_1_1.Length; i++)
            {
                byte[] currentCert = File.ReadAllBytes(DataStructs.ALL_EPID_CERTS_1_1[i]);
                for (int j = 0; j < currentCert.Length; j++)
                {
                    certsList.Add(currentCert[j]);
                }
            }
            return certsList.ToArray();
        }


        //Returns a byte array containing the SIGMA 1_0 certificate for the specified group ID 
        public static byte[] GetSpecificEpidCertificate_SIGMA_1_0(uint groupID)
        {
            byte[] groupIDByteArray = BitConverter.GetBytes(groupID);
            byte[] certificatesBytes;

            //Get all EPID certificates' bytes for SIGMA 1_0
            certificatesBytes = GetAllEpidCerts_SIGMA_1_0();
            //Certificates array
            byte[][] certificates = new byte[certificatesBytes.Length / DataStructs.EPID_CERT_LEN][];
            //Reverse the group ID
            byte[] reversedGroupID = new byte[groupIDByteArray.Length];
            for (int i = 0; i < groupIDByteArray.Length; i++)
            {
                reversedGroupID[i] = groupIDByteArray[groupIDByteArray.Length - i - 1];
            }
            //Search for the certificate that fits the current group ID   
            for (int i = 0; i < certificates.Length; i++)
            {
                certificates[i] = new byte[DataStructs.EPID_CERT_LEN];
                Array.Copy(certificatesBytes, i * DataStructs.EPID_CERT_LEN, certificates[i], 0, DataStructs.EPID_CERT_LEN);
                if (Utils.CompareArray(reversedGroupID, 0, certificates[i], DataStructs.EPID_GID_OFFSET_IN_CERT, DataStructs.EPID_GID_LEN))
                    return certificates[i];
            }
            return null;
        }


        //Returns a byte array containing the SIGMA 1_1 certificate for the specified group ID 
        public static byte[] GetSpecificEpidCertificate_SIGMA_1_1(uint groupID)
        {
            int certGroupID = 0;
            //Get all EPID certificates for SIGMA 1_1
            byte[] certificatesBytes = GetAllEpidCerts_SIGMA_1_1();
            int cumulativeCertLen = 0;
            int currCertLen = 0;

            //Search for the certificate that fits the current group ID   
            while (cumulativeCertLen < certificatesBytes.Length)
            {
                //Calculate current cert length
                currCertLen = int.Parse((certificatesBytes[cumulativeCertLen + 2] * 100).ToString(), System.Globalization.NumberStyles.HexNumber) + (certificatesBytes[cumulativeCertLen + 3]) + 4;

                //Copy current cert
                byte[] currCertBytes = new byte[currCertLen];
                Array.Copy(certificatesBytes, cumulativeCertLen, currCertBytes, 0, currCertLen);
                cumulativeCertLen += currCertLen;

                X509Certificate cert = new X509Certificate(currCertBytes);

                //Search the group ID in the certificate
                string subject = cert.Subject.Remove(0, cert.Subject.IndexOf("OU"));
                string[] editedSubject = subject.Remove(subject.IndexOf(',')).Split(' ');
                subject = editedSubject[editedSubject.Length - 1];
                certGroupID = int.Parse(subject);

                //Check whether certificate group ID equals the specified group ID 
                if (groupID == certGroupID)
                    return currCertBytes;
            }
            return null;
        }

        //Convert the received structure into a byte array
        public static byte[] StructureToByteArray(object obj)
        {
            int len = Marshal.SizeOf(obj);
            byte[] byteArray = new byte[len];
            IntPtr ptr = Marshal.AllocHGlobal(len);
            Marshal.StructureToPtr(obj, ptr, true);
            Marshal.Copy(ptr, byteArray, 0, len);
            Marshal.FreeHGlobal(ptr);
            return byteArray;
        }

        //Convert the received byte array into an Int32
        public static int ByteArrayToInt(byte[] bytes)
        {
            return BitConverter.ToInt32(bytes, 0);
        }


        // Compares two arrays; each starts from a different offset
        // Returns true if the arrays are equal in the given range
        public static bool CompareArray(Array firstArray, int firstArrayOffset, Array secondArray, int secondArrayOffset, int lengthToCompare)
        {
            if (firstArray == secondArray)
                return true;
            if (firstArray == null || secondArray == null)
                return false;
            if (firstArray.Length < firstArrayOffset + lengthToCompare)
                return false;
            if (secondArray.Length < secondArrayOffset + lengthToCompare)
                return false;
            for (int i = 0; i < lengthToCompare; i++)
            {
                if (!firstArray.GetValue(firstArrayOffset + i).Equals(secondArray.GetValue(secondArrayOffset + i)))
                    return false;
            }
            return true;
        }


        //Generates VLR
        public static DataStructs.VLRHeader GenerateVLR(DataStructs.VLR_ID_TYPE vlrType, int dataLen)
        {
            DataStructs.VLRHeader vlr;
            vlr.ID = (byte)vlrType;
            vlr.PaddedBytes = (byte)((4 - (DataStructs.VLR_HEADER_LEN + dataLen) % 4) % 4);
            vlr.VLRLength = (short)(DataStructs.VLR_HEADER_LEN + dataLen + vlr.PaddedBytes);
            return vlr;
        }



        //Convert the received byte array into a structure
        public static object ByteArrayToStructure(byte[] bytearray, ref object obj)
        {
            int len = Marshal.SizeOf(obj);
            IntPtr i = Marshal.AllocHGlobal(len);
            Marshal.Copy(bytearray, 0, i, len);
            obj = Marshal.PtrToStructure(i, obj.GetType());
            Marshal.FreeHGlobal(i);
            return obj;

        }

        //Check whther BK exists in the signed message
        public static byte[] GetBKValuesFromSignedMessage(byte[] message)
        {
            if (message.Length < DataStructs.SIGRL_BK_SIZE + 4)
                return null;

            byte[] BKValue = new byte[DataStructs.SIGRL_BK_SIZE];

            Buffer.BlockCopy(message, 4, BKValue, 0, DataStructs.SIGRL_BK_SIZE);

            return BKValue;
        }


    }
}
