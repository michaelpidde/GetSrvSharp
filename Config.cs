using Newtonsoft.Json;

public class Config {
    public const string ContentDirectory = "content";
    public const string TemplateDirectory = "template";
    public const string ErrorTemplateDirectory = "error";
    public string Host { get; set; }
    public int Port { get; set; }
    public DirectoryInfo SiteRoot { get; set; }
    public string DefaultPage { get; set; }
    public bool TemplateEngineEnabled { get; set; }
    public FileInfo LogFile { get; set; }

    // This gets created via json deserialization by passing a file into Get. Do not allow direct instantiation
    protected Config() { }

    public DirectoryInfo GetContentDirectory() {
        return GetDirectory(ContentDirectory, "Warning: Content directory does not exist.");
    }

    public DirectoryInfo GetTemplateDirectory() {
        return GetDirectory(TemplateDirectory, "Warning: Template directory does not exist.");
    }

    public DirectoryInfo GetErrorTemplateDirectory() {
        return GetDirectory(ErrorTemplateDirectory, "Notice: Error template directory does not exist.");
    }

    private DirectoryInfo GetDirectory(string findDirectory, string warning) {
        var directory = new DirectoryInfo(SiteRoot!.FullName + Path.DirectorySeparatorChar + findDirectory);
        if(!directory.Exists) {
            Console.WriteLine(warning);
        }
        return directory;
    }

    public static Config Get(FileInfo configFile) {
        static void Error(string message) {
            Console.WriteLine($"Config error: {message}.");
            Environment.Exit(1);
        }

        if(!configFile.Exists) {
            Error("Server configuration file does not exist");
        }

        JsonConverter[] converters = new JsonConverter[]
        {
            new FileInfoConverter(),
            new DirectoryInfoConverter()
        };
        Config? config = JsonConvert.DeserializeObject<Config>(configFile.OpenText().ReadToEnd(), converters);

        if(config == null) {
            Error("Cannot parse config file");
        }

        if(config!.Host == null) {
            Error("Config error: Missing key 'host'");
        }

        // If port is not in file it will set this to 0
        if(config.Port == 0) {
            Error("Missing key 'port' or invalid value");
        }

        if(config.SiteRoot == null) {
            Error("Missing key 'siteRoot'");
        }
        if(!config.SiteRoot!.Exists) {
            Error("Directory specified by 'siteRoot' does not exist");
        }

        if(config.DefaultPage == null) {
            Error("Missing key 'defaultPage'");
        }

        if(config.LogFile == null) {
            Error("Config error: Missing key 'logFile'");
        }
        if(!config.LogFile!.Exists) {
            Error("Config error: Log file does not exist");
        }

        return config;
    }
}
