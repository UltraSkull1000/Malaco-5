using Discord;
using Discord.Interactions;
using Malaco5.Entities;

namespace Malaco5.Modules;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

// Warning is disabled because despite the lack of need for asynchronous operation in this function, Discord.Net requires the async flag on these functions. 
public class MalacoAutocompletes
{
    public class RollAutocomplete : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            List<AutocompleteResult> suggestions = new List<AutocompleteResult>() {
                new AutocompleteResult("d20", "1d20"),
                new AutocompleteResult("d12", "1d12"),
                new AutocompleteResult("d10", "1d10"),
                new AutocompleteResult("d8", "1d8"),
                new AutocompleteResult("d6", "1d6"),
                new AutocompleteResult("d4", "1d4")
            };

            var user = User.GetUser(context.User.Id, out var _);
            if (user.lastRoll != null && user.lastRoll != "")
                suggestions.Insert(0, new AutocompleteResult($"(last) {user.lastRoll}", user.lastRoll));
            if (user.savedRolls.Count() > 0)
            {
                var options = user.savedRolls;
                foreach (var o in options)
                {
                    suggestions.Add(new AutocompleteResult(o.Key, o.Value));
                }
            }

            string current = (string)autocompleteInteraction.Data.Current.Value;
            if (current != "")
                suggestions.Insert(0, new AutocompleteResult(current, current));

            return AutocompletionResult.FromSuccess(suggestions.Take(25));
        }
    }

    public class SavedAutocomplete : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            List<AutocompleteResult> suggestions = new List<AutocompleteResult>();

            var user = User.GetUser(context.User.Id, out var _);
            if (user != null)
            {
                var options = user.savedRolls;
                foreach (var o in options)
                {
                    suggestions.Add(new AutocompleteResult(o.Key, o.Key));
                }
            }

            return AutocompletionResult.FromSuccess(suggestions.Take(25));
        }
    }

    public class MusicAutocomplete : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            List<AutocompleteResult> suggestions = new List<AutocompleteResult>();
            string current = (string)autocompleteInteraction.Data.Current.Value;
            if(!Directory.Exists("music"))
                Directory.CreateDirectory("music");
            var files = Directory.GetFiles("music", $"{current}*");
            foreach (var file in files){
                suggestions.Add(new AutocompleteResult(file, file));
            }
            if(suggestions.Count() == 0)
                return AutocompletionResult.FromError(new NullReferenceException("No music found in folder."));
            return AutocompletionResult.FromSuccess(suggestions.Take(25));
        }
    }
}
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
