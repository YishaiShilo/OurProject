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
using System.Net.Sockets;


using System.Windows.Forms;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Drawing;

namespace DALSamplesServer
{
    public class ProtectedOutputHandler
    {
        uint imageId;
        private bool EPIDReceived;
        private int groupID;
        private byte[] symmetric_key;
        private byte[] encrypted_image;
        private byte[] mod;
        private byte[] exponent;  
        private byte[] signed_mod;
        private byte[] signed_exponent;
        private byte[] nonce;
        private const int EXIT = 1;
        private const int SENDING_KEYS = 2;
        private const int REQUESTING_IMAGE = 3;
        private const int SENDING_GROUP_ID = 4;

        private bool clientConnected;
        const int EPID_NONCE_LEN = 32;
        const int EPID_SIGNATURE_LEN = 569;
        const int TA_UUID_LEN = 16;


        public ProtectedOutputHandler(byte[] symmetric_key, byte[] encrypted_image, uint clientId)
        {
            this.symmetric_key = symmetric_key;
            this.encrypted_image = encrypted_image;
            this.imageId = clientId;
        }

        public void handleClientComm(object client)
        {
            try
            {
                TcpClient tcpClient = (TcpClient)client;
                Socket socket = tcpClient.Client;
                clientConnected = socket.Connected;
                while (clientConnected)
                {         
                    byte[] cmd = new byte[4];
                    //get the command id
                    socket.Receive(cmd, 0, 4, 0);
                    //parse the command id sent by the client
                    int command = BitConverter.ToInt32(cmd, 0);
                    switch (command)
                    {
                        case SENDING_GROUP_ID:
                            {
                                Console.WriteLine("Recieving group id...");
                                getGroupID(socket);
                                Console.WriteLine("Finished group id.");
                            }
                            break;
                        case SENDING_KEYS:
                            {
                                Console.WriteLine("Recieving key...");
                                getKeys(socket);
                                Console.WriteLine("Finished recieving key.");
                            }
                            break;
                        case REQUESTING_IMAGE:
                            {
                                Console.WriteLine("Sending image...");
                                sendImage(socket);
                                Console.WriteLine("Finished sending image...");
                            }
                            break;
                        case EXIT:
                            //client disconnecting, break the loop
                            break;
                    }
                    //check connection status
                    clientConnected = socket.Connected;  
                    //if client sent EXIT command - break the loop and end this client communication
                    if (command == EXIT)
                        break;                         
                }
                Console.WriteLine("Protected Output Sample Client disconnected.\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void getGroupID(Socket socket)
        {
            //Receive EPID group ID from client
            byte[] groupIDByteArray = new byte[4];
            socket.Receive(groupIDByteArray, 0, 4, 0);
            groupID = Utils.ByteArrayToInt(groupIDByteArray);
            EPIDReceived = true;
        }

        private void getKeys(Socket socket)
        {
            byte[] datasize = new byte[4];

            //recieve from client the size of the keys buffer
            int recv = socket.Receive(datasize, 0, 4, 0);
            int size = BitConverter.ToInt32(datasize, 0);

            int total = 0;
            int dataleft = size;
            byte[] keyData = new byte[size];

            //read the key buffer in chuncks
            while (total < size)
            {
                recv = socket.Receive(keyData, total, dataleft, 0);
                if (recv == 0)
                {
                    break;
                }
                total += recv;
                dataleft -= recv;
            }

            exponent = new byte[4];
            mod = new byte[256];
            signed_mod = new byte[EPID_SIGNATURE_LEN];
            signed_exponent = new byte[EPID_SIGNATURE_LEN];
            nonce = new byte[EPID_NONCE_LEN];

            //init the mod and exponent buffers
            Buffer.BlockCopy(keyData, 0, exponent, 0, 4);
            Buffer.BlockCopy(keyData, 4, mod, 0, 256);
            Buffer.BlockCopy(keyData, 260, signed_exponent, 0, EPID_SIGNATURE_LEN);
            Buffer.BlockCopy(keyData, 260 + EPID_SIGNATURE_LEN, signed_mod, 0, EPID_SIGNATURE_LEN);
            Buffer.BlockCopy(keyData, 260 + EPID_SIGNATURE_LEN * 2, nonce, 0, EPID_NONCE_LEN);

            //validat we received the EPID group ID
            if (!EPIDReceived)
                throw new Exception("Invaild keys. Aborting client.");
            //validate that the key was signed by the TA
            if (!validateKeySignature())
                throw new Exception("Invaild keys. Aborting client.");
        }

        private bool validateKeySignature()
        {
            //ta ID
            String origTaId = "d144fc9a4a4f4cf3a467cab63fe40adc";
            Guid guid = new Guid(origTaId);
            //convert TA id to bytes
            byte[] taId = guid.ToByteArray();

            byte[] zeros = new byte[TA_UUID_LEN];
            byte[] adaptedMessage = new byte[taId.Length + zeros.Length + nonce.Length];
            taId.CopyTo(adaptedMessage, 0);
            zeros.CopyTo(adaptedMessage, taId.Length);
            nonce.CopyTo(adaptedMessage, taId.Length + zeros.Length);
            //validate exponent signature
            bool res = validateExponent(adaptedMessage);
            //validate mod signature
            if(res)
                res = validateMod(adaptedMessage);
            return res;
        }

        private bool validateMod(byte[] commonAdaptedMessage)
        {
            //complete the adtapted message with the mod
            byte[] adaptedMessage = new byte[commonAdaptedMessage.Length + mod.Length];
            commonAdaptedMessage.CopyTo(adaptedMessage, 0);
            mod.CopyTo(adaptedMessage, commonAdaptedMessage.Length);
            return validateSignature(adaptedMessage,signed_mod);        
        }

        private bool validateSignature(byte[] adaptedMessage, byte[] signature)
        {
            //groupCert contains the SIGMA1_0 certificate for the specific EPID group ID
            byte[] groupCert = Utils.GetSpecificEpidCertificate_SIGMA_1_0((uint)groupID);
            //epidParamsCert contains the mathematic parameters
            byte[] epidParamsCert = File.ReadAllBytes(DataStructs.DEBUG_SIGNED_BIN_PARAMS_CERT_FILE);

            // taskInfoArray is a data structure defined in the DAL implementation. It is prepended to the message by DAL prior to signing,
            // and so has to be prepened by us prior to verification
            byte[] taskInfoArray = Utils.StructureToByteArray(DataStructs.GetTaskInfo());
            byte[] infoNonceMessage = new byte[taskInfoArray.Length + adaptedMessage.Length]; //TaskInfo || Nonce || Message


            taskInfoArray.CopyTo(infoNonceMessage, 0);//concatenate the info to infoNonceMessage
            adaptedMessage.CopyTo(infoNonceMessage, taskInfoArray.Length);//concatenate the adapted message(including the nonce) to infoNonceMessage

            //Verify the signature
            //When we call the MessageVerifyPch function, we can send
            //baseName - the basename that will be signed as part of the signature
            //privateKeyRevList - list of the platforms that were revoked based on the platform’s private key
            //SignatureRevList - list of the platforms that were revoked based on the signature generated by the platform
            //GroupRevList - list of the EPID groups that were revoked based on the group public key
            //as parameters.
            CdgResult retStatus;
            CdgStatus status = CryptoDataGenWrapper_1_1.MessageVerifyPch(groupCert, groupCert.Length, epidParamsCert, infoNonceMessage, infoNonceMessage.Length, null, 0, signature, signature.Length, out retStatus, null, null, null);

            if (status != CdgStatus.CdgStsOk)
                return false;
            else
                if (retStatus != CdgResult.CdgValid)
                    return false;
                else
                    return true;
        }

        private bool validateExponent(byte[] commonAdaptedMessage)
        {
            //complete the adapted message with the exponent
            byte[] adaptedMessage = new byte[commonAdaptedMessage.Length + exponent.Length];
            commonAdaptedMessage.CopyTo(adaptedMessage, 0);
            exponent.CopyTo(adaptedMessage, commonAdaptedMessage.Length);
            return validateSignature(adaptedMessage, signed_exponent);   
        }

        private void sendImage(Socket socket)
        {
            byte[] data = generateData();
            int total = 0;
            int size = data.Length;
            int dataleft = size;
            int sent;

            byte[] datasize = new byte[4];
            datasize = BitConverter.GetBytes(size);
            //dend the client the size of the data buffer
            sent = socket.Send(datasize);

            //send the data in chuncks
            while (total < size)
            {
                sent = socket.Send(data, total, dataleft, SocketFlags.None);
                total += sent;
                dataleft -= sent;
            }    
        }

        private byte[] generateData()
        {
            uint timesToSee = 3;
            byte[] keyAndTimes = new byte[symmetric_key.Length + 8];
            //first four bytes store the limitation of times viewing the image
            keyAndTimes[0] = (byte)(timesToSee >> 3 * 8);
            keyAndTimes[1] = (byte)((timesToSee << 1 * 8) >> 3 * 8);
            keyAndTimes[2] = (byte)((timesToSee << 2 * 8) >> 3 * 8);
            keyAndTimes[3] = (byte)((timesToSee << 3 * 8) >> 3 * 8);
            //second four bytes store the image id, should be unique
            keyAndTimes[4] = (byte)(imageId >> 3 * 8);
            keyAndTimes[5] = (byte)((imageId << 1 * 8) >> 3 * 8);
            keyAndTimes[6] = (byte)((imageId << 2 * 8) >> 3 * 8);
            keyAndTimes[7] = (byte)((imageId << 3 * 8) >> 3 * 8);
            
            //copy the symetric key after the limitation and image id
            symmetric_key.CopyTo(keyAndTimes, 8);
            
            //encrypt the data using the client public key
            byte[] encrypted_symetric_key = encryptRSA(mod, exponent, keyAndTimes);

            byte[] data = new byte[sizeof(uint) * 3/*width,height,symmetricKeyLength 32 bit each*/+ encrypted_symetric_key.Length + sizeof(uint)/*encryptedBitmapSize*/+ encrypted_image.Length];

            uint width = 960;
            uint height = 720;
            uint symmetricKeyLength = (uint)encrypted_symetric_key.Length;
            uint encryptedBitmapSize = (uint)encrypted_image.Length;

            //copy the width of the image
            copy(width, data, 0);
            //copy the height of the image
            copy(height, data, 4);
            //copy the length of the encrypted key buffer
            copy(symmetricKeyLength, data, 8);
            //copy the encrypted key buffer
            encrypted_symetric_key.CopyTo(data, 12);
            //copy the size of the encrypted bitmap
            copy(encryptedBitmapSize, data, 12 + encrypted_symetric_key.Length);
            //copy the encrypted image
            encrypted_image.CopyTo(data, 16 + encrypted_symetric_key.Length);

            Console.WriteLine("Image Id: "+imageId);

            return data;

        }

        //copies the integer to the byte array
        public static void copy(uint integer, byte[] arr, int offset)
        {
            var v = BitConverter.GetBytes(integer);
            arr[offset] = v[0];
            arr[offset + 1] = v[1];
            arr[offset + 2] = v[2];
            arr[offset + 3] = v[3];
        }

        public static byte[] encryptRSA(byte[] modulus, byte[] exponent, byte[] small_data)
        {
            RSAParameters rsa_params = new RSAParameters();

            rsa_params.Modulus = modulus;
            rsa_params.Exponent = exponent;

            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.ImportParameters(rsa_params);

            return rsa.Encrypt(small_data, false); //use PKCS#1 v1.5 padding
        }

    }
}
