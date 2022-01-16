using System.Collections.Generic;
using System.ComponentModel;

using DSharpPlus.Entities;

using Newtonsoft.Json;

using TylorsTech.SimpleJsonSettings;

namespace TRUCSBot
{
    public class ApplicationSettings : StronglyTypedSettingsDefinition
    {
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

        public string IgdbClientId { get; set; }

        public string IgdbClientSecret { get; set; }

        public ulong GameSuggestionChannelId { get; set; }

        public List<RoleCategorySettingsItem> ReactionRoles { get; } = new();
    }

    public class RoleCategorySettingsItem
    {
        public string CategoryDescription { get; set; }
        public ulong? DiscordMessageId { get; set; }
        public List<RoleSettingsItem> Roles { get; set; }
    }

    public class RoleSettingsItem
    {
        [JsonIgnore]
        private DiscordEmoji _emoji;

        public string ReactionEmoji { get; set; }
        public string RoleName { get; set; }
        public string RoleDescription { get; set; }

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
