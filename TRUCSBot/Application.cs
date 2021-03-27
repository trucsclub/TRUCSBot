using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using TylorsTech.SimpleJsonSettings;

namespace TRUCSBot
{
    public partial class Application
    {
        private readonly List<DiscordActivity> _activityList = new List<DiscordActivity>();

        private System.Timers.Timer _statusTimer;
        private int _displayedActivity = 0;

        private IServiceProvider _serviceProvider;
        private ILogger _logger;

        public DiscordClient Discord { get; private set; }
        public CommandsNextExtension DiscordCommands { get; private set; }
        public ApplicationSettings Settings { get; private set; }
        public List<System.Timers.Timer> AnnouncementTimers { get; } = new List<System.Timers.Timer>();
        public RoleManager RoleManager { get; private set; }

        /// <summary>
        /// Configure services for Dependency Injection.
        /// </summary>
        internal void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddLogging(builder =>
                {
                     builder.AddConsole();
                     builder.AddSystemdConsole();
                });

            _serviceProvider = serviceCollection.BuildServiceProvider();
            _logger = _serviceProvider.GetRequiredService<ILogger<Program>>();
        }

        public async void OnStartup(string[] args)
        {
            _logger.LogInformation("Starting the TRUSU CS Club Discord bot...");
            _logger.LogInformation($"Runtime directory: {Environment.CurrentDirectory}");

            CheckArgs(args);

            Settings = StronglyTypedSettingsFileBuilder<ApplicationSettings>.FromFile(Environment.CurrentDirectory, "settings.json")
                .WithDefaultNullValueHandling(NullValueHandling.Include)
                .WithDefaultValueHandling(DefaultValueHandling.Populate)
                .WithFileNotFoundBehavior(SettingsFileNotFoundBehavior.ReturnDefault)
                .WithEncoding(System.Text.Encoding.UTF8)
                .Build();

            if (Settings.Token == "INSERT TOKEN HERE")
            {
                // And then quit. EDIT YOUR SHIT OWNER!
                _logger.LogError("You haven't set your token! You must edit settings.json and add your token before running the bot.");
                Settings.Save();
                Application.Current.Shutdown();
                return;
            }

            RoleManager = new RoleManager();

            var token = Debugger.IsAttached && !string.IsNullOrEmpty(Settings.DebugToken) ? Settings.DebugToken : Settings.Token;

            Discord = new DiscordClient(new DiscordConfiguration()
            {
                AutoReconnect = true,
                Token = token,
                TokenType = TokenType.Bot
            });

            DiscordCommands = Discord.UseCommandsNext(new CommandsNextConfiguration() { CaseSensitive = true, StringPrefixes = new[] { Settings.CommandPrefix }, EnableDms = true, Services = _serviceProvider });

            DiscordCommands.RegisterCommands<Commands.AdministrativeCommands>();
            DiscordCommands.RegisterCommands<Commands.AnnouncementCommands>();
            DiscordCommands.RegisterCommands<Commands.BoardCommands>();
            DiscordCommands.RegisterCommands<Commands.BotCommands>();
            DiscordCommands.RegisterCommands<Commands.GameNightSuggestionCommands>();
            DiscordCommands.RegisterCommands<Commands.InteractionCommands>();
            DiscordCommands.RegisterCommands<Commands.RoleCommands>();
            DiscordCommands.RegisterCommands<Commands.TestCommands>();

            if (Settings.RequireAccept)
            {
                DiscordCommands.RegisterCommands<Commands.WelcomeCommands>();
            }

            if (Settings.GameStatusMessages == null)
            {
                _logger.LogInformation("No game status messages were found in Settings.");
                Settings.GameStatusMessages = new List<string> { $"Use {Settings.CommandPrefix}help for more info." };
                Settings.Save();
            }

            _activityList.Clear(); // just in case, for whatever reason it has items
            foreach (var message in Settings.GameStatusMessages)
            {
                _activityList.Add(new DiscordActivity(message)); // custom isn't supported on bots :(
            }

            _logger.LogInformation(_activityList.Count + " status message(s) loaded.");

            if (Settings.WelcomeMessages.Count <= 0)
            {
                Settings.WelcomeMessages.Add("Welcome {NICK}!");
                Settings.Save();
                _logger.LogInformation("No welcome messages found in settings; set default.");
            }

            SetupDiscordEvents();
            _logger.LogInformation("Connecting to Discord...");
            await Discord.ConnectAsync();
            _logger.LogInformation("Connected to Discord.");

            _logger.LogInformation("Load complete. Bot is now running.");
        }

