using Archipelago.RetroArchClient.OcarinaOfTime.Services.Interfaces;
using Newtonsoft.Json;

namespace Archipelago.RetroArchClient.OcarinaOfTime.Services;

public class FileService : IFileService
{
    /// <inheritdoc/>
    public string GetFilePathAtCurrentDirectory(string fileName) =>
		Path.Combine(System.Environment.CurrentDirectory, fileName);

    /// <inheritdoc/>
    public bool TryLoadJsonFile<T>(string path, out T result)
        where T : class, new()
    {
        // May want an async version at some point?
        result = new T();
        try 
        {
            if(!File.Exists(path))
            {
                Console.WriteLine($"File not found at path: {path} ");
                return false;
            }

            string jsonText = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(jsonText))
            {
                Console.WriteLine("File was empty.");
                return false;
            }

            var deserializedJson = JsonConvert.DeserializeObject<T>(jsonText);
            if (deserializedJson is null)
            {
                Console.WriteLine("Deserialization result was null.");
                return false;
            }

            Console.WriteLine($"Successfully loaded JSON file {path}");
            result = deserializedJson;
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception while loading file {path}: {ex}");
            return false;
        }
    }
}
