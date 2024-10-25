using Discord.WebSocket;
using Oestus;
using Timer = System.Timers.Timer;

namespace Malaco5;

public class StatusHandler{
    private DiscordSocketClient? _client;
    private string[] statuses = [];
    public StatusHandler(DiscordSocketClient _client){
        this._client = _client;
        if(!File.Exists("status.txt"))
            return;
        statuses = File.ReadAllLines("status.txt");
        _client.Ready += OnReady;
    }

    public async Task OnReady(){
        Malaco5.Print($"Client is Ready, Preparing Status Handler...");
        if(_client == null){
            throw new NullReferenceException(nameof(_client));
        }
        _client.Ready -= OnReady;
        await _client.SetCustomStatusAsync($"Malaco Successfully Started! {DateTime.Now.ToShortTimeString()}");
        var timer = new Timer(20000);
        timer.Elapsed += async (s, e) => await _client.SetCustomStatusAsync(statuses[OestusRNG.Next(0,statuses.Length)]);
        timer.Start();
        Malaco5.Print("Finished Preparing Status Handler.");
    }
}