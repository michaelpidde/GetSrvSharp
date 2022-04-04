using Newtonsoft.Json;

public class DirectoryInfoConverter : JsonConverter<DirectoryInfo> {
    public override DirectoryInfo? ReadJson(JsonReader reader, Type objectType, DirectoryInfo? existingValue, bool hasExistingValue, JsonSerializer serializer) {
        try {
            return new DirectoryInfo((string)reader.Value);
        } catch {
            return null;
        }
    }

    public override void WriteJson(JsonWriter writer, DirectoryInfo? value, JsonSerializer serializer) { }
}

public class FileInfoConverter : JsonConverter<FileInfo> {
    public override FileInfo? ReadJson(JsonReader reader, Type objectType, FileInfo? existingValue, bool hasExistingValue, JsonSerializer serializer) {
        try {
            return new FileInfo((string)reader.Value);
        } catch {
            return null;
        }
    }

    public override void WriteJson(JsonWriter writer, FileInfo? value, JsonSerializer serializer) { }
}

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

    public DirectoryInfo GetContentDirectory() =>
        GetDirectory(ContentDirectory, "Warning: Content directory does not exist.");

    public DirectoryInfo GetTemplateDirectory() =>
        GetDirectory(TemplateDirectory, TemplateEngineEnabled ? "Warning: Template directory does not exist." : null);

    public DirectoryInfo GetErrorTemplateDirectory() => GetDirectory(ErrorTemplateDirectory);

    private DirectoryInfo GetDirectory(string findDirectory, string? warning = null) {
        var directory = new DirectoryInfo(SiteRoot!.FullName + Path.DirectorySeparatorChar + findDirectory);
        if(!directory.Exists)
            Console.WriteLine(warning);
        return directory;
    }

    public static Config Get(FileInfo configFile) {
        static void Error(string message) {
            Console.WriteLine($"Config error: {message}.");
            Environment.Exit(1);
        }

        if(!configFile.Exists)
            Error("Server configuration file does not exist");

        JsonConverter[] converters = new JsonConverter[]
        {
            new FileInfoConverter(),
            new DirectoryInfoConverter()
        };

        Config? config = null;
        try {
            config = JsonConvert.DeserializeObject<Config>(configFile.OpenText().ReadToEnd(), converters);
        } catch(Exception e) {
            Error($"Cannot parse config file: {e.Message}");
        }

        if(config!.Host == null)
            Error("Config error: Missing key 'host'");

        // If port is not in file it will set this to 0
        if(config.Port == 0)
            Error("Missing key 'port' or invalid value");

        if(config.SiteRoot == null)
            Error("Missing key 'siteRoot' or invalid value");
        if(!config.SiteRoot!.Exists)
            Error("Directory specified by 'siteRoot' does not exist");

        if(config.DefaultPage == null)
            Error("Missing key 'defaultPage' or invalid value");

        if(config.LogFile == null)
            Error("Config error: Missing key 'logFile' or invalid value");
        if(!config.LogFile!.Exists)
            Error("Config error: Log file does not exist");

        return config;
    }
}
