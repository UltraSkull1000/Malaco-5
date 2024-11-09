using System.Text;
using Discord.Interactions;
using Malaco5.Entities;
using Oestus;
using static Malaco5.Modules.MalacoAutocompletes;

namespace Malaco5.Modules;

public class General() : InteractionModuleBase
{
    [SlashCommand("uptime", "Checks the uptime of the current shard")]
    public async Task Uptime()
    {
        await RespondAsync($"The current shard has been up for {(DateTime.Now - Malaco5.startTime).ToString()}.", ephemeral: true);
    }

    [SlashCommand("ping", "Checks the ping to the current shard")]
    public async Task Ping()
    {
        await RespondAsync($"*Pong! {(DateTime.Now - Context.Interaction.CreatedAt).ToString("fff")}ms*", ephemeral: true);
    }

    public string Roll(string query, out string filepath)
    {
        query = ProcessQuery(query);
        filepath = "";
        int result = Dice.Parse(query, out var resultString);
        if (resultString.Length > 1900)
        {
            filepath = $"{Context.Interaction.Id}.txt";
            if (!File.Exists(filepath))
                File.WriteAllText(filepath, resultString);
            else
            {
                var writer = File.OpenWrite(filepath);
                writer.Write(Encoding.UTF8.GetBytes($"\n{resultString}"));
                writer.Close();
            }
        }
        return $"**{result}**! `{resultString}`";
    }

    [SlashCommand("roll", "Rolls Dice!")]
    public async Task RollDice([Autocomplete(typeof(RollAutocomplete))] string query)
    {
        var res = Roll(query, out string file);
        if (file != "")
            await RespondWithFileAsync(file, text: res);
        else await RespondAsync($"{Context.User.Mention} rolled `{query.Replace("dm10", "rt")}` for a total of {res}!");
        
        var u = User.GetUser(Context.User.Id, out var _);
        u.lastRoll = query;
        u.UpdateUser();
    }

    [SlashCommand("rollmany", "Rolls Multiple sets of Dice!")]
    public async Task RollManyDice([Autocomplete(typeof(RollAutocomplete))] string query, int repeat)
    {
        if (repeat == 1){
            await RollDice(query);
            return;
        }

        if(repeat > 16){
            await RespondAsync("You don't need to roll that many dice!", ephemeral:true);
            return;
        }

        List<string> result = new List<string>();
        bool exception = false;
        Exception? exc = null;

        for(int i = 0; i < repeat; i++)
        {
            try{
            result.Add(Roll(query, out var filepath));
            }
            catch(Exception ex){
                exception = true;
                exc = ex;
            }
        }

        if(exception && exc != null){
            await RespondAsync(exc.Message, ephemeral:true);
            return;
        }
        await RespondAsync($"{Context.User.Mention} rolled `{query}` {repeat} times! \n\n{string.Join("\n", result)}");

        var u = User.GetUser(Context.User.Id, out var _);
        u.lastRoll = query;
        u.UpdateUser();
    }

    [SlashCommand("saveroll", "Save a roll that you use frequently!")]
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