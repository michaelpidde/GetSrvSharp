using System.CommandLine.DragonFruit;
using Newtonsoft.Json;

public class Config
{
    public const string ContentDirectory = "content";
    public const string TemplateDirectory = "template";
    public int Port { get; set; }
    public DirectoryInfo? SiteRoot { get; set; }
    public string DefaultPage { get; set; }
    public bool TemplateEngineEnabled { get; set; }

    public DirectoryInfo GetContentDirectory()
    {
        var directory = new DirectoryInfo(SiteRoot.FullName + Path.DirectorySeparatorChar + ContentDirectory);
        if (!directory.Exists)
        {
            Console.WriteLine("Warning: Content directory does not exist.");
        }
        return directory;
    }

    public DirectoryInfo GetTemplateDirectory()
    {
        var directory = new DirectoryInfo(SiteRoot.FullName + Path.DirectorySeparatorChar + TemplateDirectory);
        if (!directory.Exists)
        {
            Console.WriteLine("Warning: Template directory does not exist.");
        }
        return directory;
    }
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

        var listener = new AsyncSocketListener(GetConfig(configFile));
        listener.Start();
    }

    static Config GetConfig(FileInfo configFile)
    {
        if (!configFile.Exists)
        {
            Console.WriteLine("Server configuration file does not exist.");
            Environment.Exit(1);
        }

        JsonConverter[] converters = new JsonConverter[]
        {
            new FileInfoConverter(),
            new DirectoryInfoConverter()
        };
        Config config = JsonConvert.DeserializeObject<Config>(configFile.OpenText().ReadToEnd(), converters);
        
        // If port is not in file it will set this to 0
        if(config.Port == 0)
        {
            Console.WriteLine("Config error: Missing key 'port' or invalid value.");
            Environment.Exit(1);
        }

        if(config.SiteRoot == null)
        {
            Console.WriteLine("Config error: Missing key 'siteRoot'.");
            Environment.Exit(1);
        }
        if(!config.SiteRoot.Exists)
        {
            Console.WriteLine("Config error: Directory specified by 'siteRoot' does not exist.");
            Environment.Exit(1);
        }

        if(config.DefaultPage == null)
        {
            Console.WriteLine("Config error: Missing key 'DefaultPage'.");
            Environment.Exit(1);
        }

        return config;
    }
}