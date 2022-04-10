using WorkerService;

public class Configuration {
    public FileInfo? ConfigFile { get; set; }
}

public class Program {
    public static async Task Main(FileInfo? configFile) {
        IHost host = Host.CreateDefaultBuilder()
            .ConfigureServices(services => {
                services.AddSingleton(new Configuration { ConfigFile = configFile });
                services.AddHostedService<Worker>();
            })
            .Build();

        await host.RunAsync();
    }
}
