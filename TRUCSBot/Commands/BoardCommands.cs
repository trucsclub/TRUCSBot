using System;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using Microsoft.Extensions.Logging;

namespace TRUCSBot.Commands
{
    public class BoardCommands : BaseCommandModule
    {
        private readonly ILogger _logger;

        public BoardCommands(ILogger<BoardCommands> logger)
        {
            _logger = logger;
        }

        [RequirePermissions(Permissions.ManageRoles)]
        [Command("addtoboard")]
        public async Task AddToBoard(CommandContext ctx, DiscordMember member)
        {
            try
            {
                await member.GrantRoleAsync(ctx.Guild.Roles.First(x => x.Value.Name == "CS Club Board").Value,
                    $"Role granted by {ctx.User.Mention}");
                await ctx.RespondAsync($"Added {member.Mention} to the board.");
            }
            catch (Exception ex)
            {
                await ctx.RespondAsync("Command failed.");
                _logger.LogError("Error executing AddToBoard", ex);
            }
        }
    }
}
