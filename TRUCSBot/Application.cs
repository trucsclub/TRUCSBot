using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;

using Newtonsoft.Json;

namespace TRUCSBot
{
    public partial class Application
    {
        public DiscordClient Discord { get; private set; }
        public CommandsNextModule DiscordCommands { get; private set; }
        public MessageScanner MessageScanner { get; private set; }
        public ApplicationSettings Settings { get; private set; }
        public List<System.Timers.Timer> AnnouncementTimers { get; } = new List<System.Timers.Timer>();

        private System.Timers.Timer statusTimer;
        private System.Timers.Timer muteTimer;
        private List<DiscordGame> gameList;
        private int displayed_game = 0;

        public Dictionary<string, int> Warnings { get; private set; }
        public Dictionary<ulong, Tuple<ulong, DateTime>> Mutes { get; set; }

        public async void OnStartup(string[] args)
        {
            new ArgumentException(); //to fix a bug

            Console.WriteLine("Starting Project Cerulean...");

            CheckArgs(args);

            if (CreateLoadSettings() == -1)
            {
                Application.Current.Shutdown();
                return;
            }

            if (Settings.Token == "INSERT TOKEN HERE")
            {
                Console.WriteLine(
                    "You haven't set your token! You must edit settings.json and add your token before running the bot.");
                Console.WriteLine("Press Ctrl-C to continue...");
                return;
            }

            if (Debugger.IsAttached && !string.IsNullOrEmpty(Settings.DebugToken))
            {
                Discord = new DiscordClient(new DiscordConfiguration()
                {
                    AutoReconnect = true,
                    Token = Settings.DebugToken,
                    TokenType = TokenType.Bot
                });
                Console.WriteLine("IN DEBUG MODE");
            }
            else
                Discord = new DiscordClient(new DiscordConfiguration()
                {
                    AutoReconnect = true,
                    Token = Settings.Token,
                    TokenType = TokenType.Bot
                });

            DiscordCommands = Discord.UseCommandsNext(new CommandsNextConfiguration() { CaseSensitive = true, StringPrefix = "!", EnableDms = true });
            DiscordCommands.RegisterCommands<Commands.AdministrativeCommands>();
            DiscordCommands.RegisterCommands<Commands.AnnouncementCommands>();
            DiscordCommands.RegisterCommands<Commands.BoardCommands>();
            DiscordCommands.RegisterCommands<Commands.BotCommands>();
            DiscordCommands.RegisterCommands<Commands.GameNightSuggestionCommands>();
            DiscordCommands.RegisterCommands<Commands.InteractionCommands>();

            if (Settings.EnableMessageScanner)
                DiscordCommands.RegisterCommands<Commands.MessageScannerCommands>();

            if (Settings.RequireAccept)
                DiscordCommands.RegisterCommands<Commands.WelcomeCommands>();

            Console.WriteLine("Loading warnings...");
            LoadWarnings();

            Console.WriteLine("Loading mutes...");
            LoadMutes();

            if (Settings.EnableMessageScanner)
                MessageScanner = new MessageScanner(Path.Combine(Application.Current.Directory, "flaggedwords.json"));

            if (Settings.GameStatusMessages == null)
            {
                Settings.GameStatusMessages = new List<string> { "use !help for more info." };
                Settings.Save();
            }

            SetupDiscordEvents();
            Console.WriteLine("Connecting to Discord...");
            await Discord.ConnectAsync();
            Console.WriteLine("Connected to Discord.");

            Console.WriteLine("Load complete. Bot is now running.");
        }

        public void StartMuteTimer()
        {
            if (muteTimer == null || muteTimer.Enabled == false)
            {
                muteTimer = new System.Timers.Timer();
                muteTimer.Interval = 500;
                muteTimer.Elapsed += MuteTimer_Elapsed;
                muteTimer.Start();
            }
        }

