namespace Archipelago.RetroArchClient.Services.Interfaces;

public interface IUserPromptService
{
    /// <summary>
    ///     Prompts the user to enter an optional value via the keyboard,
    ///     using a standard prompt format. If the user does not provide a value,
    ///     returns the default instead.
    /// </summary>
    /// <param name="name">The name of the value to prompt the user for.</param>
    /// <param name="defaultValue">The default value to return if the user does not enter a value.</param>
    /// <returns>The value returned</returns>
    string PromptForInput(string name, string defaultValue);


    /// <summary>
    ///     Similar to <see cref="PromptForInput(string, string)" />,
    ///     except the standard prompt is fully replaced with provided
    ///     <paramref name="prompt" /> instead.
    /// </summary>
    /// <param name="prompt">The prompt to send the user.</param>
    /// <param name="defaultValue">The default value to return if the user does not enter a value.</param>
    /// <example>The value entered by user, or default.</example>
    string CustomPromptForInput(string prompt, string defaultValue);


    /// <summary>
    ///     Prompts the user for a boolean answer (i.e. yes or no response)
    /// </summary>
    /// <param name="prompt">The prompt to send the user.</param>
    /// <param name="defaultValue">The default value to return if the user does not enter a value.</param>
    /// <returns>A <see cref="bool" /> based on response and default. (see examples)</returns>
    /// <example>
    ///     If the defaultValue is true, and the user does not provide a value, return true,
    ///     If the defaultValue is true, and the user enters a string starting with "n" or "N", then return false instead.
    ///     If the defaultValue is false, and the user does not provide a value, return false,
    ///     If the defaultValue is false, and the user enters a string starting with "y" or "Y", then return true instead.
    /// </example>
    bool PromptForBool(string prompt, bool defaultValue);
}
