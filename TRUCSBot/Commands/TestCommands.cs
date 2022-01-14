using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace TRUCSBot.Commands
{
    public class TestCommands : BaseCommandModule
    {
        [Command("getemojiid")]
        public async Task GetEmojiId(CommandContext ctx, DiscordEmoji emoji)
        {
            await ctx.RespondAsync(emoji.Name);
        }
    }
}
