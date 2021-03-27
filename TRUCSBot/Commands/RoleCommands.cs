using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;

namespace TRUCSBot.Commands
{
    [Description("Role Commands")]
    public class RoleCommands : BaseCommandModule
    {
        private readonly ILogger _logger;

        public RoleCommands(ILogger<RoleCommands> logger)
        {
            _logger = logger;
        }

        [RequirePermissions(Permissions.ManageRoles)]
        [Command("resendreactionmessage")]
        public async Task ResendReactionMessage(CommandContext ctx)
        {
            await Application.Current.RoleManager.SendMessagesAsync(ctx);
        }

    }
}
