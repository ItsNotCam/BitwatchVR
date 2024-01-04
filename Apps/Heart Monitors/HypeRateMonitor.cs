using System;
using System.Text;

using WebSocketSharp;
using Newtonsoft.Json.Linq;


namespace HoloPad.Apps.HeartMonitor
{
    internal class HypeRateMonitor : IHeartMonitorBase
    {
        const string URL = "wss://hrproxy.fortnite.lol:2096/hrproxy";

        WebSocket socket;
        int currentHeartRate;

        public int GetCurrentHeartRate() => currentHeartRate;

        public void Initialize(Config config)
        {
            // string access_token = "9ca25af0-bb40-460b-aab9-aa7fa2f904dd";
            try
            {
                // setup web socket
                socket = new WebSocket(URL);
                socket.OnMessage += UpdateHeartRate;
                socket.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12;
                socket.Connect();

                JObject jo = new JObject() {
                    { "reader", "hyperate" },
                    { "identifier", config.data.heartConfig.access_token },
                    { "service", "vrchat" }
                };
                SendMessage(jo.ToString());
            }
            catch (Exception e) { }
        }

        public void Uninitialize() => socket.Close();

        void UpdateHeartRate(object sender, MessageEventArgs eArgs)
        {
            try
            {
                // Parse the message and get the HR or Pong
                JObject jo = JObject.Parse(eArgs.Data);
                if (jo["method"] != null)
                {
                    JObject jout = new JObject();
                    jout.Add("method", "pong");
                    jout.Add("pingId", jo["pingId"]?.Value<string>());
                    SendMessage(jout.ToString());
                }
                else
                {
                    currentHeartRate = Convert.ToInt32(jo["hr"].Value<string>());
                }
            }
            catch (Exception) { }

        }

        void SendMessage(string message)
        {
            byte[] content = Encoding.UTF8.GetBytes(message);
            socket.Send(content);
        }
    }
}
