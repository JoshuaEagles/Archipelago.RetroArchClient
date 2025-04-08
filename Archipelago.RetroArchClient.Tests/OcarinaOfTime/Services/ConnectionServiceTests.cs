using FluentAssertions;
using Archipelago.RetroArchClient.Configuration;
using Archipelago.RetroArchClient.OcarinaOfTime.Services;
using Archipelago.RetroArchClient.Services.Interfaces;
using Moq;
using Archipelago.RetroArchClient.OcarinaOfTime.Services.Interfaces;

namespace Archipelago.RetroArchClient.Tests.OcarinaOfTime.Services;
public class ConnectionServiceTests
{
	private Mock<IUserPromptService> _mockUserPromptService;

	public ConnectionServiceTests()
	{
		_mockUserPromptService = new Mock<IUserPromptService>();
	}

	[Fact]
	public void LoadOoTClientConnectionSettings_NoPrompt_LoadsSettings()
	{
		//Given

		//When

		//Then
	}
}
