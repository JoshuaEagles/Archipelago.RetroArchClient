using Archipelago.RetroArchClient.Configuration;
using Archipelago.RetroArchClient.OcarinaOfTime.Services;
using FluentAssertions;

namespace Archipelago.RetroArchClient.Tests.Services;

public class FileServiceTests
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
		//Given
		var fileService = new FileService();
		var currentDir = System.Environment.CurrentDirectory;
        var path = Path.Combine(currentDir, TestFileFolderName, fileName);

		//When
        var fileLoadedSuccessfully = fileService.TryLoadJsonFile(path, out ConfigurationSettings result);

		//Then
		fileLoadedSuccessfully.Should().BeTrue();
		expected.Should().BeEquivalentTo(result);
    }

}
