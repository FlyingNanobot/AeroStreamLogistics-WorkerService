using System.Text.Json;
using WorkerService.Models;

namespace WorkerService.Infrastructure
{
    /// <summary>
    /// Lightweight deserializer for OpenSky JSON responses.
    /// </summary>
    /// <remarks>
    /// OpenSky returns an array-based 'states' structure where each element is a heterogeneous
    /// array. This class provides a minimal parser that maps those array indices into a
    /// strongly-typed OpenSkyResponse and AircraftState objects.
    /// </remarks>
    public static class OpenSkyDeserializer
    {
        /// <summary>
        /// Parse the raw JSON payload from OpenSky into a typed OpenSkyResponse.
        /// </summary>
        /// <param name="json">Raw JSON returned by the OpenSky API.</param>
        /// <returns>OpenSkyResponse with Time and a list of AircraftState entries.</returns>
        public static OpenSkyResponse Parse(string json)
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var response = new OpenSkyResponse
            {
                Time = root.GetProperty("time").GetInt64(),
                States = new List<AircraftState>()
            };

            foreach (var stateArray in root.GetProperty("states").EnumerateArray())
            {
                var s = stateArray.EnumerateArray().ToArray();

                var aircraft = new AircraftState
                {
                    // Strings
                    Icao24 = s[0].ValueKind == JsonValueKind.Null ? null : s[0].GetString(),
                    Callsign = s[1].ValueKind == JsonValueKind.Null ? null : s[1].GetString(),
                    OriginCountry = s[2].ValueKind == JsonValueKind.Null ? null : s[2].GetString(),

                    // Longs (nullable)
                    TimePosition = s[3].ValueKind == JsonValueKind.Null ? null : s[3].GetInt64(),
                    LastContact = s[4].ValueKind == JsonValueKind.Null ? null : s[4].GetInt64(),

                    // Doubles (nullable)
                    Longitude = s[5].ValueKind == JsonValueKind.Null ? null : s[5].GetDouble(),
                    Latitude = s[6].ValueKind == JsonValueKind.Null ? null : s[6].GetDouble(),
                    BaroAltitude = s[7].ValueKind == JsonValueKind.Null ? null : s[7].GetDouble(),

                    // Bool
                    OnGround = s[8].ValueKind != JsonValueKind.Null && s[8].GetBoolean(),

                    // More doubles
                    Velocity = s[9].ValueKind == JsonValueKind.Null ? null : s[9].GetDouble(),
                    TrueTrack = s[10].ValueKind == JsonValueKind.Null ? null : s[10].GetDouble(),
                    VerticalRate = s[11].ValueKind == JsonValueKind.Null ? null : s[11].GetDouble(),

                    // Geo altitude
                    GeoAltitude = s[13].ValueKind == JsonValueKind.Null ? null : s[13].GetDouble(),

                    // Squawk
                    Squawk = s[14].ValueKind == JsonValueKind.Null ? null : s[14].GetString(),

                    // Spi
                    Spi = s[15].ValueKind != JsonValueKind.Null && s[15].GetBoolean(),

                    // Position source
                    PositionSource = s[16].ValueKind == JsonValueKind.Null ? 0 : s[16].GetInt32()
                };

                response.States.Add(aircraft);
            }

            return response;
        }
    }

}
