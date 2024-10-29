using System.Linq.Expressions;
using System.Reflection;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Malaco5.Entities;
using Malaco5.Modules;
using Malaco5.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Malaco5;
public class CommandHandler
{
    private DiscordSocketClient? _client;
    private InteractionService _interactionService;
    private IServiceProvider _serviceProvider;
    public CommandHandler(DiscordSocketClient _client)
    {
        this._client = _client;
        _interactionService = new InteractionService(_client);
        _serviceProvider = SetupServices();
        _client.Ready += OnReady;

        Malaco5.Print("Initialized Command Handler!");
    }
    public async Task OnReady(){
        Malaco5.Print($"Client is Ready, Preparing Services...");
        if(_client == null){
            throw new NullReferenceException(nameof(_client));
        }
        _client.Ready -= OnReady;
        await _interactionService.AddModulesAsync(Assembly.GetExecutingAssembly(), _serviceProvider);
        await _interactionService.RegisterCommandsGloballyAsync();
        _client.InteractionCreated += HandleInteraction;
        Malaco5.Print("Finished Preparing Services.");
    }
    private async Task HandleInteraction(SocketInteraction interaction){
        User.EnsureUser(interaction.User.Id);
        var ctx = new SocketInteractionContext(_client, interaction);
        await _interactionService.ExecuteCommandAsync(ctx, _serviceProvider);
    }

    private IServiceProvider SetupServices()
    => new ServiceCollection()
    .AddSingleton(_interactionService)
    .AddSingleton(typeof(General))
    .AddSingleton(typeof(Music))
    .AddSingleton(typeof(MalacoAutocompletes))
    .BuildServiceProvider();
}