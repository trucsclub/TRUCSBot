using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace TRUCSBot.Commands
{
    public class BotCommands
    {
        [Command("about"), Description("Find out info about the bot")]
        public async Task About(CommandContext ctx)
        {
            await ctx.RespondAsync(
                "CSClubBot was created by Tylor Pater for the official TRU CS Club Discord channel. \nIt's written in C# and targets .NET Core 2.0. It runs on Tylor's personal server.");
        }
    }
}
