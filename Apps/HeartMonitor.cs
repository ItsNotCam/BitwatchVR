using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

namespace HoloPad.Apps.HeartMonitor
{
    public class HeartMonitorApp
    {
        const sbyte SEND_AVERAGE_INTERVAL = 15;

        IHeartMonitorBase monitor;
        Config config;

        Thread heartMonitorThread;
        bool stopThread;

        float[] history = new float[20];
        short historyIdx;

        public float heartRate;
        public float avgHeartRate;

        public HeartMonitorApp(Config config)
        {
            this.config = config;
            monitor = null;

            switch(config.data.heartConfig.monitor_type) {
                case Config.HeartMonitorType.Pulsoid:
                    monitor = new PulsoidMonitor();
                    break;
                case Config.HeartMonitorType.Stromno:
                    monitor = new StromnoMonitor();
                    break;
                case Config.HeartMonitorType.FitBit:
                    monitor = new FitBitMonitor();
                    break;
                case Config.HeartMonitorType.None:
                    break;
            }

            if (monitor != null) {
                monitor.Initialize(config);

                stopThread = false;
                heartMonitorThread = new Thread(UpdateHR);
                heartMonitorThread.Start();
            }
        }

        public void Uninitialize() => monitor.Uninitialize();

        void UpdateHR()//IUnifiedAvatarOSC osc)
        {
            while (!stopThread)
            {
                int rawHeartRate = monitor.GetCurrentHeartRate();
                heartRate = IntToVRCFloat(rawHeartRate);
                //Console.WriteLine($"[*] HR: {heartRate}");

                // if we detect that 10 of the last bois are the same then we pause
                // because we are most likely not tracking anymore
                history[historyIdx] = heartRate;
                historyIdx = (short)(historyIdx >= history.Length - 1 ? 0 : historyIdx + 1);
                if (history.Where(s => s != history[0]).Count() < 1) {
                    continue;
                }

                // set average
                config.data.heartConfig.lifetime_sample_sum += (decimal)rawHeartRate;
                config.data.heartConfig.lifetime_sample_count++;

                int rawAvgHeartrate = config.GetAverageHR();
                avgHeartRate = IntToVRCFloat(rawAvgHeartrate);

                Thread.Sleep(1000);
            }

            Uninitialize();
        }

        static float IntToVRCFloat(int i_hr)
        {
            int clamped = Math.Max(Math.Min(255, i_hr), 0);
            return (clamped - 127) / 127f;// Lerp(-1, 1, clamped / 255f);
            //return Lerp(-1, 1, clamped / 255f);
        }

        static float Lerp(float f_first, float f_second, float f_factor)
        {
            return f_first * (1 - f_factor) + f_second * f_factor;
        }
    }
}
