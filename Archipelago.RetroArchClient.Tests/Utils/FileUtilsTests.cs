using Archipelago.RetroArchClient.Configuration;
using Archipelago.RetroArchClient.Utils;

namespace Archipelago.RetroArchClient.Tests.Utils;

public class FileUtilsTests
{
    private const string TestFileFolderName = "TestFiles";

    [Theory]
    [InlineData("config.valid.1.json")]
    public void TryLoadJsonFile_LoadsConfigurationFile(string fileName)
    {
        var expected = new ConfigurationSettings(
                new ArchipelagoServer("archipelago.gg", 38281, "", ""),
                new RetroArch("localhost", 55355));

        var path = Path.Combine(Environment.CurrentDirectory, TestFileFolderName, fileName);
        var success = FileUtils.TryLoadJsonFile(path, out ConfigurationSettings result);


        Assert.True(success);
        Assert.Equal(expected, result);

    }

}
