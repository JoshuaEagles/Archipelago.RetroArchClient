namespace Archipelago.RetroArchClient.Utils;

public static class ByteUtils
{
	public static bool CheckBit(long memoryToCheck, byte bitToCheck)
		=> (memoryToCheck & (1 << bitToCheck)) >= 1;
}
