using WebSocketSharp;

namespace HoloPad.Apps.HeartMonitor
{
    internal class StromnoMonitor : IHeartMonitorBase
    {
        int currentHeartRate;

        public int GetCurrentHeartRate() => currentHeartRate;

        public void Initialize(Config config) { }

        public void Uninitialize() { }

        void UpdateHeartRate(object sender, MessageEventArgs eArgs) { }
    }
}
