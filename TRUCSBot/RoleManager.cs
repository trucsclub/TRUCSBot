using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace TRUCSBot
{
    public class RoleManager
    {
        public RoleManager()
        {
        }

        /// <summary>
        /// This method sends all of the reaction messages again, overwriting the ones stored in the settings.
        /// </summary>
        /// <param name="ctx">DSharpPlus context</param>
        public async Task SendMessagesAsync(CommandContext ctx)
        {
            foreach (var roleCategory in Application.Current.Settings.ReactionRoles)
            {
                await ctx.TriggerTypingAsync();
                var messageText = new StringBuilder("React with the following emojis to gain the role:\n");

                var reactions = new List<DiscordEmoji>();

                foreach (var role in roleCategory.Roles)
                {
                    messageText.AppendLine(role.ReactionEmoji + " = " + role.RoleDescription);
                    reactions.Add(role.Emoji);
                }

                var message = await ctx.RespondAsync(messageText.ToString());
                roleCategory.DiscordMessageId = message.Id;

                foreach (var reaction in reactions)
                {
                    await message.CreateReactionAsync(reaction);
                }
            }

            Application.Current.Settings.Save();
        }

        public async Task CheckReactionAddedAsync(MessageReactionAddEventArgs e)
        {
            if (e.User.Id == Application.Current.Discord.CurrentUser.Id)
            {
                // We don't want to respond to our own reactions.
                return;
            }

            var roleCategory = Application.Current.Settings.ReactionRoles.FirstOrDefault(x => x.DiscordMessageId == e.Message.Id);
            if (roleCategory == null)
            {
                return;
            }

            // It is a reaction message we sent; handle it.
            // Grab required role:
            var role = roleCategory.Roles.FirstOrDefault(x => x.Emoji == e.Emoji);
            if (role == null)
            {
                // TODO: log
                return;
            }

            await ((DiscordMember)e.User).GrantRoleAsync(e.Guild.Roles.Values.First(x => x.Name == role.RoleName));
        }

        public async Task CheckReactionRemovedAsync(MessageReactionRemoveEventArgs e)
        {
            if (e.User.Id == Application.Current.Discord.CurrentUser.Id)
            {
                // We don't want to respond to our own reactions.
                return;
            }

            var roleCategory = Application.Current.Settings.ReactionRoles.FirstOrDefault(x => x.DiscordMessageId == e.Message.Id);
            if (roleCategory == null)
            {
                return;
            }

            // It is a reaction message we sent; handle it.
            // Grab required role:
            var role = roleCategory.Roles.FirstOrDefault(x => x.Emoji == e.Emoji);
            if (role == null)
            {
                // TODO: log
                return;
            }

            await ((DiscordMember)e.User).RevokeRoleAsync(e.Guild.Roles.Values.First(x => x.Name == role.RoleName));
        }
    }
}