        private void CheckArgs(string[] args)
        {
            foreach (var arg in args)
            {
                switch (arg)
                {
                    case "--help":
                        Console.WriteLine("COMMANDS");
                        Console.WriteLine("--dontscan, -d\t\tDont' scan for flagged words");
                        Console.WriteLine("--help\t\tShow this help");
                        Application.Current.Shutdown();
                        return;

                    default:
                        _logger.LogError("Unknown command line switch: " + arg);
                        Application.Current.Shutdown();
                        return;
                }
            }
        }

        private void SetupDiscordEvents()
        {
            Discord.Ready += Discord_Ready;
            DiscordCommands.CommandErrored += DiscordCommands_CommandErrored;
            Discord.GuildMemberAdded += Discord_GuildMemberAdded;
            Discord.GuildMemberRemoved += Discord_GuildMemberRemoved;
            Discord.ClientErrored += Discord_ClientErrored;
            Discord.Resumed += Discord_Resumed;
            Discord.MessageReactionAdded += Discord_MessageReactionAdded;
            Discord.MessageReactionRemoved += Discord_MessageReactionRemoved;
        }

        private Task Discord_MessageReactionRemoved(DiscordClient sender, DSharpPlus.EventArgs.MessageReactionRemoveEventArgs e)
        {
            return RoleManager.CheckReactionRemovedAsync(e);
        }

        private Task Discord_MessageReactionAdded(DiscordClient sender, DSharpPlus.EventArgs.MessageReactionAddEventArgs e)
        {
            return RoleManager.CheckReactionAddedAsync(e);
        }

        private async Task Discord_ClientErrored(DiscordClient sender, DSharpPlus.EventArgs.ClientErrorEventArgs e)
        {
            _logger.LogError("Discord client errored", e);
            await Task.CompletedTask;
        }

        private async Task Discord_GuildMemberRemoved(DiscordClient sender, DSharpPlus.EventArgs.GuildMemberRemoveEventArgs e)
        {
            await e.Guild.Channels.First(x => x.Value.Name == "general").Value.SendMessageAsync(e.Member.Mention + " has left the building.");
            await Task.CompletedTask;
        }

        private async Task Discord_GuildMemberAdded(DiscordClient sender, DSharpPlus.EventArgs.GuildMemberAddEventArgs e)
        {
            if (!Settings.RequireAccept)
            {
                await e.Guild.Channels.First(x => x.Value.Name == "general").Value
                    .SendMessageAsync(Settings.WelcomeMessages.Random().Replace("{NICK}", e.Member.Mention));
            }
        }

        private async Task DiscordCommands_CommandErrored(CommandsNextExtension sender, CommandErrorEventArgs e)
        {
            _logger.LogError($"Error in Discord command", e);
            await Task.CompletedTask;
        }

        private async Task Discord_Ready(DiscordClient sender, DSharpPlus.EventArgs.ReadyEventArgs e)
        {
            _logger.LogInformation("Discord is ready. Yay!");

            if (Settings.GameStatusMessages.Count > 1)
            {
                if (_statusTimer == null)
                {
                    _statusTimer = new System.Timers.Timer()
                    {
                        Interval = Settings.ActivityMessageUpdateInterval,
                        AutoReset = true
                    };

                    _statusTimer.Elapsed += StatusTimer_Elapsed;
                }

                _statusTimer.Start();
            }
            else if (Settings.GameStatusMessages.Count == 1)
            {
                _logger.LogInformation("Only one activity message exists; setting it.");
                await Discord.UpdateStatusAsync(_activityList[0]);
            }
            else
            {
                _logger.LogWarning("No activity messages exist; not setting one.");
            }
        }

        private async Task Discord_Resumed(DiscordClient sender, DSharpPlus.EventArgs.ReadyEventArgs e)
        {
            if (Settings.GameStatusMessages.Count == 1)
            {
                _logger.LogInformation("Only one activity message exists; setting it.");
                await Discord.UpdateStatusAsync(_activityList[0]);
            }
        }

        private async void StatusTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            _displayedActivity++;

            if (_displayedActivity >= _activityList.Count)
            {
                _displayedActivity = 0;
            }

            await Discord.UpdateStatusAsync(_activityList[_displayedActivity]);
        }

        public void OnShutdown()
        {
            Discord?.DisconnectAsync().Wait();
        }
    }
}
