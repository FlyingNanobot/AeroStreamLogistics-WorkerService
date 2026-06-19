namespace WorkerService.Models
{
    /// <summary>
    /// Represents a response from the OpenSky Network API's /states/all endpoint.
    /// </summary>
    /// <remarks>
    /// This model maps the top-level JSON returned by OpenSky. The <see cref="Time"/> value
    /// is the server timestamp (Unix epoch seconds) for the snapshot and <see cref="States"/>
    /// contains a list of individual aircraft state vectors.
    /// </remarks>
    public class OpenSkyResponse
    {
        /// <summary>
        /// Snapshot time in seconds since Unix epoch (UTC).
        /// </summary>
        /// <remarks>
        /// OpenSky returns time as an integer number of seconds since 1970-01-01 UTC.
        /// Use DateTimeOffset.FromUnixTimeSeconds(Time) to convert to a DateTimeOffset.
        /// </remarks>
        public long Time { get; set; }

        /// <summary>
        /// Collection of aircraft state vectors contained in the snapshot.
        /// </summary>
        /// <remarks>
        /// Each entry contains positional, ident and flight-related data for one tracked
        /// transponder/ads-b emitter. Some fields are nullable when data is not available.
        /// </remarks>
        public List<AircraftState>? States { get; set; }
    }

    /// <summary>
    /// An OpenSky state vector for a single aircraft/ads-b emitter.
    /// </summary>
    /// <remarks>
    /// The fields mirror the OpenSky 'states' array. Most numeric fields are nullable since
    /// the OpenSky API often omits values when unavailable (e.g., no position or no altitude).
    /// See property remarks for unit and interpretation details.
    /// </remarks>
    public class AircraftState
    {
        /// <summary>
        /// Unique ICAO 24-bit address in hexadecimal (lowercase).
        /// </summary>
        public string? Icao24 { get; set; }          // Unique ICAO hex

        /// <summary>
        /// Callsign as reported by the aircraft. Often padded with spaces or null.
        /// </summary>
        public string? Callsign { get; set; }        // Flight callsign

        /// <summary>
        /// Country inferred from the ICAO address allocation.
        /// </summary>
        public string? OriginCountry { get; set; }   // Country of origin

        /// <summary>
        /// Time (Unix epoch seconds) of the last position update for this aircraft, if known.
        /// </summary>
        /// <remarks>
        /// Nullable: when position data is not available the value will be null. Convert
        /// via DateTimeOffset.FromUnixTimeSeconds(TimePosition.Value) when not null.
        /// </remarks>
        public long? TimePosition { get; set; }      // Last position timestamp

        /// <summary>
        /// Time (Unix epoch seconds) of the last contact received for this aircraft.
        /// </summary>
        public long? LastContact { get; set; }       // Last contact timestamp

        /// <summary>
        /// WGS-84 longitude in decimal degrees. Nullable when position not reported.
        /// </summary>
        public double? Longitude { get; set; }

        /// <summary>
        /// WGS-84 latitude in decimal degrees. Nullable when position not reported.
        /// </summary>
        public double? Latitude { get; set; }

        /// <summary>
        /// Barometric altitude in meters as reported by the aircraft (may be null).
        /// </summary>
        /// <remarks>
        /// Barometric altitude depends on the aircraft's altimeter setting; it differs from
        /// geometric altitude (GPS/ellipsoid). Use with caution when correlating to ground
        /// elevation — prefer GeoAltitude when available for height above the WGS-84 ellipsoid.
        /// </remarks>
        public double? BaroAltitude { get; set; }

        /// <summary>
        /// True if the transponder indicates the aircraft is on the ground.
        /// </summary>
        public bool OnGround { get; set; }

        /// <summary>
        /// Velocity over ground in meters per second. Nullable if unknown.
        /// </summary>
        /// <remarks>
        /// OpenSky provides speed as m/s. Convert to knots by multiplying with 1.943844.
        /// </remarks>
        public double? Velocity { get; set; }

        /// <summary>
        /// True track (heading) in decimal degrees clockwise from North. Nullable.
        /// </summary>
        public double? TrueTrack { get; set; }

        /// <summary>
        /// Vertical rate (m/s). Positive means climbing, negative descending. Nullable.
        /// </summary>
        public double? VerticalRate { get; set; }

        /// <summary>
        /// Sensor identifiers that contributed to this state vector. Representation may vary.
        /// </summary>
        /// <remarks>
        /// In the raw OpenSky JSON this is an array of integers. This project currently maps
        /// it to a nullable double to match existing deserialization code; if you need sensor
        /// ids prefer changing the type to int[] or List<int> and update deserialization.
        /// </remarks>
        public double? Sensors { get; set; }        // optional

        /// <summary>
        /// Geometric altitude above the WGS-84 ellipsoid in meters. Nullable.
        /// </summary>
        public double? GeoAltitude { get; set; }

        /// <summary>
        /// Assigned squawk (transponder) code as a string. May be null or empty.
        /// </summary>
        public string? Squawk { get; set; }

        /// <summary>
        /// Special position indicator (SPI) flag — true when set.
        /// </summary>
        public bool Spi { get; set; }

        /// <summary>
        /// Source of the position: 0 = ADS-B, 1 = ASTERIX, 2 = MLAT (per OpenSky docs).
        /// </summary>
        public int PositionSource { get; set; }
    }


}
