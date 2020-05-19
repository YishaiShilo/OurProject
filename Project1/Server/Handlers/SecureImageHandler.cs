using System;
using System.Collections.Generic;
using System.Net.Sockets;



using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace DALSamplesServer
{
    class SecureImageHandler
    {
        public void handleClientComm(object client)
        {

            Encryption e = new Encryption();
            if (!e.handleClientComm(client))
            {
                
                Console.WriteLine("fail to make key");
            }
            Console.WriteLine("make key successfuly");
            Authentication a = new Authentication(e);
            if (a.handleClientComm(client))
            {
                Console.WriteLine("fail proccess aut key");
            }
            Console.WriteLine("aut key successfuly");
            ImageHandler i = new ImageHandler(e, a);
            if (i.handleClientComm(client))
            {
                Console.WriteLine("fail send image");
            }
            Console.WriteLine("send image successfuly");


        }
    }
}
