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
using System.Threading;
using System.Net;

using System.Windows.Forms;
using System.IO;
using System.Security.Cryptography;
using System.Drawing;

namespace DALSamplesServer
{
    class Server
    {
        private TcpListener tcpListener;
        private Thread listenThread;
        private uint clientId;
        public byte[] symmetric_key;
        public byte[] encrypted_image;

        private const int PROTECTED_OUTPUT_SAMPLE = 1;
        private const int EPID_PROVISIONING_SAMPLE = 2;
        private const int EPID_SIGNING_SAMPLE = 3;
        private const int SIGMA_SAMPLE = 4;
        public Server()
        {
            //init Protected Output Sample data
            initProtectedOutputData();
            //start listening for clients
            tcpListener = new TcpListener(IPAddress.Any, 27015);
            listenThread = new Thread(new ThreadStart(listenForClients));
            listenThread.Start();
        }

        private void initProtectedOutputData()
        {
            //generate symetric key, if not exist
            if (symmetric_key == null)
                symmetric_key = generateAESkey();
            //encrypt the bitmap with the symetric key
            encrypted_image = encryptImage();
            clientId = 1;
        }

        public void stop()
        {
            //stop listening for connections
            listenThread.Abort();
            tcpListener.Stop();
        }

        private void listenForClients()
        {
            tcpListener.Start();
            Console.WriteLine("The server is running at port 27015...");
            Console.WriteLine("The local End point is  :" +
                              tcpListener.LocalEndpoint);
            Console.WriteLine("Waiting for a connections....\n");
            while (true)
            {
                //blocks until a client has connected to the server
                TcpClient client = this.tcpListener.AcceptTcpClient();
                Console.WriteLine("\nConnection accepted from " + client.Client.LocalEndPoint);

                //client should send an integer specifying which kind of client it is
                byte[] cmd = new byte[4];
                client.Client.Receive(cmd, 0, 4, 0);
                int sampleType = BitConverter.ToInt32(cmd, 0);
                switch (sampleType)
                {
                    case PROTECTED_OUTPUT_SAMPLE:
                        {
                            Console.WriteLine("Connected client is Protected Output Sample");
                            //create a thread to handle communication 
                            //with connected client
                            ProtectedOutputHandler ch = new ProtectedOutputHandler(symmetric_key, encrypted_image,clientId);
                            Thread clientThread = new Thread(new ParameterizedThreadStart(ch.handleClientComm));
                            clientThread.Start(client);
                            //increment the client id
                            clientId++;
                        }break;
                    case EPID_PROVISIONING_SAMPLE:
                        {
                            Console.WriteLine("Connected client is EPID Provisioning Sample");
                            //create a thread to handle communication 
                            //with connected client
                            EPIDProvisioningHandler ch = new EPIDProvisioningHandler();
                            Thread clientThread = new Thread(new ParameterizedThreadStart(ch.handleClientComm));
                            clientThread.Start(client);
                        } break;
                    case EPID_SIGNING_SAMPLE:
                        {
                            Console.WriteLine("Connected client is EPID Signing Sample");
                            //create a thread to handle communication 
                            //with connected client
                            EPIDSigningHandler ch = new EPIDSigningHandler();
                            Thread clientThread = new Thread(new ParameterizedThreadStart(ch.handleClientComm));
                            clientThread.Start(client);
                        } break;
						case SIGMA_SAMPLE:
                        {
                            Console.WriteLine("Connected client is SIGMA Sample");
                            //create a thread to handle communication 
                            //with connected client
                            SIGMAHandler ch = new SIGMAHandler();
                            Thread clientThread = new Thread(new ParameterizedThreadStart(ch.handleClientComm));
                            clientThread.Start(client);
                        } break;
                    
                    default: break;
                }
            }       
        }
        
        public static byte[] generateAESkey()
        {
            //generate AES key
            RijndaelManaged aes = new RijndaelManaged();
            aes.Mode = CipherMode.ECB;
            aes.KeySize = 128;
            aes.GenerateKey();
            return aes.Key;
        }

        private byte[] encryptImage()
        {
            string path = @"..\..\Images\ImageToEncrypt.bmp";
            //the image should be fliped
            flip24BitImage(path);
            byte[] bitstream = getFileBytes(path);
            //reflip the image for future use
            flip24BitImage(path);
            //convert the image to XRGB
            byte[] xrgbStream = convertToXRGB(bitstream);
            bitstream = xrgbStream;
            //encrypt the bitmap using the symetric key
            //return ProtectedOutputHostWrapper.encryptBitmap(bitstream, symmetric_key); TODO
            return bitstream;
        }
        
        private byte[] convertToXRGB(byte[] bitstream)
        {
            //the new size is the original size + padding of one byte per RGB block
            int xrgbSize = bitstream.Length + ((bitstream.Length - 54) / 3);

            byte[] xrgb = new byte[xrgbSize];

            //copy the first 54 bytes, which are the header of the bitmap.
            Array.Copy(bitstream, xrgb, 54);

            //The image is expected to be a 24 bit bitmap,we are converting it to 32
            //the 28th byte represents the number of bits per pixel.
            xrgb[28] = (byte)0x20; 

            int xrgbPos = 54;
            int i = 54;
            while (i + 2 < bitstream.Length)
            {
                //copy the RGB
                xrgb[xrgbPos] = bitstream[i];         // R
                xrgb[xrgbPos + 1] = bitstream[i + 1]; // G
                xrgb[xrgbPos + 2] = bitstream[i + 2]; // B
                //Add the Alpha channel
                xrgb[xrgbPos + 3] = 0;                // X

                //the 32-bit bitmap - 4 bytes per pixel
                xrgbPos += 4;
                //the 24-bit bitmap - 3 bytes per pixel
                i += 3;
            }

            //return the 32-bit bitmap bytes
            return xrgb;
        }

        private void flip24BitImage(string filename)
        {
            Image image = Bitmap.FromFile(filename);

            image.RotateFlip(RotateFlipType.RotateNoneFlipY);

            image.Save(filename, System.Drawing.Imaging.ImageFormat.Bmp);

        }

        public static byte[] getFileBytes(string filename)
        {
            const int buffersize = 1024;
            byte[] readBuff = new byte[buffersize];
            int count;
            int bytesWritten = 0;

            // read the file 
            FileStream stream = new FileStream(filename, FileMode.Open);
            
            BinaryReader reader = new BinaryReader(stream);
            MemoryStream mstream = new MemoryStream();

            do
            {
                count = reader.Read(readBuff, 0, buffersize);
                bytesWritten += count;
                mstream.Write(readBuff, 0, count);
            }
            while (count > 0);


            byte[] file_data = new byte[bytesWritten];

            mstream.Position = 0;
            mstream.Read(file_data, 0, bytesWritten);

            mstream.Close();
            reader.Close();
            stream.Close();

            return file_data;
        }
    }
}
