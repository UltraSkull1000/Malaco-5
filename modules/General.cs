using Discord.Interactions;
using Malaco5.Entities;
using Oestus;
using static Malaco5.Modules.MalacoAutocompletes;

namespace Malaco5.Modules;

public class General() : InteractionModuleBase
{
    [SlashCommand("uptime", "Checks the uptime of the current shard", runMode: RunMode.Async)]
    public async Task Uptime()
    {
        await RespondAsync($"The current shard has been up for {(DateTime.Now - Malaco5.startTime).ToString()}.", ephemeral: true);
    }
    
    [SlashCommand("ping", "Checks the ping to the current shard", runMode: RunMode.Async)]
    public async Task Ping()
    {
        await RespondAsync($"*Pong! {(DateTime.Now - Context.Interaction.CreatedAt).ToString("fff")}ms*", ephemeral: true);
    }

    [SlashCommand("roll", "Rolls Dice!", runMode: RunMode.Async)]
    public async Task RollDice([Autocomplete(typeof(RollAutocomplete))] string query)
    {
        query = ProcessQuery(query);
        try
        {
            int result = Dice.Parse(query, out var resultString);
            if (resultString.Length > 1900)
            {
                File.WriteAllText($"{Context.Interaction.Id}.txt", resultString);
                await RespondWithFileAsync($"{Context.Interaction.Id}.txt", text: $"{Context.User.Mention} rolled `{query}` for a total of **{result}**!");
                File.Delete($"{Context.Interaction.Id}.txt");
            }
            else
                await RespondAsync($"{Context.User.Mention} rolled `{query.Replace("dm10", "rt")}` for a total of **{result}**! `{resultString}`");

            var u = User.GetUser(Context.User.Id, out var _);
            u.lastRoll = query;
            u.UpdateUser();
        }
        catch (Exception ex)
        {
            await RespondAsync(ex.Message, ephemeral: true);
        }
    }

    [SlashCommand("saveroll", "Save a roll that you use frequently!", runMode: RunMode.Async)]
    public async Task SaveRoll(string name, string roll)
    {
        roll = ProcessQuery(roll);
        User user = User.GetUser(Context.User.Id, out bool existed);
        user.savedRolls.Add(name, roll);
        if (existed)
            user.UpdateUser();
        else
            user.SaveUser();
        await RespondAsync($"Added roll `{name}` as `{roll}`!", ephemeral: true);
    }

    [SlashCommand("updateroll", "Update a roll that you've saved previously!")]
    public async Task UpdateRoll([Autocomplete(typeof(SavedAutocomplete))] string name, string roll)
    {
        roll = ProcessQuery(roll);
        User user = User.GetUser(Context.User.Id, out bool existed);
        if (existed)
        {
            if (user.savedRolls.ContainsKey(name))
            {
                user.savedRolls[name] = roll;
                user.UpdateUser();
                await RespondAsync($"Updated roll `{name}` to `{roll}`!", ephemeral: true);
            }
            else
            {
                user.savedRolls.Add(name, roll);
                user.UpdateUser();
                await RespondAsync($"Added roll `{name}` as `{roll}`!", ephemeral: true);
            }
        }
        else await RespondAsync("You have no saved rolls to delete!", ephemeral: true);

    }

    [SlashCommand("deleteroll", "Delete a roll that you've saved previously!")]
    public async Task DeleteRoll([Autocomplete(typeof(SavedAutocomplete))] string name)
    {
        User user = User.GetUser(Context.User.Id, out bool existed);
        if (existed)
        {
            user.savedRolls.Remove(name);
            user.UpdateUser();
            await RespondAsync($"Deleted roll `{name}`!", ephemeral: true);
        }
        else await RespondAsync("You have no saved rolls to delete!", ephemeral: true);
    }

    string ProcessQuery(string q) => q.Replace(" ", "").Replace("rt", "dm10").ToLower();
    
}