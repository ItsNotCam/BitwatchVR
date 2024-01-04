using BuildSoft.VRChat.Osc.Avatar;
using BuildSoft.VRChat.Osc;
using HoloPad.Apps.HeartMonitor;
using HoloPad.Apps.Weather;
using HoloPad.Apps;

namespace HoloPad
{
    public class App
    {
        // static parameters
        static class Parameters
        {
            // OpenVR
            public static class OpenVR
            {
                public const string HAPTICS_LEFT = "Haptics/_PulseLeft";
                public const string HAPTICS_RIGHT = "Haptics/_PulseRight";
            }

            // Main System
            public static class System
            {
                public const string ID = "OSC/ID";
                public const string VALUE = "OSC/Value";
            }


            // Media
            public static class Media
            {
                public const string AUDIO_NEXT = "OSC/Audio/Next";
                public const string AUDIO_PREV = "OSC/Audio/Prev";
                public const string AUDIO_PLAY_PAUSE = "OSC/Audio/PlayPause";

                public const string DISCORD_MUTE = "OSC/Audio/DiscordMute";
            }
        }

        enum Cycle
        {
            START = 0,

            // batteries
            READY_CONTROLLER_LEFT = 1, SEND_CONTROLLER_LEFT = 2,
            READY_CONTROLLER_CHARGING_LEFT = 3, SEND_CONTROLLER_CHARGING_LEFT = 4,

            READY_CONTROLLER_RIGHT = 5, SEND_CONTROLLER_RIGHT = 6,
            READY_CONTROLLER_CHARGING_RIGHT = 7, SEND_CONTROLLER_CHARGING_RIGHT = 8,

            READY_TRACKER_1 = 9, SEND_TRACKER_1 = 10,
            READY_TRACKER_CHARGING_1 = 11, SEND_TRACKER_CHARGING_1 = 12,

            READY_TRACKER_2 = 13, SEND_TRACKER_2 = 14,
            READY_TRACKER_CHARGING_2 = 15, SEND_TRACKER_CHARGING_2 = 16,

            READY_TRACKER_3 = 17, SEND_TRACKER_3 = 18,
            READY_TRACKER_CHARGING_3 = 19, SEND_TRACKER_CHARGING_3 = 20,

            READY_TRACKER_4 = 21, SEND_TRACKER_4 = 22,
            READY_TRACKER_CHARGING_4 = 23, SEND_TRACKER_CHARGING_4 = 24,

            READY_TRACKER_5 = 25, SEND_TRACKER_5 = 26,
            READY_TRACKER_CHARGING_5 = 27, SEND_TRACKER_CHARGING_5 = 28,

            READY_TRACKER_6 = 29, SEND_TRACKER_6 = 30,
            READY_TRACKER_CHARGING_6 = 31, SEND_TRACKER_CHARGING_6 = 32,

            READY_TRACKER_7 = 33, SEND_TRACKER_7 = 34,
            READY_TRACKER_CHARGING_7 = 35, SEND_TRACKER_CHARGING_7 = 36,

            READY_TRACKER_8 = 37, SEND_TRACKER_8 = 38,
            READY_TRACKER_CHARGING_8 = 39, SEND_TRACKER_CHARGING_8 = 40,

            // heart
            READY_HEARTRATE = 41, SEND_HEARTRATE = 42,
            READY_AVG_HEARTRATE = 43, SEND_AVG_HEARTRATE = 44,

            // time
            READY_MONTH = 45, SEND_MONTH = 46,
            READY_DAY = 47, SEND_DAY = 48,
            READY_DAYNAME = 49, SEND_DAYNAME = 50,
            READY_HOUR = 51, SEND_HOUR = 52,
            READY_MINUTE = 53, SEND_MINUTE = 54,

            // weather
            READY_FORECAST = 55, SEND_FORECAST = 56,
            READY_TEMPERATURE = 57, SEND_TEMPERATURE = 58,
            READY_INTENSITY = 59, SEND_INTENSITY = 60,
            READY_SUN = 61, SEND_SUN = 62,

            DONE = 63
        }

        // config
        Config config;

        // apps
        TimeApp time;
        CamVRApp camVR;
        MediaApp media;
        WeatherApp weather;
        HeartMonitorApp heartMonitor;
        HWInfoApp hwinfo;

