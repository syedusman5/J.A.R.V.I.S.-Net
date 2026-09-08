using Jarvis.Core.Interface;
using Jarvis.Core.Service;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Jarvis.Core.Skill
{
    public sealed class WeatherOptions
    {
        public const string SectionName = "Weather";

        public string DefaultLocation { get; set; } = "Chennai";
    }

    public class WeatherSkill(IOptions<WeatherOptions> options, IHttpClientFactory httpClientFactory) : ISkill
    {
        public string Name => "weather";

        public string Description => "Current weather for a city.";

        public IReadOnlyList<string> Examples => ["weather in Tokyo", "what's the weather"];

        public async Task<SkillResult> ExecuteAsync(UserRequest request, CancellationToken ct)
        {
            var place = ExtractLocation(request) ?? options.Value.DefaultLocation;

            var http = httpClientFactory.CreateClient("jarvis");

            var geo = await http.GetFromJsonAsync<GeocodeResponse>(
            $"https://geocoding-api.open-meteo.com/v1/search?name={Uri.EscapeDataString(place)}&count=1&language=en&format=json",
            ct);

            var match = geo?.Results?.FirstOrDefault();
            if (match is null)
                return SkillResult.Fail($"I couldn't find a place called {place}.");

            var url = $"https://api.open-meteo.com/v1/forecast" +
                      $"?latitude={match.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}" +
                      $"&longitude={match.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}" +
                      $"&current=temperature_2m,relative_humidity_2m,weather_code";

            var forecast = await http.GetFromJsonAsync<ForecastResponse>(url, ct);
            if (forecast?.Current is null)
                return SkillResult.Fail("The weather service didn't return anything usable.");

            var c = forecast.Current;
            var text = $"{match.Name}: {c.Temperature:0.#}°C, {Describe(c.WeatherCode)}, humidity {c.Humidity}%.";

            return SkillResult.Ok(text, text);
        }

        public int Score(UserRequest request)
        {
            if (!request.ContainsAnyWord("weather", "temperature", "forecast", "raining", "hot", "cold", "umbrella"))
                return 0;

            return request.ContainsAnyWord("weather", "forecast") ? 85 : 50;
        }

        private static string? ExtractLocation(UserRequest request)
        {
            string[] markers = ["in", "at", "for"];
            var idx = Array.FindIndex(request.Tokens, t => markers.Contains(t));

            if (idx < 0 || idx == request.Tokens.Length - 1) return null;

            return string.Join(' ', request.Tokens.Skip(idx + 1));
        }

        private static string Describe(int code) => code switch
        {
            0 => "clear sky",
            1 or 2 or 3 => "partly cloudy",
            45 or 48 => "foggy",
            >= 51 and <= 57 => "drizzling",
            >= 61 and <= 67 => "raining",
            >= 71 and <= 77 => "snowing",
            >= 80 and <= 82 => "rain showers",
            >= 95 and <= 99 => "thunderstorms",
            _ => "unclear conditions"
        };

        private sealed record GeocodeResponse([property: JsonPropertyName("results")] List<GeoResult>? Results);

        private sealed record GeoResult([property: JsonPropertyName("name")] string Name, [property: JsonPropertyName("latitude")] double Latitude, [property: JsonPropertyName("longitude")] double Longitude);

        private sealed record ForecastResponse([property: JsonPropertyName("current")] CurrentWeather? Current);
        private sealed record CurrentWeather(
            [property: JsonPropertyName("temperature_2m")] double Temperature,
            [property: JsonPropertyName("relative_humidity_2m")] int Humidity,
            [property: JsonPropertyName("weather_code")] int WeatherCode);
    }
}
