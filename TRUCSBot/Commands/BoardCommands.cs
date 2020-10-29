using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace TRUCSBot.Commands
{
    public class BoardCommands
    {
        [RequirePermissions(DSharpPlus.Permissions.ManageRoles)]
        [Command("addtoboard")]
        public async Task AddToBoard(CommandContext ctx, DiscordMember member)
        {
            try
            {
                await ctx.Guild.GrantRoleAsync(member, ctx.Guild.Roles.First(x => x.Name == "CS Club Board"));
                await ctx.RespondAsync($"Added {member.Mention} to the board.");
            }
            catch (Exception ex)
            {

            }
        }
    }
}
