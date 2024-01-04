using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoloPad.Apps
{
    public class TimeApp
    {
        Thread timeThread;
        bool stopThread;

        public int month;
        public int day;
        public int dayName;
        public int hour;
        public int minute;

        public TimeApp() => StartThread();
        
        public void StopThread() {
            while(timeThread != null && timeThread.IsAlive)
                stopThread = true;

            stopThread = false;
        }

        public void StartThread()
        {
            StopThread();
            timeThread = new Thread(UpdateTime);
            timeThread.Start();
        }

        void UpdateTime()
        {
            // i do it this way cuz its faster ok, fight me
            while (!stopThread)
            {
                DateTime dt = DateTime.Now;
                month = dt.Month;
                day = dt.Day;
                dayName = (int)dt.DayOfWeek;
                hour = dt.Hour;
                minute = dt.Minute;

                Thread.Sleep(1000);
            }
        }
    }
}
