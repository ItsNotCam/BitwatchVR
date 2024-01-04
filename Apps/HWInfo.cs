using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

using Hardware.Info;
using OpenHardwareMonitor.Hardware;

namespace HoloPad.Apps
{
    internal class HWInfoApp
    {
        public HWInfoApp()
        {
            Computer comp = new Computer();
            foreach(var h in comp.Hardware) {
                Console.WriteLine("Hardware Type: " + h.HardwareType);
                Console.WriteLine("Name: " + h.Name);
            }
            /*
            HardwareInfo hwinfo = new HardwareInfo();
            hwinfo.RefreshAll();

            Console.WriteLine("CPU: " + hwinfo.CpuList[0].);

            foreach(VideoController vid in hwinfo.VideoControllerList) {
                Console.WriteLine(vid.Name);
                Console.WriteLine(vid.AdapterRAM / 1024f);
            }
            */
        }
    }
}
