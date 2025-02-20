using Archipelago.RetroArchClient.OcarinaOfTime;

try
{
    var ootClient = new OoTClient();
    await ootClient.RunClient();
}
catch (Exception ex)
{
    Console.WriteLine(ex);
    Console.WriteLine("-----------------");
    Console.WriteLine("If you believe this to be a client error, report this to the Client Devs!");
    Console.WriteLine("Do not report this if this was caused by a wrongly made input like Slot Name or Password being wrong!");
    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();
}