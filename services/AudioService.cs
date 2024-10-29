using System.Diagnostics;
using Discord;
using Discord.Audio;

namespace Malaco5.Services;

public static class AudioService
{
    public static Dictionary<ulong, AudioServer> servers {get; set;} = new Dictionary<ulong, AudioServer>();

    public static AudioServer? GetAudioServer(ulong guildId, IAudioClient? audioClient = null, IVoiceChannel? vc = null, ITextChannel? tc = null) => servers.GetValueOrDefault(guildId) ?? AddAudioServer(guildId, audioClient, vc, tc);
    public static AudioServer? AddAudioServer(ulong guildId, IAudioClient? audioClient, IVoiceChannel? vc, ITextChannel? tc){
        if(audioClient == null || vc == null || tc == null)
            return null;
        if (servers.ContainsKey(guildId))
            return servers[guildId];
        var server = new AudioServer(audioClient, vc, tc);
        servers.Add(guildId, server);
        return server;
    }
}

public class AudioServer{
    public Track? currentTrack;
    public Queue<Track> queue;
    public Status status;
    public bool loop = false;
    IAudioClient audioClient;
    ITextChannel tc;
    IVoiceChannel vc;

    public enum Status
    {
        Idle,
        Playing
    }

    public AudioServer(IAudioClient audioClient, IVoiceChannel vc, ITextChannel tc)
    {
        this.audioClient = audioClient;
        this.vc = vc;
        this.tc = tc;
        queue = new Queue<Track>();
        status = Status.Idle;
        AudioLapsed += (s, e) =>
        {
            try
            {
                PlayNext();
            }
            catch (Exception ex)
            {   
                if(tc != null)
                    tc.SendMessageAsync(ex.Message).GetAwaiter().GetResult();
                return;
            }
        };
    }

    private Process CreateStream(string path)
    {
        var x = Process.Start(new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
            UseShellExecute = false,
            RedirectStandardOutput = true,
        });
        if (x == null)
            throw new Exception("ffmpeg Process returned null.");
        return x;
    }

    public void Disconnect()
    {
        audioClient.StopAsync();
    }

    public void Enqueue(Track track)
    {
        queue.Enqueue(track);
        if (status == Status.Idle)
            PlayNext();
    }

    public void PlayNext()
    {
        if (queue.Count() == 0)
            throw new NullReferenceException("Queue is empty.");
        Play(queue.Dequeue()).GetAwaiter().GetResult();
        status = Status.Idle;
    }

    public async Task Play(Track track)
    {
        status = Status.Playing;
        using (var ffmpeg = CreateStream(track.path))
        using (var output = ffmpeg.StandardOutput.BaseStream)
        using (var discord = audioClient.CreatePCMStream(AudioApplication.Mixed))
        {
            try
            {
                currentTrack = track;
                await output.CopyToAsync(discord);
            }
            finally
            {
                await discord.FlushAsync();
                currentTrack = null;
                AudioLapsed?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public event EventHandler AudioLapsed;
}

public class Track
{
    public string path;
    public string name;

    public Track(string path, string name)
    {
        this.path = path;
        this.name = name;
    }
}