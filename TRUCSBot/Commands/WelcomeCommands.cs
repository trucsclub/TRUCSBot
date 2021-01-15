using System.Linq;
using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace TRUCSBot.Commands
{
    public class WelcomeCommands : BaseCommandModule
    {
        [Hidden]
        [Description("Accept the terms of the channel and join the group")]
        [Command("accept")]
        public async Task Accept(CommandContext ctx)
        {
            if (ctx.Member.Roles.Any(x => x.Name == "Member"))
            {
                // already a member
                await ctx.Member.SendMessageAsync("You're already a member!");
                return;
            }

            // add group to the Members group
            await ctx.Member.GrantRoleAsync(ctx.Guild.Roles.First(x => x.Value.Name == "Member").Value); // note: this must be changed if we update the title
            await ctx.Message.DeleteAsync();
            await ctx.Guild.Channels.First(x => x.Value.Name == "general").Value
                .SendMessageAsync(Application.Current.Settings.WelcomeMessages.Random().Replace("{NICK}", ctx.Member.Mention));
        }
    }
}
