using Archipelago.RetroArchClient.Services;
using Archipelago.RetroArchClient.Services.Interfaces;

namespace Archipelago.RetroArchClient.Tests.OcarinaOfTime.Services
{
	public class UserPromptServiceTests
	{
		[Theory]
		[InlineData("y", true,  true)]
		[InlineData("y", false, true)]
		[InlineData("n", true,  false)]
		[InlineData("n", false, false)]
		[InlineData("",  true,  true)]
		[InlineData("",  false, false)]
		[InlineData("abc",  true,  true)]
		[InlineData("abc",  false, false)]
		public void PromptForBool_ReturnsExpectedValue(string input, bool defaultValue, bool expected)
		{
			//Given
			Console.SetIn(new StringReader(input));
			IUserPromptService sut = new UserPromptService();

			//When
			var result = sut.PromptForBool("yay or nay?", defaultValue);

			//Then
			Assert.Equal(expected, result);
		}
	}
}

