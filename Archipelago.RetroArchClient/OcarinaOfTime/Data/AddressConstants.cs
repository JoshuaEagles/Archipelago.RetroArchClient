namespace Archipelago.RetroArchClient.OcarinaOfTime.Data;

/// <summary>
///     Contains all constant addresses, pointers and offsets that are being used
///     throughout the client.
/// </summary>
public class AddressConstants
{
	// Addresses
	public const long CurrentSceneAddress = 0xA01C8544;
	public const long ScenePointerAddress = 0xA01CA208;
	public const long BigPoesRequiredAddress = 0xA0400EAD;
	public const long BigPoePointsAddress = 0xA011B48C;
	public const long EventContextAddress = 0xA011B4A4;
	public const long GetInfoStartAddress = 0xA011B4C0;
	public const long InfoTableStartAddress = 0xA011B4C8;
	public const long FishingContextAddress = 0xA011B490;

	public const uint LocalReceivedItemsCountAddress = 0xA011A660;
	public const uint IncomingPlayerAddress = 0xA0400026;
	public const uint IncomingItemAddress = 0xA0400028;

	// Pointers
	public const long TriforceHuntCompleteCreditsCutscenePointer = 0x80383C10;
	public const long GanonDefeatedCutscenePointer = 0x80382720;

	// Offsets
	public const long MainStateOffset = 0xA011B92F;
	public const long SubStateOffset = 0xA011B933;
	public const long MenuStateOffset = 0xA01D8DD5;
	public const long LogoStateOffset = 0xA011F200;
	public const long LinkStateOffset = 0xA01DB09C;
	public const long LinkHealthOffset = 0xA011A600;
	public const long ShopContextOffset = 0xA011AB84;
	public const long EquipmentOffset = 0xA011A640;
	public const long SkulltulaFlagsOffset = 0xA011B46C;
	public const long SceneFlagsOffset = 0xA011A6A4;
}