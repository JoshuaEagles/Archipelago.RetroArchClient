using Archipelago.RetroArchClient.Configuration;
using Archipelago.RetroArchClient.OcarinaOfTime.Services;
using Archipelago.RetroArchClient.OcarinaOfTime.Services.Interfaces;
using Archipelago.RetroArchClient.Services.Interfaces;
using FluentAssertions;
using Moq;

namespace Archipelago.RetroArchClient.Tests.OcarinaOfTime.Services;

public class ConfigurationServiceTests
{
    private readonly Mock<IUserPromptService> _mockUserPromptService;
    private readonly Mock<IFileService> _mockFileService;

    public ConfigurationServiceTests()
    {
        _mockUserPromptService = new Mock<IUserPromptService>();
        _mockFileService = new Mock<IFileService>();
    }

    private readonly Func<ConfigurationSettings> GetTestConfigurationSettings =
        () => new ConfigurationSettings(
            new ArchipelagoServer("123", 123, "abc", "def"),
            new RetroArch("123", 123));


    [Fact]
    public void LoadConfigurationSettings_WithDefaultUserResponse_LoadsDefaultFile()
    {
        //Given
        var expectedConfigurationSettings = GetTestConfigurationSettings();
        var configService = new TestConfigurationService(_mockUserPromptService.Object, _mockFileService.Object, true,
            expectedConfigurationSettings);

        // Default response
        _mockUserPromptService
            .Setup(m => m.PromptForInput(It.IsAny<string>(), It.IsAny<string>()))
            .Returns("path/to/file/config.json");

        _mockFileService
            .Setup(m => m.GetFilePathAtCurrentDirectory("config.json"))
            .Returns("path/to/file/config.json");


        // Returns file successfully
        _mockFileService
            .Setup(m => m.TryLoadJsonFile("path/to/file/config.json",
                out expectedConfigurationSettings))
            .Returns(true);

        //When
        var actualConfigurationSettings = configService.LoadConfigurationSettings();

        //Then
        actualConfigurationSettings
            .Should()
            .BeEquivalentTo(expectedConfigurationSettings);
    }
}

public class TestConfigurationService : ConfigurationService
{
    private readonly ConfigurationSettings _configurationSettings;
    private readonly bool _success;

    public TestConfigurationService(
        IUserPromptService userPromptService,
        IFileService fileService,
        bool success,
        ConfigurationSettings configurationSettings
    )
        : base(userPromptService, fileService)
    {
        _success = success;
        _configurationSettings = configurationSettings;
    }

    // return whatever was provided in constructor
    protected virtual bool TryLoadConfigFile(string filePath, out ConfigurationSettings configFile)
    {
        configFile = _configurationSettings;
        return _success;
    }
}