        private async void MuteTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            for (int i = 0; i < Mutes.Count; i++)
            {
                var f = Mutes.ElementAt(i);
                if (f.Value.Item2 < DateTime.Now)
                {
                    Mutes.Remove(f.Key);
                    var guild = Discord.Guilds[f.Value.Item1];
                    await guild.RevokeRoleAsync(await guild.GetMemberAsync(f.Key), guild.Roles.First(x => x.Name == "Muted"), "Mute finished.");
                }
            }
        }

        /// <summary>
        /// Attempts to load a settings file, and if it succeeds returns 0. If it fails, returns -1.
        /// </summary>
        /// <returns></returns>
        private int CreateLoadSettings()
        {
            if (File.Exists(Path.Combine(Application.Current.Directory, "settings.json")))
            {
                Settings = JsonConvert.DeserializeObject<ApplicationSettings>(File.ReadAllText(Path.Combine(Application.Current.Directory, "settings.json")));
                return 0;
            }
            //Generate new one!
            Settings = GenerateNewSettingsFile();

            //save
            Settings.Save();
            //And then quit. EDIT YOUR SHIT OWNER!
            Console.WriteLine("No settings file found. One has been generated; edit it before you run the bot again!");
            Console.WriteLine("Press Ctrl-C to continue...");
            Console.ReadLine();
            return -1;
        }

        private ApplicationSettings GenerateNewSettingsFile()
        {
            var m = new ApplicationSettings()
            {
                Token = "INSERT TOKEN HERE"
            };
            return m;
        }

        private void CheckArgs(string[] args)
        {
            foreach (var arg in args)
            {
                switch (arg)
                {
                    case "--dontscan":
                    case "-d":
                        Console.WriteLine("Word scanner disabled.");
                        Settings.EnableMessageScanner = false;
                        break;

                    case "--help":
                        Console.WriteLine("COMMANDS");
                        Console.WriteLine("--dontscan, -d\t\tDont' scan for flagged words");
                        Console.WriteLine("--help\t\tShow this help");
                        Application.Current.Shutdown();
                        return;

                    default:
                        Console.WriteLine("Unknown command line switch: " + arg, LogLevel.Error);
                        Application.Current.Shutdown();
                        return;
                }
            }
        }
        private void LoadWarnings()
        {
            var filename = Path.Combine(Application.Current.Directory, "warnings.json");
            if (File.Exists(filename))
            {
                Warnings = JsonConvert.DeserializeObject<Dictionary<string, int>>(File.ReadAllText(filename));
            }
            else
            {
                Warnings = new Dictionary<string, int>();
            }
        }

        public void SaveWarnings()
        {
            var filename = Path.Combine(Application.Current.Directory, "warnings.json");
            File.WriteAllText(filename, JsonConvert.SerializeObject(Warnings, Formatting.Indented));
        }

        private void LoadMutes()
        {
            var filename = Path.Combine(Application.Current.Directory, "mutes.json");
            if (File.Exists(filename))
            {
                Mutes = JsonConvert.DeserializeObject<Dictionary<ulong, Tuple<ulong, DateTime>>>(File.ReadAllText(filename));
            }
            else
            {
                Mutes = new Dictionary<ulong, Tuple<ulong, DateTime>>();
            }
        }

        private void SaveMutes()
        {
            var filename = Path.Combine(Application.Current.Directory, "mutes.json");
            File.WriteAllText(filename, JsonConvert.SerializeObject(Mutes, Formatting.Indented));
        }

        private void SetupDiscordEvents()
        {
            Discord.Ready += Discord_Ready;
            Discord.MessageCreated += Discord_MessageCreated;
            Discord.MessageUpdated += Discord_MessageUpdated;
            DiscordCommands.CommandErrored += DiscordCommands_CommandErrored;
            Discord.GuildMemberAdded += Discord_GuildMemberAdded;
            Discord.GuildMemberRemoved += Discord_GuildMemberRemoved;
        }

        private async Task Discord_GuildMemberRemoved(DSharpPlus.EventArgs.GuildMemberRemoveEventArgs e)
        {
            await e.Guild.Channels.First(x => x.Name == "general").SendMessageAsync(e.Member.Mention + " has left the building.");
            await Task.CompletedTask;
        }

