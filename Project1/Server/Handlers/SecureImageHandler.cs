using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace DALSamplesServer
{
    class SecureImageHandler
    {
        public void handleClientComm(object client)
        {
            SIGMAHandler sigHan = new SIGMAHandler();
            sigHan.handleClientComm(client);
            var g = sigHan.GaGb;


        }
    }
}
