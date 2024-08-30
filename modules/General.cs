using Discord;
using Discord.Interactions;
using Oestus;

public class General() : InteractionModuleBase
{
    [SlashCommand("roll", "Rolls Dice!", runMode: RunMode.Async)]
    public async Task RollDice(string query)
    {
        query = query.Replace(" ", "");
        query = query.ToLower();
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
                await RespondAsync($"{Context.User.Mention} rolled `{query}` for a total of **{result}**! `{resultString}`");
        }
        catch (Exception ex)
        {
            await RespondAsync(ex.Message, ephemeral: true);
        }
    }

    public class DiceAutocompleteHandler : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            IEnumerable<AutocompleteResult> results = new[] {
                new AutocompleteResult("1d20","1d20"),
                new AutocompleteResult("1d12","1d12"),
                new AutocompleteResult("1d10","1d10"),
                new AutocompleteResult("1d8", "1d8"),
                new AutocompleteResult("1d6", "1d6"),
                new AutocompleteResult("1d4", "1d4"),
                new AutocompleteResult("1df", "1df")
            };
            return AutocompletionResult.FromSuccess(results);
        }
    }
}