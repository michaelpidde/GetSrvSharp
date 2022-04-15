namespace WorkerService;

public class Worker : BackgroundService {
  private AsyncSocketListener _listener;

  public Worker(Configuration config) {
    if(config.ConfigFile == null || !config.ConfigFile.Exists) {
      Console.WriteLine("Supply a valid path to a server configuration file.");
      Environment.Exit(1);
    }

    _listener = new AsyncSocketListener(Config.Get(config.ConfigFile));
    _listener.Start();
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
    while(!stoppingToken.IsCancellationRequested) {
      _listener.Listen();
    }
  }
}
