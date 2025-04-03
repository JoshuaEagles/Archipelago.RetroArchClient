using Archipelago.RetroArchClient.Services.Interfaces;

namespace Archipelago.RetroArchClient.Services;

public class UserPromptService : IUserPromptService
{
    /// <inheritdoc/>
    public string PromptForInput(string name, string defaultValue)
    {
		var message = $"Enter the {name}, or leave blank and press enter to use: \"{defaultValue}\": ";
		Console.WriteLine(message);
		var userResponse = Console.ReadLine();
		return string.IsNullOrWhiteSpace(userResponse) ? defaultValue : userResponse;
    }

	/// <inheritdoc/>
    public string CustomPromptForInput(string prompt, string defaultValue)
    {
		Console.WriteLine(prompt);
		var userResponse = Console.ReadLine();
		return string.IsNullOrWhiteSpace(userResponse) ? defaultValue : userResponse;
    }

	/// <inheritdoc/>
    public bool PromptForBool(string prompt, bool defaultValue)
    {
		var yesNoHint = defaultValue ? "(Y/n)" : "(y/N)";
		Console.WriteLine($"{prompt} {yesNoHint}: ");
        var answer = Console.ReadLine();

		if (string.IsNullOrWhiteSpace(answer))
		{
			return defaultValue;
		}

		// if the defaultValue is true, then we assume true unless the
		// response starts with 'n', and vice versa
		char oppositeChar = defaultValue ? 'n' : 'y';
		if (answer.ToLowerInvariant().StartsWith(oppositeChar))
		{
			return !defaultValue;
		}

		return defaultValue;
    }
}
