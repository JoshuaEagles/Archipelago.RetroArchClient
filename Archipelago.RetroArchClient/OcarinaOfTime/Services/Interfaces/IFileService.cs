namespace Archipelago.RetroArchClient.OcarinaOfTime.Services.Interfaces;

public interface IFileService
{
    /// <summary>
    ///     Returns the path to a file under the current directory.
    /// </summary>
    /// <param name="parameterName">The name of the file.</param>
    /// <returns>The full path to the file, under the current running directory.</returns>
    /// <example>
    ///     If the process was running at /home/bob/service/ and the
    ///     <paramref name="fileName" /> was "foo", method would
    ///     return "/home/bob/service/foo"
    /// </example>
    string GetFilePathAtCurrentDirectory(string fileName);

    /// <summary>
    ///     Attempt to load a JSON file and deserialize it to the declared type.
    /// </summary>
    /// <param name="path">The full path to the file.</param>
    /// <param name="result">The result of the file load attempt, if successful.</param>
    /// <returns>Whether the file was deserialized successfully.</returns>
    bool TryLoadJsonFile<T>(string path, out T result)
        where T : class, new();
}
