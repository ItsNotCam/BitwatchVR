using System;

using WebSocketSharp;
using Newtonsoft.Json.Linq;

namespace HoloPad.Apps.HeartMonitor
{
    internal class PulsoidMonitor : IHeartMonitorBase
    {
        const string URL_BASE = "wss://dev.pulsoid.net/api/v1/data/real_time?access_token=";

        WebSocket socket;
        int currentHeartRate;

        public int GetCurrentHeartRate() {
            //Console.WriteLine($"[*] HR 1: {currentHeartRate}");
            return currentHeartRate;
        }// WatchUtils.IntToVRCFloatLmao(currentHeartRate);

        public void Initialize(Config config)
        {
            // string access_token = "9ca25af0-bb40-460b-aab9-aa7fa2f904dd";
            string url = URL_BASE + config.data.heartConfig.access_token;
            try {
                // setup web socket
                socket = new WebSocket(url);
                socket.OnMessage += UpdateHeartRate;
                socket.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12;
                socket.Connect();
            } catch (Exception e) {
                Console.WriteLine($"\n[!!] Failed to connect to socket at {url} with\n{e}");
            }
        }

        public void Uninitialize() => socket.Close();

        void UpdateHeartRate(object sender, MessageEventArgs eArgs)
        {
            // parse the data from json
            try {
                JObject jo = JObject.Parse(eArgs.Data);
                if (jo != null) {
                    currentHeartRate = jo["data"]["heart_rate"].Value<int>();
                    //Console.WriteLine($"[*] HR: {currentHeartRate} ");
                }
            }
            catch (Exception e) { }
        }
    }
}
