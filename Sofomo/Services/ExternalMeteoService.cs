﻿using Microsoft.Extensions.Options;
using Sofomo.Config;
using Sofomo.Domain.Models.Request;
using Sofomo.Domain.Models.Response;
using System.Text.Json;

namespace Sofomo.Services
{
    public class ExternalMeteoService(
        IOptions<ExternalServiceSettings> externalServiceSettings,
        IHttpClientFactory httpClientFactory,
        ILogger<ExternalMeteoService> logger)
    {
        private readonly ExternalServiceSettings _externalServiceSettings = externalServiceSettings.Value;
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
        private readonly ILogger<ExternalMeteoService> _logger = logger;

        public async Task<Weather?> Get(Coordinates coordinates)
        {
            using HttpClient client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(_externalServiceSettings.BaseUrl);

            try
            {
                var response = await client.GetStringAsync
                    ($"forecast?latitude={coordinates.Latitude}&longitude={coordinates.Longitude}&current_weather=true&daily=sunrise&timezone=auto");

                return GetWeatherDataFromJson(response);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error getting weather forecast from meteo external service: {Error}", ex);
            }

            return null;
        }

        private static Weather GetWeatherDataFromJson(string response)
        {
            var jsonDoc = JsonDocument.Parse(response);
            var root = jsonDoc.RootElement;

            return new Weather
            {
                Temperature = root.GetProperty("current_weather").GetProperty("temperature").GetDouble(),
                WindDirection = root.GetProperty("current_weather").GetProperty("winddirection").GetDouble(),
                WindSpeed = root.GetProperty("current_weather").GetProperty("windspeed").GetDouble(),
            };
        }
    }
}
