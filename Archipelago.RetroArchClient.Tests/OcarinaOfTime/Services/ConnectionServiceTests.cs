using Archipelago.RetroArchClient.Configuration;
using Archipelago.RetroArchClient.OcarinaOfTime.Services;
using Archipelago.RetroArchClient.Services.Interfaces;
using FluentAssertions;
using Moq;

namespace Archipelago.RetroArchClient.Tests.OcarinaOfTime.Services;

public class ConnectionServiceTests
{
    private readonly Mock<IUserPromptService> _mockUserPromptService;

    public ConnectionServiceTests() => _mockUserPromptService = new Mock<IUserPromptService>();

    [Fact]
    public void LoadOoTClientConnectionSettings_NoPrompt_LoadsSettings()
    {
        //Given
        var settings = new ConfigurationSettings(
            new ArchipelagoServer("ap.gg", 12345, "player1", "super secure password"),
            new RetroArch("retro", 67890));

        var promptManually = false;
        _mockUserPromptService
            .Setup(m => m.PromptForBool(It.IsAny<string>(), It.IsAny<bool>()))
            .Returns(promptManually);

        var connectionService = new ConnectionService(_mockUserPromptService.Object);

        //When
        var clientSettings = connectionService.LoadOoTClientConnectionSettings(settings);

        //Then
        clientSettings.Should().NotBeNull();
        clientSettings.ArchipelagoHostName.Should().Be(settings.ArchipelagoServer.Address);
        clientSettings.ArchipelagoPort.Should().Be(settings.ArchipelagoServer.Port);
        clientSettings.Password.Should().Be(settings.ArchipelagoServer.Password);
        clientSettings.SlotName.Should().Be(settings.ArchipelagoServer.SlotName);
        clientSettings.RetroArchHostName.Should().Be(settings.RetroArch.Address);
        clientSettings.RetroArchPort.Should().Be(settings.RetroArch.Port);
    }

    [Fact]
    public void LoadOoTClientConnectionSettings_WithManualPrompts_LoadsSettings()
    {
        //Given
        var connectionService = new ConnectionService(_mockUserPromptService.Object);

        var defaultSettings = new ConfigurationSettings(
            new ArchipelagoServer("ap.gg", 12345, "player1", "super secure password"),
            new RetroArch("retro", 67890));

        var archipelagoHostname = "coolsite.gg";
        var archipelagoPort = "11111";
        var archipelagoSlotname = "player2";
        var archipelagoPassword = "super duper secret password";
        var retroarchHostname = "arch";
        var retroarchPort = "22222";

        var settings = new ConfigurationSettings(
            new ArchipelagoServer(archipelagoHostname, int.Parse(archipelagoPort), archipelagoSlotname,
                archipelagoPassword),
            new RetroArch(retroarchHostname, int.Parse(retroarchPort)));

        var promptManually = true;
        _mockUserPromptService
            .Setup(m => m.PromptForBool(It.IsAny<string>(), It.IsAny<bool>()))
            .Returns(promptManually);

        _mockUserPromptService
            .SetupSequence(m => m.PromptForInput(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(archipelagoHostname)
            .Returns(archipelagoPort)
            .Returns(archipelagoSlotname)
            .Returns(archipelagoPassword)
            .Returns(retroarchHostname)
            .Returns(retroarchPort);

        //When
        var clientSettings = connectionService.LoadOoTClientConnectionSettings(defaultSettings);

        //Then
        clientSettings.Should().NotBeNull();
        clientSettings.ArchipelagoHostName.Should().Be(settings.ArchipelagoServer.Address);
        clientSettings.ArchipelagoPort.Should().Be(settings.ArchipelagoServer.Port);
        clientSettings.Password.Should().Be(settings.ArchipelagoServer.Password);
        clientSettings.SlotName.Should().Be(settings.ArchipelagoServer.SlotName);
        clientSettings.RetroArchHostName.Should().Be(settings.RetroArch.Address);
        clientSettings.RetroArchPort.Should().Be(settings.RetroArch.Port);
    }
}
