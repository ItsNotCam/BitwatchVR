using System;
using System.Threading;
using System.Text;

using WebSocketSharp;

namespace HoloPad.Apps.HeartMonitor
{
    internal class FitBitMonitor : IHeartMonitorBase
    {
        // constants
        const string URL = "";
        const string GET_HR_MSG = "getHR";
        const string CHECK_CONN_MSG = "checkFitbitConnection";
        const int SAMPLE_INTERVAL = 500;

        // status
        bool fitbitConnected;
        int currentHeartRate;

        // stuff that does things mhm yea woot woot
        WebSocket socket;
        Thread _thread;

        public int GetCurrentHeartRate() => currentHeartRate;// WatchUtils.IntToVRCFloatLmao(currentHeartRate);

        public void Initialize(Config config)
        {
            fitbitConnected = false;
            currentHeartRate = 0;

            _thread = new Thread(() =>
            {
                // init socket
                try {
                    socket = new WebSocket(URL);
                    socket.OnMessage += FitBitConnected;
                    socket.OnMessage += UpdateHeartRate;
                    socket.Connect();
                } catch(Exception e) { }

                if (socket != null) {
                    // check if fitbit connected
                    SendMessage(CHECK_CONN_MSG);

                    // sample heart rate every 500ms
                    while(socket.IsAlive) {
                        if (fitbitConnected) {
                            SendMessage(GET_HR_MSG);
                        } else {
                            SendMessage(CHECK_CONN_MSG);
                        }
                        Thread.Sleep(SAMPLE_INTERVAL);
                    }
                }
            });
            _thread.Start();
        }

        public void Uninitialize() => socket.Close();

        void FitBitConnected(object sender, MessageEventArgs eArgs)
        {
            try { fitbitConnected = eArgs.Data.Contains("yes"); } 
            catch (Exception) {  }
        }

        void UpdateHeartRate(object sender, MessageEventArgs eArgs) {
            try { currentHeartRate = Convert.ToInt32(eArgs.Data); }
            catch (Exception) { }
        }

        void SendMessage(string message)
        {
            byte[] content = Encoding.UTF8.GetBytes(message);
            socket.Send(content);
        }
    }
}