        // data loop
        Thread loop;
        Cycle cycle;

        public App()
        {
            // start cycle
            cycle = Cycle.START;

            // init config
            config = new Config();

            // init child apps
            camVR = new CamVRApp();
            time = new TimeApp();
            heartMonitor = new HeartMonitorApp(config);
            weather = new WeatherApp(config);
            media = new MediaApp();
            hwinfo = new HWInfoApp();

            // run sending thread
            loop = new Thread(StartLoop);
        }

        public async void Start()
        {
            //Discord discord = new Discord();
            //return;

            loop.Start();

            // init osc app
            OscAvatarConfig? avatarConfig = null;
            PrintDebug($"[NOTIFICATION] Reading now... Try to \"Reset Avatar.\"\n");
            avatarConfig = await OscAvatarConfig.WaitAndCreateAtCurrentAsync();
            PrintDebug($"[NOTIFICATION] Read avatar config. Name: {avatarConfig.Name}");

            // register handlers
            OscAvatarParameterChangedEventHandler? logHandler = LogParameterChange;
            OscAvatarParameterChangedEventHandler? hapticsHandlerL = HandleHapticsL;
            OscAvatarParameterChangedEventHandler? hapticsHandlerR = HandleHapticsR;
            OscAvatarParameterChangedEventHandler? audioHandler = HandleMedia;

            // check for avatar change
            OscAvatarUtility.AvatarChanged += (sender, e) =>
            {
                //avatarConfig.Parameters.ParameterChanged -= logHandler;
                avatarConfig.Parameters.ParameterChanged -= hapticsHandlerL;
                avatarConfig.Parameters.ParameterChanged -= hapticsHandlerR;
                avatarConfig.Parameters.ParameterChanged -= audioHandler;

                avatarConfig = OscAvatarConfig.CreateAtCurrent()!;
                PrintDebug($"[NOTIFICATION] Changed avatar. Name: {avatarConfig.Name}\n");

                //avatarConfig.Parameters.ParameterChanged += logHandler;
                avatarConfig.Parameters.ParameterChanged += hapticsHandlerL;
                avatarConfig.Parameters.ParameterChanged += hapticsHandlerR;
                avatarConfig.Parameters.ParameterChanged += audioHandler;
            };

            // add handlers
            //avatarConfig.Parameters.ParameterChanged += logHandler;
            avatarConfig.Parameters.ParameterChanged += hapticsHandlerL;
            avatarConfig.Parameters.ParameterChanged += hapticsHandlerR;
            avatarConfig.Parameters.ParameterChanged += audioHandler;

            await Task.Delay(-1);
        }
        
        void StartLoop()
        {
            int loopCount = 0;
            PrintDebug("[NOTIFICATION] System Loop Started");
            while (true)
            {
                // save config every 1000 runs :)
                if (loopCount > 1000)
                {
                    loopCount = 0;
                    config.WriteValues();
                }

                if (cycle >= Cycle.READY_CONTROLLER_LEFT && cycle < Cycle.READY_HEARTRATE)
                {
                    if (camVR.ovrRunning) DoBatteryCycle();
                    else cycle = Cycle.READY_HEARTRATE;
                }

                if (cycle >= Cycle.READY_HEARTRATE && cycle < Cycle.READY_MONTH && heartMonitor != null)
                    DoHeartrateCycle();
                else if (cycle >= Cycle.READY_MONTH && cycle < Cycle.READY_FORECAST && time != null)
                    DoDateTimeCycle();
                else if (cycle >= Cycle.READY_FORECAST && cycle < Cycle.DONE && weather != null)
                    DoWeatherCycle();

                NextState();
                loopCount++;

                Thread.Sleep(150);
            }
        }

