using Discord;
using Discord.WebSocket;
using Malaco5.Entities;
namespace Malaco5;
public class Malaco5()
{
    public static DiscordSocketClient? _client;
    public static CommandHandler? commandHandler;
    public static DateTime startTime;

    public static void Main(string[] args)
    {
        Print("Booting Malaco 5...");
        while (!File.Exists("token.txt"))
        {
            Print("Token Not Found, Please Enter Token: ", ConsoleColor.White, ConsoleColor.Black, false);
            string? t = Console.ReadLine();
            File.WriteAllText("token.txt", t);
        }
        Print("Starting Asyncronous Operation...");
        MainAsync().GetAwaiter().GetResult();
    }

    public static async Task MainAsync()
    {
        _client = new DiscordSocketClient();

        _client.Log += LogClientMessage;
        _client.JoinedGuild += LogGuildJoin;
        _client.LeftGuild += LogGuildLeave;

        commandHandler = new CommandHandler(_client);

        await _client.LoginAsync(TokenType.Bot, File.ReadAllText("token.txt"));
        await _client.StartAsync();
        
        while (_client.ConnectionState != ConnectionState.Connected)
        {
            Thread.Sleep(100);
        }

        startTime = DateTime.Now;
        await Task.Delay(-1);
    }

    private static Task LogGuildLeave(SocketGuild arg)
    {
        Print($"Left Guild {arg.Name} <{arg.Id}>", ConsoleColor.Red);
        return Task.CompletedTask;
    }

    private static Task LogGuildJoin(SocketGuild arg)
    {
        Print($"Joined Guild {arg.Name} <{arg.Id}> owned by {arg.Owner.Username} <@{arg.OwnerId}>", ConsoleColor.Blue);
        return Task.CompletedTask;
    }

    private static Task LogClientMessage(LogMessage arg)
    {
        switch (arg.Severity)
        {
            default: return Task.CompletedTask;
            case LogSeverity.Info:
                Print(arg.Message);
                break;
            case LogSeverity.Warning:
                Print(arg.Message, ConsoleColor.Yellow);
                break;
            case LogSeverity.Error:
            case LogSeverity.Critical:
                Print($"{arg.Message}\n\t{arg.Exception}\n\n\t{arg.Source}", ConsoleColor.Red);
                break;
        }
        return Task.CompletedTask;
    }
    public static void Print(string text, ConsoleColor fg = ConsoleColor.Gray, ConsoleColor bg = ConsoleColor.Black, bool showTimestamp = true)
    {
        Console.ForegroundColor = fg;
        Console.BackgroundColor = bg;
        Console.WriteLine(showTimestamp ? $"{DateTime.Now.ToShortTimeString()} > {text}" : text);
        Console.ResetColor();
    }
}