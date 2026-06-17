using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using WorkerService.Application.Contract;
using WorkerService.Infrastructure;
using WorkerService.Models;

namespace WorkerService.Workers
{
    public class OpenSkyIngestionWorker : BackgroundService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IKafkaProducer _producer;
        private readonly OpenSkyAuthService _auth;
        private readonly CryptoValidatorService _crypto;
        private readonly IConfiguration _config;

        public OpenSkyIngestionWorker(
            IHttpClientFactory httpClientFactory,
            IKafkaProducer producer,
            OpenSkyAuthService auth,
            CryptoValidatorService crypto,
            IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _producer = producer;
            _auth = auth;
            _crypto = crypto;
            _config = config;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var client = _httpClientFactory.CreateClient("opensky");

            while (!stoppingToken.IsCancellationRequested)
            {
                var token = await _auth.GetAccessTokenAsync();
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync("https://opensky-network.org/api/states/all", stoppingToken);
                var json = await response.Content.ReadAsStringAsync(stoppingToken);
                var data = OpenSkyDeserializer.Parse(json);

                foreach (var aircraft in data.States)
                {
                    await _producer.PublishAsync("telemetry.raw", aircraft);
                }

                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }
}