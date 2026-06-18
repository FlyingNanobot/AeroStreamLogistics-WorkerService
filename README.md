AeroStreamLogistics-WorkerService
================================

Project: .NET Worker Service that consumes OpenSky Network state vectors and processes aircraft telemetry.

What this README contains
- Brief explanation of the OpenSky model used in the project
- Units and interpretation notes for critical fields
- Guidance for generating API documentation from XML comments

OpenSky model and critical theory
--------------------------------

- Snapshot time (OpenSkyResponse.Time)
  - Integer seconds since Unix epoch (UTC). Use DateTimeOffset.FromUnixTimeSeconds(...) to convert.

- State vectors (OpenSkyResponse.States)
  - Each AircraftState represents an ads-b/transponder state vector. Many fields are nullable when data is not available.

- Coordinates
  - Latitude/Longitude: WGS-84 decimal degrees. Use standard geographic libraries to compute distances or bounding boxes.

- Altitudes
  - BaroAltitude: Barometric altitude in meters as reported by aircraft. Depends on altimeter setting and is not the same as geometric altitude.
  - GeoAltitude: Geometric altitude above the WGS-84 ellipsoid in meters (preferred for absolute elevation comparisons).

- Velocity and vertical rate
  - Velocity: meters per second. Multiply by 1.943844 to get knots.
  - VerticalRate: meters per second. Positive = climbing, negative = descending.

- Time fields
  - TimePosition and LastContact are Unix epoch seconds; they may be null when the corresponding data is missing.

- Sensors
  - The raw OpenSky JSON contains an array of sensor IDs. The project currently maps it to a nullable double to match legacy deserialization; consider changing to int[] or List<int> if sensor ids are required.

XML comments and developer notes
-------------------------------

- The WorkerService.Models.OpenSkyResponse and AircraftState classes include XML documentation comments (///) and <remarks> explaining units, nullability and conversion guidance.
- These comments are intended to be consumed by IDE tooltips and to generate API documentation.

Generating documentation
------------------------

1. In the .csproj set <GenerateDocumentationFile>true</GenerateDocumentationFile> (Visual Studio project properties -> Build -> XML documentation file).
2. Build the project. The XML file will be produced next to the assembly and can be used with tools like DocFX or Sandcastle.

Notes and suggestions
---------------------

- Keep the AircraftState.Sensors mapping in mind; if you rely on sensor ids, update the model to a collection type and fix deserialization.
- When correlating altitude to ground elevation, prefer GeoAltitude when available; barometric altitude varies with pressure settings.

Project structure and developer notes
-----------------------------------

- Dependency injection and hosted services
  - Program.cs registers the following singletons and hosted services:
	- IKafkaProducer -> Application.Concrete.KafkaProducer (singleton)
	- OpenSkyAuthService (singleton)
	- CryptoValidatorService (singleton)
	- OpenSkyIngestionWorker (hosted BackgroundService)

- Suggested developer tasks
  - Replace Console.WriteLine calls in infrastructure classes with ILogger<T> to integrate with the host logging pipeline.
  - Enable XML documentation generation in the project file (GenerateDocumentationFile) and use DocFX or similar to publish API docs.