        void SendOSCMessage(string address, object value, string debugMsg = "")
        {
            if (address == null || address.Length < 1)
            {
                PrintDebug("[ERROR] Error in sending message, address was invalid.");
                return;
            }

            if (value == null)
            {
                PrintDebug($"[ERROR] Error in sending message, value being sent to '{address}' was null.");
                return;
            }

            Type valueType = value.GetType();

            value = valueType == typeof(Cycle) ? (int)value : value;
            PrintDebug($"{debugMsg} {value} => '{address}'");

            if (valueType == typeof(bool))
            {
                OscParameter.SendAvatarParameter(address, (bool)value);
            }
            else if (valueType == typeof(int) || valueType == typeof(Cycle))
            {
                OscParameter.SendAvatarParameter(address, (int)value);
            }
            else if (valueType == typeof(float))
            {
                OscParameter.SendAvatarParameter(address, (float)value);
            }
            else
            {
                PrintDebug($"[ERROR] Error in sending message, value '{valueType}' is not of an accepted type.");
            }
        }

        void DoBatteryCycle()
        {
            if (!camVR.ovrRunning)
                return;

            string address = string.Empty;
            object value = -1;

            switch (cycle)
            {
                // left controller
                case Cycle.READY_CONTROLLER_LEFT:
                    address = Parameters.System.ID;
                    value = Cycle.READY_CONTROLLER_LEFT;
                    break;
                case Cycle.SEND_CONTROLLER_LEFT:
                    address = Parameters.System.VALUE;
                    value = camVR.batteries[0].batteryLevel;
                    break;
                case Cycle.READY_CONTROLLER_CHARGING_LEFT:
                    address = Parameters.System.ID;
                    value = Cycle.READY_CONTROLLER_CHARGING_LEFT;
                    break;
                case Cycle.SEND_CONTROLLER_CHARGING_LEFT:
                    address = Parameters.System.VALUE;
                    value = camVR.batteries[0].isCharging ? 1f : 0f;
                    break;

                // right controller
                case Cycle.READY_CONTROLLER_RIGHT:
                    address = Parameters.System.ID;
                    value = Cycle.READY_CONTROLLER_RIGHT;
                    break;
                case Cycle.SEND_CONTROLLER_RIGHT:
                    address = Parameters.System.VALUE;
                    value = camVR.batteries[1].batteryLevel;
                    break;
                case Cycle.READY_CONTROLLER_CHARGING_RIGHT:
                    address = Parameters.System.ID;
                    value = Cycle.READY_CONTROLLER_CHARGING_RIGHT;
                    break;
                case Cycle.SEND_CONTROLLER_CHARGING_RIGHT:
                    address = Parameters.System.VALUE;
                    value = camVR.batteries[1].isCharging ? 1f : 0f;
                    break;

                // tracker battery 1
                case Cycle.READY_TRACKER_1:
                    address = Parameters.System.ID;
                    value = Cycle.READY_TRACKER_1;
                    break;
                case Cycle.SEND_TRACKER_1:
                    address = Parameters.System.VALUE;
                    value = camVR.batteries.Count > 2
                        ? camVR.batteries[2].batteryLevel
                        : 0;
                    break;
                case Cycle.READY_TRACKER_CHARGING_1:
                    address = Parameters.System.ID;
                    value = Cycle.READY_TRACKER_CHARGING_1;
                    break;
                case Cycle.SEND_TRACKER_CHARGING_1:
                    address = Parameters.System.VALUE;
                    value = camVR.batteries.Count > 2
                        ? camVR.batteries[2].isCharging ? 1f : 0f
                        : 0;
                    break;

                // tracker battery 2
                case Cycle.READY_TRACKER_2:
                    address = Parameters.System.ID;
                    value = Cycle.READY_TRACKER_2;
                    break;
                case Cycle.SEND_TRACKER_2:
                    address = Parameters.System.VALUE;
                    value = camVR.batteries.Count > 3
                        ? camVR.batteries[3].batteryLevel
                        : 0;
                    break;
                case Cycle.READY_TRACKER_CHARGING_2:
                    address = Parameters.System.ID;
                    value = Cycle.READY_TRACKER_CHARGING_2;
                    break;
                case Cycle.SEND_TRACKER_CHARGING_2:
                    address = Parameters.System.VALUE;
                    value = camVR.batteries.Count > 3
                        ? camVR.batteries[3].isCharging ? 1f : 0f
                        : 0;
                    break;

                // tracker battery 3
                case Cycle.READY_TRACKER_3:
                    address = Parameters.System.ID;
                    value = Cycle.READY_TRACKER_3;
                    break;
                case Cycle.SEND_TRACKER_3:
                    address = Parameters.System.VALUE;
                    value = camVR.batteries.Count > 4
                        ? camVR.batteries[4].batteryLevel
                        : 0;
                    break;
                case Cycle.READY_TRACKER_CHARGING_3:
                    address = Parameters.System.ID;
                    value = Cycle.READY_TRACKER_CHARGING_3;
                    break;
                case Cycle.SEND_TRACKER_CHARGING_3:
                    address = Parameters.System.VALUE;
                    value = camVR.batteries.Count > 4
                        ? camVR.batteries[4].isCharging ? 1f : 0f
                        : 0;
                    break;

                // tracker battery 4
                case Cycle.READY_TRACKER_4:
                    address = Parameters.System.ID;
                    value = Cycle.READY_TRACKER_4;
                    break;
                case Cycle.SEND_TRACKER_4:
                    address = Parameters.System.VALUE;
                    value = camVR.batteries.Count > 5
                        ? camVR.batteries[5].batteryLevel
                        : 0;
                    break;
                case Cycle.READY_TRACKER_CHARGING_4:
                    address = Parameters.System.ID;
                    value = Cycle.READY_TRACKER_CHARGING_4;
                    break;
                case Cycle.SEND_TRACKER_CHARGING_4:
                    address = Parameters.System.VALUE;
                    value = camVR.batteries.Count > 5
                        ? camVR.batteries[5].isCharging ? 1f : 0f
                        : 0;
                    break;

                // tracker battery 5
                case Cycle.READY_TRACKER_5:
                    address = Parameters.System.ID;
                    value = Cycle.READY_TRACKER_5;
                    break;
                case Cycle.SEND_TRACKER_5:
                    address = Parameters.System.VALUE;
                    value = camVR.batteries.Count > 6
                        ? camVR.batteries[6].batteryLevel
                        : 0;
                    break;
                case Cycle.READY_TRACKER_CHARGING_5:
                    address = Parameters.System.ID;
                    value = Cycle.READY_TRACKER_CHARGING_5;
                    break;
                case Cycle.SEND_TRACKER_CHARGING_5:
                    address = Parameters.System.VALUE;
                    value = camVR.batteries.Count > 6
                        ? camVR.batteries[6].isCharging ? 1f : 0f
                        : 0;
                    break;

                // tracker battery 6
                case Cycle.READY_TRACKER_6:
                    address = Parameters.System.ID;
                    value = Cycle.READY_TRACKER_6;
                    break;
                case Cycle.SEND_TRACKER_6:
                    address = Parameters.System.VALUE;
                    value = camVR.batteries.Count > 7
                        ? camVR.batteries[7].batteryLevel
                        : 0;
                    break;
                case Cycle.READY_TRACKER_CHARGING_6:
                    address = Parameters.System.ID;
                    value = Cycle.READY_TRACKER_CHARGING_6;
                    break;
                case Cycle.SEND_TRACKER_CHARGING_6:
                    address = Parameters.System.VALUE;
                    value = camVR.batteries.Count > 7
                        ? camVR.batteries[7].isCharging ? 1f : 0f
                        : 0;
                    break;

                // tracker battery 7
                case Cycle.READY_TRACKER_7:
                    address = Parameters.System.ID;
                    value = Cycle.READY_TRACKER_7;
                    break;
                case Cycle.SEND_TRACKER_7:
                    address = Parameters.System.VALUE;
                    value = camVR.batteries.Count > 8
                        ? camVR.batteries[8].batteryLevel
                        : 0;
                    break;
                case Cycle.READY_TRACKER_CHARGING_7:
                    address = Parameters.System.ID;
                    value = Cycle.READY_TRACKER_CHARGING_7;
                    break;
                case Cycle.SEND_TRACKER_CHARGING_7:
                    address = Parameters.System.VALUE;
                    value = camVR.batteries.Count > 8
                        ? camVR.batteries[8].isCharging ? 1f : 0f
                        : 0;
                    break;

                // tracker battery 8
                case Cycle.READY_TRACKER_8:
                    address = Parameters.System.ID;
                    value = Cycle.READY_TRACKER_8;
                    break;
                case Cycle.SEND_TRACKER_8:
                    address = Parameters.System.VALUE;
                    value = camVR.batteries.Count > 9
                        ? camVR.batteries[9].batteryLevel
                        : 0;
                    break;
                case Cycle.READY_TRACKER_CHARGING_8:
                    address = Parameters.System.ID;
                    value = Cycle.READY_TRACKER_CHARGING_8;
                    break;
                case Cycle.SEND_TRACKER_CHARGING_8:
                    address = Parameters.System.VALUE;
                    value = camVR.batteries.Count > 9
                        ? camVR.batteries[9].isCharging ? 1f : 0f
                        : 0;
                    break;
            }

            SendOSCMessage(address, value, "[BATTERY]");
        }

