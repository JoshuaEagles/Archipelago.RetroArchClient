using Archipelago.RetroArchClient.Configuration;
using Archipelago.RetroArchClient.Utils;

namespace Archipelago.RetroArchClient.Tests.Utils;

public class FileUtilsTests
{
    private const string TestFileFolderName = "TestFiles";

	// Mapping of test file name to its expected result
    public static IEnumerable<object[]> Data => 
		new List<object[]>
		{
			new object[] 
			{
				"config.valid.json", // all properties defined
				new ConfigurationSettings( 
					new ArchipelagoServer("archipelago.gg", 38281, "123", "456"), 
					new RetroArch("localhost", 55355))
			},
			new object[]
			{
				"config.valid.missingprops.json", // missing some properties
				new ConfigurationSettings( 
					new ArchipelagoServer("archipelago.gg", 38281, "123", ""), 
					new RetroArch("localhost", 55355))
			},
			new object[]
			{
				"config.valid.empty.json",  // empty json file
				new ConfigurationSettings( 
					new ArchipelagoServer("archipelago.gg", 38281, "", ""), 
					new RetroArch("localhost", 55355))
			},
		};

    [Theory]
	[MemberData(nameof(Data))]
    public void TryLoadJsonFile_LoadsConfigurationFile(string fileName, ConfigurationSettings expected)
    {
        var path = Path.Combine(Environment.CurrentDirectory, TestFileFolderName, fileName);
        var success = FileUtils.TryLoadJsonFile(path, out ConfigurationSettings result);

        Assert.True(success);
        Assert.Equal(expected, result);
    }

}
