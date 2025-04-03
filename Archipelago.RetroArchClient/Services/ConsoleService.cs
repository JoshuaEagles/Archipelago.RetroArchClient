using Archipelago.RetroArchClient.Services.Interfaces;

namespace Archipelago.RetroArchClient.Services;

public class ConsoleService : IConsoleService
{
	public void WriteLine(string message)
	{
		Console.WriteLine(message);
	}

	public string? ReadLine()
	{
		return Console.ReadLine();
	}
}

