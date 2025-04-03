namespace Archipelago.RetroArchClient.Services.Interfaces
{
	/// <summary>
	/// Wrapper for Console operations.
	/// </summary>
	public interface IConsoleService
	{
		void WriteLine(string message);
		string? ReadLine();
	}
}

