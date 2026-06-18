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
    /// <summary>
    /// Background worker that periodically ingests state vectors from the OpenSky API
    /// and publishes them to a Kafka topic.
    /// </summary>
    /// <remarks>
    /// The worker uses IHttpClientFactory to create a client, OpenSkyAuthService to obtain
    /// bearer tokens, and IKafkaProducer to publish messages. It honors cancellation via
    /// the provided CancellationToken and polls on a fixed delay.
    /// </remarks>
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

        /// <summary>
        /// Core execution loop invoked by the BackgroundService host.
        /// </summary>
        /// <remarks>
        /// The method runs until the host signals cancellation. It retrieves an access token,
        /// performs the OpenSky request, deserializes the result and publishes each aircraft
        /// state to the configured Kafka topic.
        /// </remarks>
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
                    await _producer.PublishAsync("telemetry-stream", aircraft);
                }

                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }
}