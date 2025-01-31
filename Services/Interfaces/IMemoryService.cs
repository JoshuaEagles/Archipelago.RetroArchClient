using System.Collections.Generic;
using System.Threading.Tasks;
using Archipelago.OoTClient.Net.Models;

namespace Archipelago.OoTClient.Net.Services.Interfaces;

// Maybe add documentation detailing what this interface is for, what functions it defines, what the functions do, etc.
// Helps to get other developers interested in helping with the client up to speed.

// See OcarinaOfTime.Enums.GameModes for example on how this could be achieved.
public interface IMemoryService
{
	Task<byte[]> ReadMemoryToByteArray(long address, int numberOfBytes);

	Task<byte> Read8(long address);

	Task<ushort> Read16(long address);

	Task<uint> Read32(long address);

	Task<Dictionary<long, long>> ReadMemoryToLongMulti(IEnumerable<MemoryReadCommand> readCommands);

	Task<Dictionary<long, byte[]>> ReadMemoryToArrayMulti(IEnumerable<MemoryReadCommand> readCommands);

	// Input Array should be in little endian (e.g. 0x01F4 = 500 in base 10)
	Task<int> WriteByteArray(long address, byte[] dataToWrite);

	Task Write8(long address, byte dataToWrite);

	Task Write16(long address, ushort dataToWrite);

	Task Write32(long address, uint dataToWrite);
}
