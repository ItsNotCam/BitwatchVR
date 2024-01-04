using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Security.Principal;
using static System.Formats.Asn1.AsnWriter;

namespace HoloPad.Apps
{
    internal class Discord
    {
        // app id:  1053733297366245457
        // version: 1
        // connect: ws://127.0.0.1:PORT/?v=VERSION&client_id=CLIENT_ID&encoding=ENCODING
        // The port range for Discord's local RPC server is [6463, 6472].

        // AUTHORIZE
        /*
         {
          "nonce": "f48f6176-4afb-4c03-b1b8-d960861f5216",
          "args": {
            "client_id": "192741864418312192",
            "scopes": ["rpc", "identify"]
          },
          "cmd": "AUTHORIZE"
        }
         */

        // AUTHENTICATE
        /*
         {
          "nonce": "5bb10a43-1fdc-4391-9512-0c8f4aa203d4",
          "args": {
            "access_token": "CZhtkLDpNYXgPH9Ml6shqh2OwykChw"
          },
          "cmd": "AUTHENTICATE"
        }
        */

        TcpListener tcpl;
        TcpClient tcpc;

        int startPort = 6463;
        int endPort = 6472;
        int boundPort = -1;

        public Discord() {
            // find bound port
            for (int i = startPort; i <= endPort && boundPort < 0; i++) {
                try {
                    tcpc = new TcpClient("127.0.0.1", i);
                    boundPort = i;
                } catch (SocketException e) {
                    Console.WriteLine($"DISCORD CONNECTION: {e.Message}");
                }
            }

            if(boundPort < 0) {
                Console.WriteLine("Failed to connect.");
                return;
            }

            Console.WriteLine("Connected to port: " + boundPort);

            // try send mic mute
            /*JObject r = JObject.Parse(@"{
                'nonce': 'f48f6176-4afb-4c03-b1b8-d960861f5216',
                'args': {
                    'mute': true
                },
                'cmd': 'SET_VOICE_SETTINGS'
            }");*/

            // https://discord.com/developers/docs/topics/rpc
            JObject r = JObject.Parse(@"{
              'nonce': 'f48f6176-4afb-4c03-b1b8-d960861f5216',
              'args': {
                'client_id': '192741864418312192',
                'scopes': ['rpc', 'identify']
              },
              'cmd': 'AUTHORIZE'
            }");

            NetworkStream ns = tcpc.GetStream();
            byte[] sentData = Encoding.ASCII.GetBytes(r.ToString());
            ns.Write(sentData, 0, sentData.Length);

            Console.WriteLine($"Sent: {r.ToString()}");

            // get response
            byte[] recvData = new byte[256];

            int bytes = ns.Read(recvData, 0, sentData.Length);
            string response = Encoding.ASCII.GetString(recvData, 0, bytes) ?? string.Empty;

            Console.WriteLine($"Received: {response}");
        }
    }
}