        void DoHeartrateCycle()
        {
            string address = string.Empty;
            object value = -1;

            switch (cycle)
            {
                case Cycle.READY_HEARTRATE: 
                    address = Parameters.System.ID; 
                    value = Cycle.READY_HEARTRATE; 
                    break;
                case Cycle.SEND_HEARTRATE: 
                    address = Parameters.System.VALUE; 
                    value = heartMonitor.heartRate; 
                    break;
                case Cycle.READY_AVG_HEARTRATE: 
                    address = Parameters.System.ID; 
                    value = Cycle.READY_AVG_HEARTRATE; 
                    break;
                case Cycle.SEND_AVG_HEARTRATE: 
                    address = Parameters.System.VALUE; 
                    value = heartMonitor.avgHeartRate; 
                    break;
            }

            SendOSCMessage(address, value, "[HEART]");
        }

        void DoDateTimeCycle()
        {
            string address = string.Empty;
            object value = -1;

            switch (cycle)
            {
                case Cycle.READY_MONTH: address = Parameters.System.ID; value = Cycle.READY_MONTH; break;
                case Cycle.SEND_MONTH: address = Parameters.System.VALUE; value = time.month / 12f; break;
                case Cycle.READY_DAY: address = Parameters.System.ID; value = Cycle.READY_DAY; break;
                case Cycle.SEND_DAY: address = Parameters.System.VALUE; value = (time.day - 1) / 30f; break;
                case Cycle.READY_DAYNAME: address = Parameters.System.ID; value = Cycle.READY_DAYNAME; break;
                case Cycle.SEND_DAYNAME: address = Parameters.System.VALUE; value = time.dayName / 7f; break;
                case Cycle.READY_HOUR: address = Parameters.System.ID; value = Cycle.READY_HOUR; break;
                case Cycle.SEND_HOUR: address = Parameters.System.VALUE; value = time.hour / 23f; break;
                case Cycle.READY_MINUTE: address = Parameters.System.ID; value = Cycle.READY_MINUTE; break;
                case Cycle.SEND_MINUTE: address = Parameters.System.VALUE; value = time.minute / 59f; break;
            }

            SendOSCMessage(address, value, "[TIME]");

        }

