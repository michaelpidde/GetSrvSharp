using System.CommandLine.DragonFruit;

public struct Config
{
    public string Port;
    public string SiteRoot;
    public string DefaultPage;
}

class Program
{
    /// <param name="configFile">Path to server configuration file</param>
    static void Main(FileInfo? configFile)
    {
        if (configFile == null || !configFile.Exists)
        {
            Console.WriteLine("Supply a valid path to a server configuration file.");
            Environment.Exit(1);
        }

        AsyncSocketListener.Start(GetConfig(configFile));
    }

    static Config GetConfig(FileInfo fileInfo)
    {
        var config = new Config();
        return config;
    }
}