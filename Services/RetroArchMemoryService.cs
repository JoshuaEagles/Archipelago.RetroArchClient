using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Archipelago.RetroArchClient.Models;
using Archipelago.RetroArchClient.Services.Interfaces;
using Archipelago.RetroArchClient.Utils;

namespace Archipelago.RetroArchClient.Services;

// Maybe add documentation detailing what this service is for, what functions it provides, what the functions do, etc.
// Helps to get other developers interested in helping with the client up to speed.

// See OcarinaOfTime.Enums.GameModes for example on how this could be achieved.
public class RetroArchMemoryService(UdpClient udpClient) : IMemoryService
{
	/// <summary>
	///     Reads the requested number of bytes from memory at the target address.
	///     Because of the swizzled memory, this supports a max of 4 bytes read at a time, or less if the address isn't 4n-1.
	/// </summary>
	public async Task<byte[]> ReadMemoryToByteArray(long address, int numberOfBytes)
	{
		if (numberOfBytes > 4)
		{
			throw new ArgumentException("This method currently supports a max of 4 bytes read to get proper output.");
		}

		if (4 - address % 4 < numberOfBytes)
		{
			throw new ArgumentException("Requested bytes go beyond a single 4 byte chunk.");
		}

		var receivedBytes = await SendAndReceiveReadMemory(address, numberOfBytes);

		var dataFromMemory = RetroArchCommandStringUtils.ParseReadMemoryToArray(
			receivedString: receivedBytes,
			isBigEndian: true
		);

		return dataFromMemory;
	}

	public async Task<byte> Read8(long address) 
		=> (byte)await ReadMemoryToLong(address, 1);

	public async Task<ushort> Read16(long address)
		=> (ushort)await ReadMemoryToLong(address, 2);
	

	public async Task<uint> Read32(long address)
		=> (uint)await ReadMemoryToLong(address, 4);

	public async Task<Dictionary<long, long>> ReadMemoryToLongMulti(IEnumerable<MemoryReadCommand> readCommands)
	{
		var receivedStrings = await SendAndReceiveReadMemoryMulti(readCommands);
		var responses = new Dictionary<long, long>();
		
		foreach (var receivedString in receivedStrings)
		{
			var numberOfBytes = RetroArchCommandStringUtils.ParseNumberOfBytes(receivedString);
			var address = ConvertAddressFromN64(
				RetroArchCommandStringUtils.ParseAddress(receivedString),
				numberOfBytes
			);
			
			var data = RetroArchCommandStringUtils.ParseReadMemoryToLong(receivedString, true);
			responses.Add(address, data);
		}

		return responses;
	}

	public async Task<Dictionary<long, byte[]>> ReadMemoryToArrayMulti(IEnumerable<MemoryReadCommand> readCommands)
	{
		var receivedStrings = await SendAndReceiveReadMemoryMulti(readCommands);
		var responses = new Dictionary<long, byte[]>();
		
		foreach (var receivedString in receivedStrings)
		{
			var numberOfBytes = RetroArchCommandStringUtils.ParseNumberOfBytes(receivedString);
			
			var address = ConvertAddressFromN64(
				RetroArchCommandStringUtils.ParseAddress(receivedString),
				numberOfBytes
			);
			
			var data = RetroArchCommandStringUtils.ParseReadMemoryToArray(receivedString, true);
			responses.Add(address, data);
		}

		return responses;
	}

	// for now don't write more than 4 bytes at a time, this won't work if you do
	public async Task<int> WriteByteArray(long address, byte[] dataToWrite)
		=> await WriteMemory(address, dataToWrite.Reverse().ToArray());

	public async Task Write8(long address, byte dataToWrite) 
		=> await WriteMemory(address, NumberToByteArray(dataToWrite, 1));

	public async Task Write16(long address, ushort dataToWrite) 
		=> await WriteMemory(address, NumberToByteArray(dataToWrite, 2));

	public async Task Write32(long address, uint dataToWrite)
		=> await WriteMemory(address, NumberToByteArray(dataToWrite, 4));

	// Need to massively rethink this service, right now the performance is abysmal
	// The issue is that the way this works, parallel stuff is impossible
	// It sends out a command, and then it takes the next value from the socket as the response
	// It would be better if it stored the command sent out, and then just waited for any response, and then we could get ours based on the data sent out

	// Need to think about this more, two interesting places where this is done: 
	// https://github.com/DigidragonZX/Archipelago/blob/82e3f970e7c9e802a82afac9b908f4868edfb530/worlds/_retroarch/socket.py
	// https://github.com/WeaponizedEmoticon/gamehook/blob/685645c3a5946a476882c4e0d2bdedf1597e6b58/src/GameHook.Domain/Drivers/RetroArchUdpPollingDriver.cs#L72

