using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using Microsoft.Extensions.Logging;

namespace TRUCSBot.Commands
{
    [Description("Administrative Commands")]
    public class AdministrativeCommands : BaseCommandModule
    {
        private readonly ILogger _logger;

        public AdministrativeCommands(ILogger<AdministrativeCommands> logger)
        {
            _logger = logger;
        }

        [RequirePermissions(Permissions.ManageMessages)]
        [Command("trim"), Aliases("purge")]
        public async Task Trim(CommandContext ctx, int numOfMessages)
        {
            try
            {
                if (numOfMessages >= 100)
                {
                    await ctx.Channel.SendMessageAsync("Please use a number less than 100.");
                    return;
                }

                var messages = await ctx.Channel.GetMessagesAsync(numOfMessages + 1); //add one for the command message
                foreach (var f in messages)
                {
                    await ctx.Channel.DeleteMessageAsync(f);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while purging messages", ex);
            }
        }

        [Command("nick"), Description("Gives someone a new nickname."), RequirePermissions(Permissions.ManageNicknames)]
        public async Task ChangeNickname(CommandContext ctx, [Description("Member to change the nickname for.")] DiscordMember member, [RemainingText, Description("The nickname to give to that user.")] string newNick)
        {
            try
            {
                // let's change the nickname, and tell the audit logs who did it.
                await member.ModifyAsync(x => {
                        x.AuditLogReason = $"Changed by {ctx.User.Mention} ({ctx.User.Id}).";
                        x.Nickname = newNick;
                    });
                //tell the channel as well
                await ctx.RespondAsync("Changed nick.");
            }
            catch (Exception ex)
            {
                await ctx.RespondAsync("Unable to complete action: " + ex.Message);
            }
        }

        [RequirePermissions(Permissions.BanMembers)]
        [Command("warn"), Description("Warns someone about an infraction they've committed (like, a warning before a ban)")]
        public async Task Warn(CommandContext ctx, DiscordMember member, [RemainingText, Description("The reason they are being warned")] string reason)
        {
            try
            {
                Application.Current.AddWarning(member.Mention);

                if (Application.Current.CheckForBan(member.Mention))
                {
                    //BANHAMMER
                    await ctx.Guild.BanMemberAsync(member,
                        reason: "Given last warning by " + ctx.Member.Username + ": " + reason);

                    await member.SendMessageAsync(
                        "You have received too many warnings. Enjoy your ban.\n\nTo appeal this ban, contact a member of the TRU CS Club Board.");
                }
                else
                {
                    await ctx.RespondAsync("Issued warning.");
                }
            }
            catch (Exception ex)
            {
                await ctx.RespondAsync("Error: " + ex.Message);
            }
        }

        [Command("resetwarnings")]
        [RequirePermissions(Permissions.BanMembers)]
        [Description("Resets the number of warnings a user has been given.")]
        public async Task ResetWarnings(CommandContext ctx, [Description("Member to reset warnings for.")] DiscordMember member)
        {
            try
            {
                Application.Current.Warnings.Remove(member.Mention);
                Application.Current.SaveWarnings();

                await ctx.RespondAsync("Reset warnings for user.");
            }
            catch (Exception ex)
            {
                await ctx.RespondAsync("Error: " + ex.Message);
            }
        }

        [Command("ban"), Description("Bans a user, regardless of how many warnings they've had"), RequirePermissions(Permissions.BanMembers)]
        public async Task Ban(CommandContext ctx, DiscordMember member, [RemainingText, Description("Reason for their ban.")] string reason)
        {
            try
            {
                await ctx.Guild.BanMemberAsync(member, reason: reason);
            }
            catch (Exception ex)
            {
                await ctx.RespondAsync("Couldn't complete action: " + ex.Message);
                _logger.LogError("Couldn't complete Ban action", ex);
            }
        }

    }
}
