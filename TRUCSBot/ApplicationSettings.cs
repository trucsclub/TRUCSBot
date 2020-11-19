using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;

using Newtonsoft.Json;

namespace TRUCSBot
{
    public class ApplicationSettings : TylorsTech.SimpleJsonSettings.StronglyTypedSettingsDefinition
    {
        public ApplicationSettings() { }

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
    }
}