        void DoWeatherCycle()
        {
            string address = string.Empty;
            object value = -1;

            switch (cycle)
            {
                case Cycle.READY_FORECAST:
                    address = Parameters.System.ID;
                    value = Cycle.READY_FORECAST;
                    break;
                case Cycle.SEND_FORECAST:
                    address = Parameters.System.VALUE;
                    value = weather.weatherData.forecast / 9f;
                    break;
                case Cycle.READY_TEMPERATURE:
                    address = Parameters.System.ID;
                    value = Cycle.READY_TEMPERATURE;
                    break;
                case Cycle.SEND_TEMPERATURE:
                    address = Parameters.System.VALUE;
                    value = weather.weatherData.temperature / 127f;
                    break;
                case Cycle.READY_INTENSITY:
                    address = Parameters.System.ID;
                    value = Cycle.READY_INTENSITY;
                    break;
                case Cycle.SEND_INTENSITY:
                    address = Parameters.System.VALUE;
                    value = weather.weatherData.intensity / 3f;
                    break;
                case Cycle.READY_SUN:
                    address = Parameters.System.ID;
                    value = Cycle.READY_SUN;
                    break;
                case Cycle.SEND_SUN: 
                    address = Parameters.System.VALUE;

                    int sunriseHour = weather.weatherData.sunrise.Hour;
                    int sunriseMinute = weather.weatherData.sunrise.Minute;
                    int sunsetHour = weather.weatherData.sunset.Hour;
                    int sunsetMinute = weather.weatherData.sunset.Minute;

                    if (time.hour > sunriseHour && time.hour < sunsetHour) {
                        value = 1f;
                    } else if(time.hour == sunriseHour && time.minute > sunriseMinute) {
                        value = 1f;
                    } else if(time.hour == sunsetHour && time.minute < sunsetMinute) {
                        value = 1f;
                    } else {
                        value = 0f;
                    }
                    break;
            }

            SendOSCMessage(address, value, "[WEATHER]");
        }