	// One idea for a better performing version would be to allow stuff to be completely async, where you send out a command and pass a callback
	// and then when the response comes in the callback gets run
	// That would work for speeding up a lot of small separate calls, although it adds a bunch of complexity

	private async Task<long> ReadMemoryToLong(long address, int numberOfBytes)
	{
		if (numberOfBytes > 8)
		{
			throw new ArgumentException(
				"Can't read more than 8 bytes per command with this method as it returns long, use the byte[] version instead"
			);
		}

		var receivedString = await SendAndReceiveReadMemory(address, numberOfBytes);
		var dataFromMemory = RetroArchCommandStringUtils.ParseReadMemoryToLong(receivedString, true);

		return dataFromMemory;
	}

	private async Task<string> SendAndReceiveReadMemory(long address, int numberOfBytes)
	{
		var convertedAddress = ConvertAddressToN64(address, numberOfBytes);

		udpClient.Send(Encoding.UTF8.GetBytes($"READ_CORE_MEMORY {convertedAddress:X8} {numberOfBytes}"));

		var receivedBytes = (await udpClient.ReceiveAsync()).Buffer;

		return Encoding.UTF8.GetString(receivedBytes);
	}

	private async Task<List<string>> SendAndReceiveReadMemoryMulti(IEnumerable<MemoryReadCommand> readCommands)
	{
		const int commandsPerIteration = 50;

		var inMemoryReadCommands = readCommands.ToImmutableArray();
		var stringBuilder = new StringBuilder();
		var receivedStrings = new List<string>();
		var commandsExecuted = 0;
		
		while (commandsExecuted < inMemoryReadCommands.Length)
		{
			foreach (var readCommand in inMemoryReadCommands.Skip(commandsExecuted).Take(commandsPerIteration))
			{
				if (readCommand.NumberOfBytes > 4)
				{
					throw new ArgumentException(
						"This method currently supports a max of 4 bytes read to get proper output."
					);
				}

				if (4 - readCommand.Address % 4 < readCommand.NumberOfBytes)
				{
					throw new ArgumentException("Requested bytes go beyond a single 4 byte chunk.");
				}

				stringBuilder.Append(
					$"READ_CORE_MEMORY {ConvertAddressToN64(readCommand.Address, readCommand.NumberOfBytes):X8} {readCommand.NumberOfBytes}\n"
				);
			}

			udpClient.Send(Encoding.UTF8.GetBytes(stringBuilder.ToString()));
			stringBuilder.Clear();

			var responseCounter = 0;
			while (responseCounter < Math.Min(
					commandsPerIteration,
					inMemoryReadCommands.Length - commandsExecuted
				))
			{
				var receivedBytes = (await udpClient.ReceiveAsync()).Buffer;
				var receivedString = Encoding.UTF8.GetString(receivedBytes);
				receivedStrings.Add(receivedString);

				responseCounter++;
			}

			commandsExecuted += commandsPerIteration;
		}

		return receivedStrings;
	}

	private async Task<int> WriteMemory(long address, byte[] dataToWrite)
	{
		var receivedString = await SendAndReceiveWriteMemory(address, dataToWrite);

		var bytesWritten = RetroArchCommandStringUtils.ParseWriteMemoryBytesWritten(receivedString);

		return bytesWritten;
	}

	private async Task<string> SendAndReceiveWriteMemory(long address, byte[] dataToWrite)
	{
		var convertedAddress = ConvertAddressToN64(address, dataToWrite.Length);
		var dataToWriteString = string.Join(' ', dataToWrite.Select(b => $"{b:X2}"));
		var str = $"WRITE_CORE_MEMORY {convertedAddress:X8} {dataToWriteString}";
		var bytes = Encoding.UTF8.GetBytes(str);
		
		udpClient.Send(bytes);

		var receivedBytes = (await udpClient.ReceiveAsync()).Buffer;

		return Encoding.UTF8.GetString(receivedBytes);
	}

	private static long ConvertAddressToN64(long address, int numberOfBytes)
	{
		// The XOR 3 here is done because the memory within each 4 byte chunk is reversed, aka swizzled
		var translatedAddress = (address ^ 3) - (numberOfBytes - 1);

		return translatedAddress;
	}

	private static long ConvertAddressFromN64(long address, int numberOfBytes)
	{
		var translatedAddress = (address + (numberOfBytes - 1)) ^ 3;

		return translatedAddress;
	}

	// Outputs big endian byte array
	private static byte[] NumberToByteArray(long number, int numberOfBytes)
	{
		var outputByteArray = new byte[numberOfBytes];
		var offset = 8 * (numberOfBytes - 1);
		
		for (var i = 0; i < numberOfBytes; i++)
		{
			outputByteArray[numberOfBytes - i - 1] = (byte)(number >> offset);
			offset -= 8;
		}

		return outputByteArray.ToArray();
	}
}
