using Newtonsoft.Json;

public class DirectoryInfoConverter : JsonConverter<DirectoryInfo>
{
    public override DirectoryInfo ReadJson(JsonReader reader, Type objectType, DirectoryInfo existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        string path = (string)reader.Value;
        return new DirectoryInfo(path);
    }

    public override void WriteJson(JsonWriter writer, DirectoryInfo? value, JsonSerializer serializer) { }
}

public class FileInfoConverter : JsonConverter<FileInfo>
{
    public override FileInfo? ReadJson(JsonReader reader, Type objectType, FileInfo? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        string file = (string)reader.Value;
        return new FileInfo(file);
    }

    public override void WriteJson(JsonWriter writer, FileInfo? value, JsonSerializer serializer) { }
}