using Newtonsoft.Json.Linq;
using HoloPad.Apps.Weather.OpenMeteoAPI;

namespace HoloPad.Apps.Weather
{
    public class WeatherApp
    {
        public struct WeatherData {
            public int temperature;
            public int forecast;
            public int intensity;
            public DateTime sunrise;
            public DateTime sunset;
        }
        public struct WeatherCodes
        {
            public static Dictionary<string, int> Icons = new Dictionary<string, int>() {
            { "01n", 1 }, { "02n", 2 }, { "03n", 3 }, { "04n", 4 },
            { "09n", 5 }, { "10n", 6 }, { "11n", 7 }, { "13n", 8 },
            { "50n", 9 },

            { "01d", 1 }, { "02d", 2 }, { "03d", 3 }, { "04d", 4 },
            { "09d", 5 }, { "10d", 6 }, { "11d", 7 }, { "13d", 8 },
            { "50d", 9 }
        };

            public static List<int> thunderstorm = new List<int>() {
            200, 201, 202, 210, 211, 212, 221,
            230, 231, 232,
        };

            public static List<int> lightRain = new List<int>() {
            300, 301, 310, 311, 321,
            500, 501,
            701, 721, 741
        };

            public static List<int> clearSky = new List<int>() {
            800
        };

            public static List<int> cloudy = new List<int>() {
            801, 802, 803, 804
        };

            public static List<int> heavyRain = new List<int>() {
            302, 311, 312, 313, 314, 321,
            502, 503, 511, 520, 521, 522, 531
        };

            public static List<int> danger = new List<int>() {
            212,
            504,
            622,
            781
        };

            public static List<int> snow = new List<int>() {
            600, 601, 602, 611, 612, 613, 615, 615, 616, 620, 621
        };

            public static List<int> lolWhat = new List<int>() {
            711, 721, 731, 751, 761, 762, 771
        };

            public static Dictionary<int, int> wmoCodes = new Dictionary<int, int>
        {
            // https://open-meteo.com/en/docs#api-documentation

            // clear sky
            { 0, 1 }, { 1, 1 }, { 2, 1 }, { 3, 1 }, 

            // few / scattered clouds
            { 45, 2 }, { 48, 2 },

            // drizzle
            { 51, 6 }, { 53, 6 }, { 55, 5 },
            { 56, 6 }, { 57, 5 },

            // rain / showers
            { 61, 6 }, { 63, 6 }, { 65, 5 },
            { 66, 6 }, { 67, 5 },
            { 80, 5 }, { 81, 6 }, { 82, 7 },

            // snow
            { 71, 8 }, { 73, 8 }, { 75, 8 }, { 77, 8 },
            { 85, 8 }, { 86, 8 },

            // thunderstorm
            { 95, 7 }, { 96, 7 }, { 99, 7 }
        };
        }

        Config config;
        Thread weatherThread;
        public WeatherData weatherData;

        public WeatherApp(Config config)
        {
            this.config = config;
            weatherData = new WeatherData() {
                temperature = 0,
                forecast = 0,
                intensity = 0,
                sunrise = new DateTime(),
                sunset = new DateTime()
            };

            HttpClient client = new HttpClient();
            string url = GetURLOpenMeteo(config);
            weatherThread = new Thread(() => SampleWeatherOpenMeteo(client, url));
            weatherThread.Start();
        }
        
        async void SampleWeatherOpenMeteo(HttpClient client, string url)
        {
            while(true)
            {
                try {
                    // "2022-12-24"
                    string today = DateTime.Now.Date.ToLocalTime().ToString("yyyy-MM-dd");

                    // Get message
                    HttpResponseMessage message = await client.GetAsync(url);
                    string body = await message.Content.ReadAsStringAsync();
                    OpenMeteoAPIObject? o = JObject.Parse(body).ToObject<OpenMeteoAPIObject>();
                    if (o is null)
                        continue;

                    float temperature = o.current_weather.temperature;
                    int weatherCode = o.current_weather.weathercode;

                    // get sunrise / sunset
                    // why does this API make it so hard?
                    DateTime sunrise = DateTime.Now;
                    DateTime sunset = DateTime.Now;
                    int todayIdx = o.daily.time.IndexOf(today);
                    if (todayIdx >= 0)
                    {
                        string sunrStr = o.daily.sunrise[todayIdx];
                        string sunsStr = o.daily.sunset[todayIdx];

                        // 2022-12-25T07:24
                        sunrise = DateTime.ParseExact(sunrStr, "yyyy-MM-ddTHH:mm",
                            System.Globalization.CultureInfo.InvariantCulture);
                        sunset = DateTime.ParseExact(sunsStr, "yyyy-MM-ddTHH:mm",
                            System.Globalization.CultureInfo.InvariantCulture);
                    }

                    weatherData.sunrise = sunrise;
                    weatherData.sunset = sunset;
                    weatherData.temperature = ClampAndRoundTemperature(temperature);
                    weatherData.forecast = WeatherCodes.wmoCodes[weatherCode];

                    App.PrintDebug(
                        $"[WEATHER_DEBUG] Temperature: {weatherData.temperature}\n" +
                        $"[WEATHER_DEBUG] Sunrise: {sunrise}\n" +
                        $"[WEATHER_DEBUG] Sunset: {sunset}\n" +
                        $"[WEATHER_DEBUG] WMO Weather Code: {weatherCode}\n" +
                        $"[WEATHER_DEBUG] Animator Weather Code: {WeatherCodes.wmoCodes[weatherCode]}"
                    );
                } catch (Exception e) {
                    App.PrintDebug($"[WEATHER] Oop - failed to get weather {e.Message}");
                }

                //sleep for 10 minutes
                Thread.Sleep(10 * 60 * 1000);
            }
        }

