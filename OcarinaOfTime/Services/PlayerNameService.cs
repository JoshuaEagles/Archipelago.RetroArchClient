using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Archipelago.OoTClient.Net.Services.Interfaces;

namespace Archipelago.OoTClient.Net.OcarinaOfTime.Services;

// Maybe add documentation detailing what this service is for, what functions it provides, what the functions do, etc.
// Helps to get other developers interested in helping with the client up to speed.

// See Enums.GameModes for example on how this could be achieved.
public class PlayerNameService(IMemoryService memoryService)
{
	public async Task WritePlayerName(byte index, string name)
	{
		const uint namesAddress = 0x80400034;
		const byte maxPlayerNameBytes = 8;

		var nameToWriteAddress = namesAddress + index * maxPlayerNameBytes;
		var bytesToWrite = new List<byte>(8);

		var asciiNameCharBytes = Encoding.ASCII.GetBytes(name);
		for (var i = 0; i < 8; i++)
		{
			var hasMoreCharacters = i < asciiNameCharBytes.Length;
			byte? charByte = hasMoreCharacters
				? asciiNameCharBytes[i]
				: null;

			charByte = charByte switch
			{
				>= 0x30 and <= 0x39 => (byte)(charByte - 0x30), // 0 to 9
				>= 0x41 and <= 0x5A => (byte)(charByte + 0x6A), // A to Z
				>= 0x61 and <= 0x7A => (byte)(charByte + 0x64), // a to z
				0x2E => 0xEA, // .
				0x2D => 0xE4, // -
				0x20 => 0xDF, // <space>
				_ => null,
			};

			if (charByte is null && hasMoreCharacters)
			{
				continue;
			}

			bytesToWrite.Add(charByte ?? 0xDF);
		}

		await memoryService.WriteByteArray((uint)nameToWriteAddress, bytesToWrite.Take(4).ToArray());
		await memoryService.WriteByteArray((uint)nameToWriteAddress + 4,  bytesToWrite.Skip(4).Take(4).ToArray());
	}
}
