using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using IGDB;
using IGDB.Models;

using Microsoft.Extensions.Logging;

namespace TRUCSBot.Commands
{
    public class GameNightSuggestionCommands : BaseCommandModule
    {
        private readonly ILogger _logger;

        private readonly IGDBClient _igdb = new(Application.Current.Settings.IGDBClientId,
            Application.Current.Settings.IGDBClientSecret);

        public GameNightSuggestionCommands(ILogger<GameNightSuggestionCommands> logger)
        {
            _logger = logger;
        }

        [Command("suggestgame")]
        public async Task SuggestGame(CommandContext ctx,
            [Description("The title of the game you want to suggest")] [RemainingText] string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                await ctx.RespondAsync("Oi fucktard, you need to specify a game when you use !suggestgame.");
                return;
            }

            var embed = new DiscordEmbedBuilder
            {
                Title = title,
                Color = DiscordColor.White
            };

            try
            {
                var games = await _igdb.QueryAsync<Game>(IGDBClient.Endpoints.Games,
                    $"search \"{Sanitize(title)}\"; fields id,name,cover.*,involved_companies.company.name,platforms.name,summary,url,aggregated_rating;");
                if (games.Length > 0)
                {
                    var game = games.OrderBy(x => LevenshteinDistance(title.ToLower(), x.Name.ToLower()))
                        .ThenByDescending(x => x.AggregatedRating).First();
                    embed.Title = game.Name;
                    embed.Description = string.IsNullOrEmpty(game.Summary)
                        ? "No information is available for this title"
                        : game.Summary;
                    embed.Color = DiscordColor.Green;
                    embed.Url = game.Url;

                    var imgUrl = game.Cover?.Value.Url;

                    if (imgUrl != null && imgUrl.StartsWith("//"))
                    {
                        imgUrl = "https:" + imgUrl;
                    }

                    embed.ImageUrl = imgUrl;
                    if (game.InvolvedCompanies?.Values.Length > 0)
                    {
                        embed.AddField("Created by",
                            string.Join(", ", game.InvolvedCompanies.Values.Select(x => x.Company.Value.Name)));
                    }

                    if (game.Platforms?.Values.Length > 0)
                    {
                        embed.AddField("Platforms", string.Join(", ", game.Platforms.Values.Select(x => x.Name)));
                    }
                }
                else
                {
                    embed.AddField("Additional Information", "Could not find game on IGDB.");
                }
            }
            catch (Exception ex) when (!Debugger.IsAttached)
            {
                await ctx.RespondAsync("An error occurred: " + ex.Message);
                _logger.LogError("Error occurred getting game night suggestion embed data", ex);
                return;
            }

            try
            {
                var message = await ctx.Message.Channel.Guild
                    .GetChannel(Debugger.IsAttached ? 691903205545607201ul : 766406856050343996ul)
                    .SendMessageAsync(embed: embed);
                await message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":thumbsup:"));
                await message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":thumbsdown:"));
                await message.CreateReactionAsync(DiscordEmoji.FromUnicode(ctx.Client, "💰"));
            }
            catch (Exception ex) when (!Debugger.IsAttached)
            {
                await ctx.RespondAsync("An error occurred: " + ex.Message);
                _logger.LogError("Error occurred posting game suggestion message", ex);
            }
        }

        private static string Sanitize(string s)
        {
            return Regex.Replace(s, @"([""\\])", @"\$1");
        }

        /// <summary>
        ///     The Levenshtein distance between two words is the minimum number of single-character edits (i.e. insertions,
        ///     deletions or substitutions) required to change one word into the other. It is named after Vladimir
        ///     Levenshtein.
        /// </summary>
        /// <remarks>Copied from https://www.csharpstar.com/csharp-string-distance-algorithm/.</remarks>
        /// <returns>The Levenshtein distance from <paramref name="s" /> to <paramref name="t" /></returns>
        private static int LevenshteinDistance(string s, string t)
        {
            int n = s.Length, m = t.Length;
            var d = new int[n + 1, m + 1];

            // Step 1
            if (n == 0)
            {
                return m;
            }

            if (m == 0)
            {
                return n;
            }

            // Step 2
            for (var i = 0; i <= n; d[i, 0] = i++)
            {
            }

            for (var j = 0; j <= m; d[0, j] = j++)
            {
            }

            // Step 3
            for (var i = 1; i <= n; i++)
            {
                // Step 4
                for (var j = 1; j <= m; j++)
                {
                    // Step 5
                    var cost = t[j - 1] == s[i - 1] ? 0 : 1;

                    // Step 6
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }

            // Step 7
            return d[n, m];
        }
    }
}
