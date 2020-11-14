using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
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
        private ILogger _logger;

        public GameNightSuggestionCommands(ILogger logger)
        {
            _logger = logger;
        }

        [Command("suggestgame")]
        public async Task AddToBoard(CommandContext ctx, [Description("The title of the game you want to suggest")] [RemainingText] string title)
        {
            var igdb = new IGDBClient("m6gfkurncg92ogg7a9gelvhvgfi2ji", "0eroahpwh9c6thv6lcl8efzfotbirt"); //TODO: move outside

            var embed = new DiscordEmbedBuilder()
            {
                Title = title,
                Color = DiscordColor.White
            };

            try
            {
                var games = await igdb.QueryAsync<Game>(IGDBClient.Endpoints.Games, $"search \"{title}\"; fields id,name,cover.*,involved_companies.company.name,platforms.name,summary,url;");
                var game = games.OrderBy(x => x.Name).FirstOrDefault();
                if (game != null)
                {
                    embed.Description = string.IsNullOrEmpty(game.Summary) ? "No information is available for this title" : game.Summary;
                    embed.Color = DiscordColor.Green;
                    embed.Url = game.Url;
                    var imgUrl = game.Cover?.Value.Url;
                    if (imgUrl.StartsWith("//"))
                        imgUrl = "https:" + imgUrl;
                    embed.ImageUrl = imgUrl;
                    if (game.InvolvedCompanies?.Values.Length > 0)
                    {
                        embed.AddField("Created by", string.Join(", ", game.InvolvedCompanies.Values.Select(x => x.Company.Value.Name)));
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
                var message = await ctx.Message.Channel.Guild.GetChannel(Debugger.IsAttached? 691903205545607201ul : 766406856050343996ul).SendMessageAsync(embed: embed);
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
    }
}