        private async Task Discord_GuildMemberAdded(DSharpPlus.EventArgs.GuildMemberAddEventArgs e)
        {
            if (!Settings.RequireAccept)
            {
                await e.Guild.Channels.First(x => x.Name == "general")
                    .SendMessageAsync(Settings.WelcomeMessages.Random().Replace("{NICK}", e.Member.Mention));
            }
        }

        private async Task Discord_MessageUpdated(DSharpPlus.EventArgs.MessageUpdateEventArgs e)
        {
            await ScanMessageForLanguage(e.Message, e.Guild, e.Author);
        }

        private async Task DiscordCommands_CommandErrored(CommandErrorEventArgs e)
        {
            Console.WriteLine($"Error in command: {e.Exception.Message}\n{e.Exception.StackTrace}", LogLevel.Error);
            await Task.CompletedTask;
        }

        private async Task Discord_MessageCreated(DSharpPlus.EventArgs.MessageCreateEventArgs e)
        {
            await ScanMessageForLanguage(e.Message, e.Guild, e.Author);
        }

        private async Task Discord_Ready(DSharpPlus.EventArgs.ReadyEventArgs e)
        {
            Console.WriteLine("Discord is ready. Yay!");
            //await Discord.UpdateStatusAsync(new DiscordGame(Settings.GameStatusMessages[0]));

            if (Settings.GameStatusMessages.Count > 1)
            {
                gameList = new List<DiscordGame>();
                foreach (var f in Settings.GameStatusMessages)
                {
                    gameList.Add(new DiscordGame(f));
                }
                Console.WriteLine(gameList.Count + " status messages loaded.");

                statusTimer = new System.Timers.Timer()
                {
                    Interval = 15000
                };
                statusTimer.Elapsed += StatusTimer_Elapsed;
                statusTimer.Start();
            }

        }

        private async void StatusTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            displayed_game++;
            if (displayed_game >= gameList.Count)
            {
                displayed_game = 0;
            }
            await Discord.UpdateStatusAsync(gameList[displayed_game]);
        }

        public void AddWarning(string usersMention)
        {
            if (Warnings.ContainsKey(usersMention))
                Warnings[usersMention]++;
            else
                Warnings.Add(usersMention, 1);
            SaveWarnings();
        }

        public void AddMute(ulong userID, int timeInMinutes, DiscordGuild guild)
        {
            if (!Mutes.ContainsKey(userID))
                Mutes.Add(userID, new Tuple<ulong, DateTime>(guild.Id, DateTime.Now.AddMinutes(timeInMinutes)));
            SaveMutes();
            StartMuteTimer();
        }

        public bool CheckForBan(string usersMention)
        {
            return Warnings[usersMention] >= 3;
        }

        private async Task ScanMessageForLanguage(DiscordMessage message, DiscordGuild guild, DiscordUser author)
        {
            if (!Settings.EnableMessageScanner || message.Author.IsBot) return;

            if (MessageScanner.ScanMessage(message.Content))
            {
                AddWarning(author.Mention);

                if (CheckForBan(author.Mention))
                {
                    //BANHAMMER
                    await guild.BanMemberAsync((DiscordMember)author,
                        reason: $"Innapropriate language in message: {message.Content}");

                    await (await Discord.CreateDmAsync(author)).SendMessageAsync(
                        "Your language has been deemed unacceptable and you have been warned multiple times. Enjoy your ban.\n\nTo appeal this ban, contact a moderator.");
                }
                else
                {
                    await message.RespondAsync($"{author.Mention}, your message has been removed for containing language deemed unacceptable in this channel. This is warning {Warnings[author.Mention]}. You only get three.");
                }
                await message.Channel.DeleteMessageAsync(message);
            }
        }

        public void OnShutdown()
        {
            MessageScanner?.Save();
            Discord?.DisconnectAsync().Wait();
        }
    }
}
