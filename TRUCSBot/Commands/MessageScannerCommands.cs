using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace TRUCSBot.Commands
{
    public class MessageScannerCommands : BaseCommandModule
    {
        [Command("addflaggedword"), Aliases("flagword")]
        public async Task AddFlaggedWord(CommandContext ctx, [RemainingText, Description("The word(s) to block")] string word)
        {
            Application.Current.MessageScanner.AddFlaggedWord(word);
            await ctx.Channel.SendMessageAsync("Added flagged word: " + word);
        }
    }
}
