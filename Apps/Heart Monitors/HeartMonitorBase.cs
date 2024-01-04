using System;

namespace HoloPad.Apps.HeartMonitor
{
    internal interface IHeartMonitorBase
    {
        int GetCurrentHeartRate();
        void Initialize(Config config);
        void Uninitialize();
    }
}
