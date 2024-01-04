using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using Newtonsoft.Json.Converters;

namespace HoloPad
{
    public class Config
    {
        public enum HeartMonitorType
        {
            [EnumMember(Value = "pulsoid")] Pulsoid,
            [EnumMember(Value = "stromno")] Stromno,
            [EnumMember(Value = "fitbit")] FitBit,
            [EnumMember(Value = "hyperate")] HypeRate,
            [EnumMember(Value = "none")] None
        }

        public class HeartData {
            [JsonConverter(typeof(StringEnumConverter))]
            public HeartMonitorType monitor_type;
            public float average_heart_rate;
            public string access_token;
            public ulong lifetime_sample_count;
            public decimal lifetime_sample_sum;
        }

        public class WeatherData
        {
            public float latitude;
            public float longitude;
            public string api_key;
            public bool use_fahrenheit;
        }

        public class ConfigData {
            public HeartData heartConfig;
            public WeatherData weatherConfig;
        }

        // constants
        static readonly string CONFIG_DIR = Path.Combine(Directory.GetCurrentDirectory(), "Config");
        static readonly string CONFIG_PATH = Path.Combine(CONFIG_DIR, "Config.json");

        public ConfigData? data;

        public Config()
        {
            try {
                if (!Directory.Exists(CONFIG_DIR))
                    Directory.CreateDirectory(CONFIG_DIR);

                if (!File.Exists(CONFIG_PATH))
                    CreateConfig();

                JObject reader = JObject.Parse(File.ReadAllText(CONFIG_PATH));
                this.data = reader.ToObject(typeof(ConfigData)) as ConfigData;
            }
            catch (System.Exception e) { }
        }
        
        public int GetAverageHR()
        {
            return (int) (data.heartConfig.lifetime_sample_sum / data.heartConfig.lifetime_sample_count);
        }

        void CreateConfig() {

            HeartData hrConfig = new HeartData() {
                average_heart_rate = 0,
                monitor_type = HeartMonitorType.Pulsoid,
                access_token = String.Empty,
                lifetime_sample_sum = 0,
                lifetime_sample_count = 0
            };

            WeatherData weatherConfig = new WeatherData() {
                latitude = 0,
                longitude = 0,
                api_key = String.Empty,
                use_fahrenheit = true
            };

            data = new ConfigData() { 
                weatherConfig = weatherConfig,
                heartConfig = hrConfig
            };

            WriteValues();
        }

        public void WriteValues() {
            File.WriteAllText(CONFIG_PATH, JObject.FromObject(data).ToString());
        }
    }
}