        void HandleHapticsL(OscAvatarParameter parameter, ValueChangedEventArgs e)
        {
            if (!parameter.Name.Contains("Haptics"))
                return;

            object? value = e.NewValue;
            if (value == null || value is not bool || (bool) value != true)
                return;

            if (parameter.Name.Equals(Parameters.OpenVR.HAPTICS_LEFT)) {
                camVR.PulseLeft();
                SendOSCMessage(Parameters.OpenVR.HAPTICS_LEFT, false, "[HAPTICS]");
            }
        }

        void HandleHapticsR(OscAvatarParameter parameter, ValueChangedEventArgs e)
        {
            if (!parameter.Name.Contains("Haptics"))
                return;

            object? value = e.NewValue;
            if (value == null || value is not bool || (bool)value != true)
                return;

            if (parameter.Name.Equals(Parameters.OpenVR.HAPTICS_RIGHT))
            {
                camVR.PulseRight();
                SendOSCMessage(Parameters.OpenVR.HAPTICS_RIGHT, false, "[HAPTICS]");
            }
        }

        void HandleMedia(OscAvatarParameter parameter, ValueChangedEventArgs e)
        {
            object? value = e.NewValue;
            if (value == null || (value is not bool) || !(bool)value)
                return;

            switch(parameter.Name)
            {
                case Parameters.Media.AUDIO_NEXT:
                    PrintDebug("[MEDIA] Next Track");
                    media.AudioNext();

                    camVR.PulseRight();
                    Thread.Sleep(100);
                    camVR.PulseRight();

                    break;
                case Parameters.Media.AUDIO_PREV: 
                    PrintDebug("[MEDIA] Previous Track");
                    media.AudioPrev();

                    camVR.PulseRight();
                    Thread.Sleep(100);
                    camVR.PulseRight();

                    break;
                case Parameters.Media.AUDIO_PLAY_PAUSE: 
                    PrintDebug("[MEDIA] Play / Pause");
                    media.AudioPlayPause();

                    camVR.PulseLeft();
                    Thread.Sleep(100);
                    camVR.PulseLeft();

                    break;
                case Parameters.Media.DISCORD_MUTE:
                    PrintDebug("[MEDIA] Discord Mute Toggle");
                    media.ToggleDiscordMute();
                    
                    camVR.PulseLeft();
                    Thread.Sleep(100);
                    camVR.PulseLeft();

                    break;
            }
        }

        void LogParameterChange(OscAvatarParameter parameter, ValueChangedEventArgs e)
        {
            if (parameter.Name.Contains("Angular") || parameter.Name.Contains("Velocity") || parameter.Name.Contains("Upright"))
                return;

            //camVR.PulseLeft();
            //camVR.PulseRight();

            //PrintDebug($"{parameter.Name}: {e.OldValue} => {e.NewValue}");
            PrintDebug($"[RECEIVED] {parameter.Name} <= {e.NewValue}");
        }

        void NextState() => cycle = (cycle == Cycle.DONE) ? Cycle.START : cycle + 1;

        public static float IntToVRCFloatLmao(int i)
        {
            int clamped = Math.Max(Math.Min(255, i), 0);
            return Lerp(-1, 1, clamped / 255f);
        }

        public static float Lerp(float f_first, float f_second, float f_factor)
        {
            return f_first * (1 - f_factor) + f_second * f_factor;
        }

        public static float Clamp(float min, float max, float value) => MathF.Max(min, MathF.Min(max, value));

        public static void PrintDebug(string message)
        {
            DateTime now = DateTime.Now;
            Console.WriteLine($"[{now.ToShortDateString()} {now.ToShortTimeString()}] {message}");
        }
    }
}
