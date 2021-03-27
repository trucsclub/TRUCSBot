using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;

using DSharpPlus.Entities;

using Newtonsoft.Json;

namespace TRUCSBot
{
    public class ApplicationSettings : TylorsTech.SimpleJsonSettings.StronglyTypedSettingsDefinition
    {
        public ApplicationSettings()
        {
        }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string DebugToken { get; set; }

        [DefaultValue("INSERT TOKEN HERE")]
        public string Token { get; set; }

        [DefaultValue(false)]
        public bool RequireAccept { get; set; }

        [DefaultValue("!")]
        public string CommandPrefix { get; set; }

        public List<string> WelcomeMessages { get; set; }

        public List<string> GameStatusMessages { get; set; }

        [DefaultValue(15000)]
        public int ActivityMessageUpdateInterval { get; set; }

        public string IGDBClientId { get; set; }

        public string IGDBClientSecret { get; set; }

        public List<RoleCategorySettingsItem> ReactionRoles { get; set; } = new ();
    }

    public class RoleCategorySettingsItem
    {
        public string CategoryDescription { get; set; }
        public ulong? DiscordMessageId { get; set; }
        public List<RoleSettingsItem> Roles { get; set; }
    }

    public class RoleSettingsItem
    {
        public string ReactionEmoji { get; set; }
        public string RoleName { get; set; }
        public string RoleDescription { get; set; }

        [JsonIgnore]
        private DiscordEmoji _emoji = null;

        [JsonIgnore]
        public DiscordEmoji Emoji
        {
            get => _emoji ??= DiscordEmoji.FromName(Application.Current.Discord, ReactionEmoji);
            set
            {
                _emoji = value;
                ReactionEmoji = _emoji.Name;
            }
        }
    }
}
