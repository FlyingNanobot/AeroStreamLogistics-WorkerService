using System;
using System.Collections.Generic;
using System.Text;

namespace WorkerService.Models
{
    public class OpenSkyResponse
    {
        public long Time { get; set; }
        public List<AircraftState> States { get; set; }
    }

    public class AircraftState
    {
        public string Icao24 { get; set; }          // Unique ICAO hex
        public string Callsign { get; set; }        // Flight callsign
        public string OriginCountry { get; set; }   // Country of origin
        public long? TimePosition { get; set; }      // Last position timestamp
        public long? LastContact { get; set; }       // Last contact timestamp
        public double? Longitude { get; set; }
        public double? Latitude { get; set; }
        public double? BaroAltitude { get; set; }
        public bool OnGround { get; set; }
        public double? Velocity { get; set; }
        public double? TrueTrack { get; set; }
        public double? VerticalRate { get; set; }
        public double? Sensors { get; set; }        // optional
        public double? GeoAltitude { get; set; }
        public string Squawk { get; set; }
        public bool Spi { get; set; }
        public int PositionSource { get; set; }
    }


}
