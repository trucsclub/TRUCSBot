using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Newtonsoft.Json;

namespace TRUCSBot
{
    public class ApplicationSettings
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Include)]
        public string DebugToken { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Include)]
        public string Token { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Include)]
        public bool RequireAccept { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Include)]
        public bool EnableMessageScanner { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Include)]
        public List<string> WelcomeMessages { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Include)]
        public List<string> GameStatusMessages { get; set; }

        public void Save()
        {
            lock (this)
            {
                File.WriteAllText(Path.Combine(Application.Current.Directory, "settings.json"), JsonConvert.SerializeObject(this, Formatting.Indented));
            }
        }
    }
}