        async void SampleOpenWeatherAPI(HttpClient client, string url) { 
            while(true) {
                try {
                    HttpResponseMessage message = await client.GetAsync(url);

                    string body = await message.Content.ReadAsStringAsync();
                    JObject obj = JObject.Parse(body);

                    //Console.WriteLine(obj.ToString());

                    // https://openweathermap.org/forecast5
                    int kelvin = (int) Math.Floor(obj["main"]["temp"].Value<float>());
                    string weatherIconName = obj["weather"][0]["icon"].Value<string>();
                    int weatherID = obj["weather"][0]["id"].Value<int>();
                    int sunrise = obj["sys"]["sunrise"].Value<int>();
                    int sunset = obj["sys"]["sunset"].Value<int>();

                    // get sunrise and sunset
                    DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                    DateTime sunriseTime = dt.AddSeconds(sunrise).ToLocalTime();
                    DateTime sunsetTime = dt.AddSeconds(sunset).ToLocalTime();

                    //Console.WriteLine("Sunrise: " + $"[{sunriseTime.ToShortDateString()} {sunriseTime.ToShortTimeString()}]");
                    //Console.WriteLine("Sunset:  " + $"[{sunsetTime.ToShortDateString()} {sunsetTime.ToShortTimeString()}]");
                    //Console.WriteLine("Feels Like: " + KelvinToFahrenheit(kelvin));

                    // convert units
                    float temp = config.data.weatherConfig.use_fahrenheit
                        ? KelvinToFahrenheit(kelvin)
                        : KelvinToCelsius(kelvin);

                    Console.WriteLine($"[*] Temperature: {temp}");

                    // clamp to VRC range (sbyte)
                    int clampedTemp = (int) Math.Min(Math.Max(-128, temp), 127);

                    // convert to (0 - 255) int
                    // with negative temperatures being converted to the range (0 - 127)
                    // and positive temperatures being converted to the range (128 - 255)
                    //float finalTemp = clamped;// < 0 ? Math.Abs(clamped) - 1 : clamped + 128;

                    // set weather code based on icons
                    // https://openweathermap.org/weather-conditions
                    int weatherCode = GetWeatherCode(weatherIconName);

                    // if the weather ID is an extreme one, add 10
                    switch(weatherID)
                    {
                        case 781: case 762: case 622: case 602:
                        case 504: case 212: case 202:
                            weatherCode += 10;
                            break;
                        default:
                            break;
                    }

                    /*
                    // set weather severity
                    int intensity = 0;
                    if (config.data.weatherConfig.use_fahrenheit) {
                        if (finalTemp < 45) {
                            intensity = 0;
                        } else if (finalTemp > 85) {
                            intensity = 1;
                        } else {
                            intensity = 2;
                        }
                    } else {
                        if (finalTemp < 7) {
                            intensity = 0;
                        } else if (finalTemp > 29) {
                            intensity = 1;
                        } else {
                            intensity = 2;
                        }
                    }
                    weatherData.intensity = intensity;
                    */

                    weatherData.forecast = weatherCode;
                    weatherData.temperature = clampedTemp;
                    weatherData.sunrise = sunriseTime;
                    weatherData.sunset = sunsetTime;
                } catch(Exception e) {
                    App.PrintDebug($"[WEATHER] Oop - failed to get weather {e.Message}");
                }

                //sleep for 10 minutes
                Thread.Sleep(10 * 60 * 1000);
            }
        }

        static int GetWeatherCode(string weatherIconName) => WeatherCodes.Icons[weatherIconName];

        static int ClampAndRoundTemperature(float temperature)
        {
            float clamped = Math.Min(Math.Max(-128, temperature), 127);
            return (int)MathF.Round(clamped);
        }

        static float KelvinToCelsius(int kelvin) => kelvin - 273.15f;

        static float KelvinToFahrenheit(int kelvin) => KelvinToCelsius(kelvin) * 9f / 5f + 32;

        static string GetURLOpenMeteo(Config config)
        {
            float lat = config.data.weatherConfig.latitude;
            float lon = config.data.weatherConfig.longitude;
            string temperature = config.data.weatherConfig.use_fahrenheit
                ? $"&temperature_unit=fahrenheit" : string.Empty;

            /* https://api.open-meteo.com/v1/forecast?
             * latitude=38.80&
             * longitude =-77.26&
             * hourly=apparent_temperature,weathercode&
             * daily=sunrise,sunset&
             * current_weather=true&
             * temperature_unit=fahrenheit&
             * windspeed_unit=mph&
             * precipitation_unit=inch&
             * timezone=auto&
             * past_days=1
            */
            return "https://api.open-meteo.com/v1/forecast?"
                + $"&latitude={lat}&longitude={lon}"
                + "&hourly=apparent_temperature,weathercode"
                + "&daily=sunrise,sunset"
                + "&current_weather=true"
                + temperature
                + "&timezone=auto"
                + "&past_days=1";
        }

        static string GetURLOpenWeather(Config config)
        {
            float lat = config.data.weatherConfig.latitude;
            float lon = config.data.weatherConfig.longitude;
            string apik = config.data.weatherConfig.api_key;// "87953227f2442dcd66d3f4264c105c8d";
            return $"https://api.openweathermap.org/data/2.5/weather" +
                $"?lat={lat}" +
                $"&lon={lon}" +
                $"&appid={apik}";
        }
    }
}
