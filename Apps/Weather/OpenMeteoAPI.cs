namespace HoloPad.Apps.Weather.OpenMeteoAPI
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public class OpenMeteoAPIObject
    {
        public float latitude;
        public float longitude;
        public float generationtime_ms;
        public int utc_offset_seconds;
        public string timezone;
        public string timezone_abbreviation;
        public float elevation;
        public CurrentWeatherObject current_weather;
        public HourlyUnitsObject hourly_units;
        public HourlyDataObject hourly;
        public DailyUnitsObject daily_units;
        public DailyDataObject daily;
    }

    public class CurrentWeatherObject {
        public float temperature;
        public float windspeed;
        public float winddirection;
        public int weathercode;
        public string time;
    }

    public class HourlyUnitsObject
    {
        public string time;
        public string apparent_temperature;
        public string weathercode;
    }

    public class HourlyDataObject
    {
        public List<string> time;
        public List<float> apparent_temperature;
        public List<int> weathercode;
    }

    public class DailyUnitsObject
    {
        public string time;
        public string sunrise;
        public string sunset;
    }

    public class DailyDataObject
    {
        public List<string> time;
        public List<string> sunrise;
        public List<string> sunset;
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}
