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
    public Embed GetQueueEmbed(AudioServer audioServer)
    {
        EmbedBuilder builder = new EmbedBuilder(){
            Title = "Queue"
        };

        if(audioServer.currentTrack == null){
            builder.WithDescription("Queue is currently empty.");
            builder.WithColor(Color.Red);
            return builder.Build();
        }
        builder.WithColor(Color.Blue);
        builder.WithDescription($"Loop: {(audioServer.loop ? "On" : "Off")}");
        builder.AddField("Current Track:", audioServer.currentTrack.name);
        for(int i = 0; i < 5; i++){
            if(i > audioServer.queue.Count()-1)
                break;
            builder.AddField($"{i+1}.", audioServer.queue.ElementAt(i).name);
        }
        return builder.Build();
    }
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
        AudioService.DeleteAudioServer(Context.Guild.Id);
        await RespondAsync("Disconnected.", ephemeral:true);
    }

    [SlashCommand("play", "Plays audio from a local path.")]
    public async Task Play([Autocomplete(typeof(MusicAutocomplete))]string relativePath){
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
        audioServer.Enqueue(new Track(relativePath, relativePath));
        await RespondAsync("Added to queue.", embed:GetQueueEmbed(audioServer), ephemeral:true);
    }

    [SlashCommand("queue", "Lists the tracks in the current queue.")]
    public async Task Queue(){
        var audioServer = AudioService.GetAudioServer(Context.Guild.Id);
        if(audioServer == null){
            await RespondAsync("No current queue.", ephemeral:true);
            return;
        }
        await RespondAsync($"{audioServer.queue.Count()} items in queue.", embed:GetQueueEmbed(audioServer));
    }

    [SlashCommand("loop", "Loops the current track.")]
    public async Task Loop(){
        var audioServer = AudioService.GetAudioServer(Context.Guild.Id);
        if(audioServer == null){
            await RespondAsync("No current queue.", ephemeral:true);
            return;
        }
        audioServer.loop = true;
        await RespondAsync($"Loop enabled.", embed:GetQueueEmbed(audioServer), ephemeral:true);
    }
}