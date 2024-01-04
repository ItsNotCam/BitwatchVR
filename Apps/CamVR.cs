using BuildSoft.VRChat.Osc;
using OVRSharp.Exceptions;
using Valve.VR;
using OVRApp = OVRSharp.Application;
using OVRSharp;

namespace HoloPad.Apps
{
    public struct OVRBattery
    {
        public uint trackerIndex;
        public float batteryLevel;
        public bool isCharging;
    }

    public class CamVRApp
    {
        // public
        public List<OVRBattery> batteries;
        public bool threadRunning;
        public bool ovrRunning;

        // ovr
        OVRApp ovrApp;
        uint LEFT_HAND;
        uint RIGHT_HAND;

        // battery
        Thread batteryThread;
        bool stopThread;

        public CamVRApp() => TryInitOVR();

        public void TryInitOVR()
        {
            try {
                ovrApp = new OVRApp(OVRApp.ApplicationType.Background);
                HoloPad.App.PrintDebug("[NOTIFICATION] OpenVR app initialized!");
            } catch (OVRSharp.Exceptions.OpenVRSystemException<Valve.VR.EVRInitError> e) {
                HoloPad.App.PrintDebug("[ERROR] Failed to load the OpenVR app - Is SteamVR running?");
            }

            ovrRunning = ovrApp != null && ovrApp.OVRSystem != null;

            batteries = new List<OVRBattery>();
            batteryThread = new Thread(BatteryThread);
            batteryThread.Start();

            threadRunning = batteryThread.IsAlive;
            if (threadRunning) {
                HoloPad.App.PrintDebug("[NOTIFICATION] Successfully started battery thread.");
            } else {
                HoloPad.App.PrintDebug("[ERROR] Failed to start battery thread.");
            }
        }

        public void Uninitialize()
        {
            StopBatteryThread();

            if (ovrApp != null) {
                ovrApp.Shutdown();
            }
        }

        public void StartBatteryThread()
        {
            StopBatteryThread();
            batteryThread = new Thread(BatteryThread);
            batteryThread.Start();

            if (batteryThread.IsAlive) {
                HoloPad.App.PrintDebug("[NOTIFICATION] Successfully started battery thread.");
            } else {
                HoloPad.App.PrintDebug("[ERROR] Failed to start battery thread.");
            }
        }

        public void StopBatteryThread()
        {
            int iterations = 0;
            while (batteryThread != null && batteryThread.IsAlive && iterations < 10)
            {
                stopThread = true;
                iterations++;
            }

            if (iterations > 0)
            {
                Console.WriteLine("[ERROR] Failed to stop battery thread, 10 attempts exceeded.");
            }
            else
            {
                Console.WriteLine("[NOTIFICATION] Successfully stopped battery thread.");
            }

            stopThread = false;
        }

        void BatteryThread()
        {
            RefreshTrackers();

            int iterations = 0;
            while (!stopThread)
            {
                for(int i = 0; i < batteries.Count; i++)
                {
                    OVRBattery battery = batteries[i];
                    battery.batteryLevel = GetBatteryLevel(ovrApp, battery.trackerIndex);
                    battery.isCharging = GetCharging(ovrApp, battery.trackerIndex);

                    batteries[i] = battery;

                    /*
                    int iBatteryLevel = (int)(battery.batteryLevel * 100);
                    string charging = battery.isCharging ? "Charging    " : "Not Charging";
                    Console.WriteLine($"[BATTERY] Device {i} | {charging} | {iBatteryLevel}%");
                    */
                }

                // should we refresh trackers?
                if (iterations > 2)
                {
                    //Console.Write($"[BATTERY] Refreshing Device List...");
                    RefreshTrackers();
                    //Console.WriteLine($"Done\n");
                    iterations = 0;
                } else {
                    iterations++;
                }

                // sleep 5 seconds
                Thread.Sleep(5000);
            }
        }

        void RefreshTrackers()
        {
            if (ovrApp == null || ovrApp.OVRSystem == null)
                return;

            batteries = new List<OVRBattery>();

            try
            {
                LEFT_HAND = ovrApp.OVRSystem.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.LeftHand);
                batteries.Add(new OVRBattery() {
                    trackerIndex = LEFT_HAND,
                    batteryLevel = GetBatteryLevel(this.ovrApp, LEFT_HAND),
                    isCharging = false
                });

                RIGHT_HAND = ovrApp.OVRSystem.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.RightHand);
                batteries.Add(new OVRBattery() {
                    trackerIndex = RIGHT_HAND,
                    batteryLevel = GetBatteryLevel(this.ovrApp, RIGHT_HAND),
                    isCharging = GetCharging(ovrApp, RIGHT_HAND)
                });

                //List<int> trackers = new List<int>();
                for (uint i = 0; i < OpenVR.k_unMaxTrackedDeviceCount; i++)
                {
                    ETrackedDeviceClass trackedClass = ovrApp.OVRSystem.GetTrackedDeviceClass(i);
                    if (trackedClass == ETrackedDeviceClass.GenericTracker)
                    {
                        //trackers.Add((int)i);
                        batteries.Add(new OVRBattery() {
                            trackerIndex = i,
                            batteryLevel = GetBatteryLevel(ovrApp, i),
                            isCharging = GetCharging(ovrApp, i)
                        });
                    }
                }
            }
            catch (Exception e) {
                HoloPad.App.PrintDebug("[ERROR] Oop! Error detected\n)");
                HoloPad.App.PrintDebug($"[ERROR] {e.Message.Replace("\n", "\n[ERROR] ")}");
            }
        }

        public void PulseRight()
        {
            ovrApp.OVRSystem.TriggerHapticPulse(RIGHT_HAND, 2, 5000);

            ulong actionHandle = 0;
            OpenVR.Input.GetActionHandle("/actions/lasermouse/out/Haptic", ref actionHandle);
            OpenVR.Input.TriggerHapticVibrationAction(actionHandle, 0, 1, 1, 1, LEFT_HAND);
        }

        public void PulseLeft() => ovrApp.OVRSystem.TriggerHapticPulse(LEFT_HAND, 2, 5000);

        public static float Lerp(float f_first, float f_second, float f_factor)
        {
            return f_first * (1 - f_factor) + f_second * f_factor;
        }

        bool GetCharging(OVRSharp.Application app, uint controller)
        {
            bool isCharging = false;

            if (app.OVRSystem.IsTrackedDeviceConnected(controller))
            {
                ETrackedPropertyError err = ETrackedPropertyError.TrackedProp_InvalidDevice;
                isCharging = app.OVRSystem.GetBoolTrackedDeviceProperty(
                    controller, ETrackedDeviceProperty.Prop_DeviceIsCharging_Bool, ref err
                );
            }

            return isCharging;
        }

        float GetBatteryLevel(OVRSharp.Application app, uint controller)
        {
            float batteryLevel = 0;
            if (app.OVRSystem.IsTrackedDeviceConnected(controller))
            {
                ETrackedPropertyError err = ETrackedPropertyError.TrackedProp_InvalidDevice;
                batteryLevel = app.OVRSystem.GetFloatTrackedDeviceProperty(
                    controller, ETrackedDeviceProperty.Prop_DeviceBatteryPercentage_Float, ref err
                );
            }

            return batteryLevel;
        }
    }
}