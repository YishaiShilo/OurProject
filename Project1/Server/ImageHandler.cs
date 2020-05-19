using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Drawing;

namespace DALSamplesServer
{
    class ImageHandler
    {
        private const int STATUS_SUCCEEDED = 0;
        private const int STATUS_FAILED = -1;

        private const int REQUESTING_IMAGE = 3;


        private bool clientConnected;



        private byte[] symmetric_key;
        private byte[] encrypted_image;
        private Socket socket;
        private Encryption encryption;
        private Authentication autHandelr;
        uint imageId = 0;

        //property

        public ImageHandler(Encryption encrytionHandler, Authentication autHandler)
        {
            encryption = encrytionHandler;
        }

        private byte[] encryptImage(string path)
        {

            flip24BitImage(path);
            byte[] bitstream = getFileBytes(path);
            //reflip the image for future use
            flip24BitImage(path);
            //convert the image to XRGB
            byte[] xrgbStream = convertToXRGB(bitstream);
            bitstream = xrgbStream;
            //encrypt the bitmap using the symetric key

            symmetric_key = generateAESkey();
            // change for decryption problem, TODO: delete
            Console.WriteLine("Key for image: " + BitConverter.ToString(symmetric_key));
            return ProtectedOutputHostWrapper.encryptBitmap(bitstream, symmetric_key);
        }

        public static byte[] generateAESkey()
        {
            //generate AES key
            RijndaelManaged aes = new RijndaelManaged();
            aes.Mode = CipherMode.ECB;
            aes.KeySize = 128;
            aes.GenerateKey();
            // change for decryption problem, TODO: delete

            byte[] e = { 0xa8, 0xda, 0xc6, 0x2f, 0x1c, 0x25, 0x81, 0xe2, 0xdf, 0x45, 0x8e, 0x21, 0x67, 0x3e, 0x55, 0x30 };
            return e;
            return aes.Key;
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

        private void sendImage()
        {
            byte[] data = generateData();
            int total = 0;
            int size = data.Length;
            int dataleft = size;
            int sent;

            byte[] datasize = new byte[4];
            datasize = BitConverter.GetBytes(size);
            //send the client the size of the data buffer
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

            //encrypt the data using the AES key from sigma session
            byte[] encrypted_symetric_key = encryption.EncryptBytes(keyAndTimes);
            Console.WriteLine("Key and times: " + keyAndTimes);
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

            Console.WriteLine("Image Id: " + imageId);

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

        public bool handleClientComm(object client)
        {
            try
            {
                TcpClient tcpClient = (TcpClient)client;
                socket = tcpClient.Client;
                clientConnected = socket.Connected;
                byte[] statusByte = new byte[DataStructs.INT_SIZE];
                while (clientConnected)
                {
                    
                    byte[] cmd = new byte[4];
                    //get the command id
                    socket.Receive(cmd, 0, 4, 0);
                    //parse the command id sent by the client
                    int command = BitConverter.ToInt32(cmd, 0);
                    if (command == REQUESTING_IMAGE)
                    {
                        encrypted_image = encryptImage("C:\\Project\\OurProject\\Project1\\Server\\Images\\ImageToEncrypt.bmp");
                        sendImage();
                        return true;

                    }
                    else
                    {
                        return false;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
    }
}
