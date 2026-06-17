using WorkerService.Application.Concrete;
using WorkerService.Application.Contract;
using WorkerService.Infrastructure;
using WorkerService.Workers;


var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHttpClient("opensky");
builder.Services.AddSingleton<IKafkaProducer, KafkaProducer>();
builder.Services.AddSingleton<OpenSkyAuthService>();
builder.Services.AddSingleton<CryptoValidatorService>();
builder.Services.AddHostedService<OpenSkyIngestionWorker>();

var host = builder.Build();
host.Run();