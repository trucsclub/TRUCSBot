using System;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using Microsoft.Extensions.Logging;

namespace TRUCSBot.Commands
{
    /// <summary>
    ///     This class handles administrative commands, that should require admin roles.
    /// </summary>
    [Description("Administrative Commands")]
    public class AdministrativeCommands : BaseCommandModule
    {
        private readonly ILogger _logger;

        public AdministrativeCommands(ILogger<AdministrativeCommands> logger)
        {
            _logger = logger;
        }

        [RequirePermissions(Permissions.ManageMessages)]
        [Command("trim")]
        [Aliases("purge")]
        public async Task Trim(CommandContext ctx, int numOfMessages)
        {
            try
            {
                if (numOfMessages >= 100)
                {
                    await ctx.Channel.SendMessageAsync("Please use a number less than 100.");
                    return;
                }

                var messages = await ctx.Channel.GetMessagesAsync(numOfMessages + 1); // add one for the command message
                foreach (var f in messages)
                {
                    await ctx.Channel.DeleteMessageAsync(f);
                }
            }
            catch (Exception)
            {
                _logger.LogError("Error while purging messages");
            }
        }

        [Command("nick")]
        [Description("Gives someone a new nickname.")]
        [RequirePermissions(Permissions.ManageNicknames)]
        public async Task ChangeNickname(CommandContext ctx,
            [Description("Member to change the nickname for.")] DiscordMember member,
            [RemainingText] [Description("The nickname to give to that user.")] string newNick)
        {
            try
            {
                // let's change the nickname, and tell the audit logs who did it.
                await member.ModifyAsync(x =>
                {
                    x.AuditLogReason = $"Changed by {ctx.User.Mention} ({ctx.User.Id}).";
                    x.Nickname = newNick;
                });
                // tell the channel as well
                await ctx.RespondAsync("Changed nick.");
            }
            catch (Exception ex)
            {
                await ctx.RespondAsync("Unable to complete action: " + ex.Message);
            }
        }

        [Command("ban")]
        [Description("Bans a user, regardless of how many warnings they've had")]
        [RequirePermissions(Permissions.BanMembers)]
        public async Task Ban(CommandContext ctx, DiscordMember member,
            [RemainingText] [Description("Reason for their ban.")] string reason)
        {
            try
            {
                await ctx.Guild.BanMemberAsync(member, reason: reason);
            }
            catch (Exception ex)
            {
                await ctx.RespondAsync("Couldn't complete action: " + ex.Message);
                _logger.LogError("Couldn't complete Ban action");
            }
        }
    }
}
