using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.MessageLog.Messages;

namespace Archipelago.RetroArchClient.OcarinaOfTime;

public static class ClientLoggerService
{
    // Todo: This needs to be retrofitted onto UI applications later on.
    // Doing console for now.
    public static void LogServerMessage(LogMessage message)
    {
        switch (message)
        {
            case HintItemSendLogMessage hintItemSendLogMessage:
                WriteColoredText(hintItemSendLogMessage.Receiver.Name, ConsoleColor.Cyan);
                Console.Write("'s ");
                WriteColoredText(hintItemSendLogMessage.Item.ItemName,
                    GetItemClassColor(hintItemSendLogMessage.Item.Flags));
                Console.Write(" is in ");
                WriteColoredText(hintItemSendLogMessage.Sender.Name, ConsoleColor.Cyan);
                Console.Write("'s World (");
                WriteColoredText(hintItemSendLogMessage.Item.LocationName, ConsoleColor.Magenta);
                Console.Write(") [");
                WriteColoredText(hintItemSendLogMessage.IsFound ? "Found" : "Not found",
                    hintItemSendLogMessage.IsFound ? ConsoleColor.Green : ConsoleColor.Red);
                Console.Write("]\n");

                break;
            case ItemSendLogMessage itemSendLogMessage:
                var sender = itemSendLogMessage.Sender;
                var receiver = itemSendLogMessage.Receiver;

                if (sender == receiver)
                {
                    WriteColoredText(sender.Name, ConsoleColor.Cyan);
                    Console.Write(" found their ");
                }
                else
                {
                    WriteColoredText(sender.Name, ConsoleColor.Cyan);
                    Console.Write(" found ");
                    WriteColoredText(receiver.Name, ConsoleColor.Cyan);
                    Console.Write("'s ");
                }

                WriteColoredText(itemSendLogMessage.Item.ItemName, GetItemClassColor(itemSendLogMessage.Item.Flags));
                Console.Write(" (");
                WriteColoredText(itemSendLogMessage.Item.LocationName, ConsoleColor.Magenta);
                Console.Write(")\n");

                break;
            case JoinLogMessage joinLogMessage:
                WriteColoredText(joinLogMessage.Player.Name, ConsoleColor.Cyan);
                HandleJoinTags(joinLogMessage);
                WriteColoredText(joinLogMessage.Player.Game, ConsoleColor.DarkMagenta);
                Console.Write($" [{string.Join(", ", joinLogMessage.Tags)}] joined the game\n");
                break;
            case LeaveLogMessage leaveLogMessage:
                WriteColoredText(leaveLogMessage.Player.Name, ConsoleColor.Cyan);
                Console.Write(" left the game\n");
                break;
            case TagsChangedLogMessage tagsChangedLogMessage:
                WriteColoredText(tagsChangedLogMessage.Player.Name, ConsoleColor.Cyan);
                Console.Write($" changed their tags to [{string.Join(", ", tagsChangedLogMessage.Tags)}]\n");
                break;
            case ChatLogMessage chatLogMessage:
                WriteColoredText(chatLogMessage.Player.Name, ConsoleColor.Cyan);
                Console.Write($": {chatLogMessage.Message}\n");
                break;
        }
    }

    private static void WriteColoredText(string text, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.Write(text);
        Console.ResetColor();
    }

    private static ConsoleColor GetItemClassColor(ItemFlags flags) => flags switch
    {
        ItemFlags.Advancement => ConsoleColor.Yellow,
        ItemFlags.Trap => ConsoleColor.Red,
        ItemFlags.NeverExclude => ConsoleColor.DarkYellow,
        ItemFlags.None => ConsoleColor.Gray,
        _ => Console.ForegroundColor,
    };

    private static void HandleJoinTags(JoinLogMessage joinLogMessage)
    {
        if (joinLogMessage.Tags.Contains("TextOnly"))
        {
            Console.Write(" viewing ");
            return;
        }

        if (joinLogMessage.Tags.Contains("Tracker"))
        {
            Console.Write(" tracking ");
            return;
        }

        Console.Write(" playing ");
    }
}
