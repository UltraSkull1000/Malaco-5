using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Malaco5.Entities;
using Malaco5.Services;
using static Malaco5.Modules.MalacoAutocompletes;

namespace Malaco5.Modules;

[Group("music", "Controls for music-based commands.")]
public class Music() : InteractionModuleBase
{   
    [SlashCommand("join", "Joins a voice channel for audio playback.")]
    public async Task Join(IVoiceChannel? channel = null){
        channel = channel ?? ((IGuildUser)Context.User).VoiceChannel;
        if(channel == null){
            await RespondAsync("No channel specified.", ephemeral: true);
            return;
        }

        var audioClient = await channel.ConnectAsync(disconnect:false);
        AudioService.AddAudioServer(Context.Guild.Id, audioClient, channel, await Context.Guild.GetTextChannelAsync(Context.Interaction.ChannelId.GetValueOrDefault()));

        await RespondAsync($"Joined channel {channel.Name}", ephemeral:true);
    }

    [SlashCommand("disconnect", "Disconnects from a voice channel.")]
    public async Task Disconnect(){
        var audioServer = AudioService.GetAudioServer(Context.Guild.Id);
        if(audioServer == null){
            await RespondAsync("Already disconnected.", ephemeral:true);
            return;
        }
        audioServer.Disconnect();
        await RespondAsync("Disconnected.", ephemeral:true);
    }

    [SlashCommand("play", "Plays audio from a local path.")]
    public async Task Play(string relativePath){
        if(relativePath.Contains("C:"))
            return;
        var audioServer = AudioService.GetAudioServer(Context.Guild.Id);
        if(audioServer == null){
            var channel = ((IGuildUser)Context.User).VoiceChannel;
            if(channel == null){
                await RespondAsync("Please connect to a voice channel before attempting to add music.", ephemeral:true);
                return;
            }
            var audioClient = await channel.ConnectAsync(disconnect:false);
            audioServer = AudioService.AddAudioServer(Context.Guild.Id, audioClient, channel, await Context.Guild.GetTextChannelAsync(Context.Interaction.ChannelId.GetValueOrDefault()));
            if(audioServer == null){
                await RespondAsync("An error has occurred.", ephemeral:true);
                return;
            }
        }
        audioServer.Enqueue(new Track($"music/{relativePath}", relativePath));
        await RespondAsync("Added to queue.", ephemeral:true);
    }
}