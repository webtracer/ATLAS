using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.WindowsServices;

Host.CreateDefaultBuilder(args)
    .UseWindowsService(options =>
    {
        options.ServiceName = "ATLAS Watcher";
    })
    .ConfigureServices(services =>
    {
        services.AddHttpClient();
        services.AddHostedService<WatcherWorker>();
    })
    .Build()
    .Run();