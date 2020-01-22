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
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;


namespace DALSamplesServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Server server = new Server();
            Console.WriteLine("Press any key to stop the server and exit\n");
            Console.Read();
            server.stop();
        }
    }
}