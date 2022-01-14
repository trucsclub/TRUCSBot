using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using TRUCSBot.Commands;

using TylorsTech.SimpleJsonSettings;

namespace TRUCSBot
{
    /// <summary>
    ///     Main application class.
    /// </summary>
    public partial class Application
    {
        /// <summary>
        ///     List of Discord Activities (the "Playing" messages) that we have loaded from settings.
        /// </summary>
        private readonly List<DiscordActivity> _activityList = new();

        /// <summary>
        ///     Index of currently displayed Discord Activity
        /// </summary>
        private int _displayedActivity;

        /// <summary>
        ///     The current logger for this class
        /// </summary>
        private ILogger _logger;

        /// <summary>
        ///     The service provider used by Dependency Injection
        /// </summary>
        private IServiceProvider _serviceProvider;

        /// <summary>
        ///     Timer in charge of updating the current Discord Activity
        /// </summary>
        private Timer _statusTimer;

        /// <summary>
        ///     The main application Discord client interface
        /// </summary>
        public DiscordClient Discord { get; private set; }

        /// <summary>
        ///     Master list of all of the current Discord Commands
        /// </summary>
        public CommandsNextExtension DiscordCommands { get; private set; }

        /// <summary>
        ///     Current application settings
        /// </summary>
        public ApplicationSettings Settings { get; private set; }

        /// <summary>
        ///     Current list of all timers for announcements.
        /// </summary>
        // ReSharper disable once CollectionNeverQueried.Global
        public List<Timer> AnnouncementTimers { get; } = new();

        /// <summary>
        ///     Current role manager.
        /// </summary>
        public RoleManager RoleManager { get; private set; }

        /// <summary>
        ///     Configure services for Dependency Injection.
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

        /// <summary>
        ///     Main application startup method
        /// </summary>
        /// <param name="args">List of command line arguments</param>
        private async void OnStartup(string[] args)
        {
            _logger.LogInformation("Starting the TRUSU CS Club Discord bot...");
            _logger.LogInformation("Runtime directory: {Directory}", Environment.CurrentDirectory);

            CheckArgs(args);

            Settings = StronglyTypedSettingsFileBuilder<ApplicationSettings>
                .FromFile(Environment.CurrentDirectory, "settings.json")
                .WithDefaultNullValueHandling(NullValueHandling.Include)
                .WithDefaultValueHandling(DefaultValueHandling.Populate)
                .WithFileNotFoundBehavior(SettingsFileNotFoundBehavior.ReturnDefault)
                .WithEncoding(Encoding.UTF8)
                .Build();

            if (Settings.Token == "INSERT TOKEN HERE")
            {
                // And then quit. EDIT YOUR SHIT OWNER!
                _logger.LogError(
                    "You haven't set your token! You must edit settings.json and add your token before running the bot");
                Settings.Save();
                Current.Shutdown();
                return;
            }

            RoleManager = new RoleManager();

            var token = Debugger.IsAttached && !string.IsNullOrEmpty(Settings.DebugToken)
                ? Settings.DebugToken
                : Settings.Token;

            Discord = new DiscordClient(new DiscordConfiguration
            {
                AutoReconnect = true,
                Token = token,
                TokenType = TokenType.Bot
            });

            DiscordCommands = Discord.UseCommandsNext(new CommandsNextConfiguration
            {
                CaseSensitive = true, StringPrefixes = new[] { Settings.CommandPrefix }, EnableDms = true,
                Services = _serviceProvider
            });

            DiscordCommands.RegisterCommands<AdministrativeCommands>();
            DiscordCommands.RegisterCommands<AnnouncementCommands>();
            DiscordCommands.RegisterCommands<BoardCommands>();
            DiscordCommands.RegisterCommands<BotCommands>();
            DiscordCommands.RegisterCommands<GameNightSuggestionCommands>();
            DiscordCommands.RegisterCommands<InteractionCommands>();
            DiscordCommands.RegisterCommands<RoleCommands>();
            DiscordCommands.RegisterCommands<TestCommands>();
            DiscordCommands.RegisterCommands<WednesdayCommands>();

            if (Settings.RequireAccept)
            {
                DiscordCommands.RegisterCommands<WelcomeCommands>();
            }

            if (Settings.GameStatusMessages == null)
            {
                _logger.LogInformation("No game status messages were found in Settings");
                Settings.GameStatusMessages = new List<string> { $"Use {Settings.CommandPrefix}help for more info." };
                Settings.Save();
            }

            _activityList.Clear(); // just in case, for whatever reason it has items
            foreach (var message in Settings.GameStatusMessages)
            {
                _activityList.Add(new DiscordActivity(message)); // custom isn't supported on bots :(
            }

            _logger.LogInformation("{Count} status message(s) loaded", _activityList.Count);

            if (Settings.WelcomeMessages.Count <= 0)
            {
                Settings.WelcomeMessages.Add("Welcome {NICK}!");
                Settings.Save();
                _logger.LogInformation("No welcome messages found in settings; set default");
            }

            SetupDiscordEvents();
            _logger.LogInformation("Connecting to Discord");
            await Discord.ConnectAsync();
            _logger.LogInformation("Connected to Discord");

            _logger.LogInformation("Load complete; bot is now running");
        }

        private void CheckArgs(string[] args)
        {
            foreach (var arg in args)
            {
                switch (arg)
                {
                    case "--help":
                        Console.WriteLine("COMMANDS");
                        Console.WriteLine("--help\t\tShow this help");
                        Current.Shutdown();
                        return;

                    default:
                        _logger.LogError("Unknown command line switch: {Arg}", arg);
                        Current.Shutdown();
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

        private Task Discord_MessageReactionRemoved(DiscordClient sender, MessageReactionRemoveEventArgs e)
        {
            return RoleManager.CheckReactionRemovedAsync(e);
        }

        private Task Discord_MessageReactionAdded(DiscordClient sender, MessageReactionAddEventArgs e)
        {
            return RoleManager.CheckReactionAddedAsync(e);
        }

        private async Task Discord_ClientErrored(DiscordClient sender, ClientErrorEventArgs e)
        {
            _logger.LogError(e.Exception, "Discord client errored: {}", e.EventName);
            await Task.CompletedTask;
        }

        private async Task Discord_GuildMemberRemoved(DiscordClient sender, GuildMemberRemoveEventArgs e)
        {
            await e.Guild.Channels.First(x => x.Value.Name == "general").Value
                .SendMessageAsync(e.Member.Mention + " has left the building.");
            await Task.CompletedTask;
        }

        private async Task Discord_GuildMemberAdded(DiscordClient sender, GuildMemberAddEventArgs e)
        {
            if (!Settings.RequireAccept)
            {
                await e.Guild.Channels.First(x => x.Value.Name == "general").Value
                    .SendMessageAsync(Settings.WelcomeMessages.Random().Replace("{NICK}", e.Member.Mention));
            }
        }

        private async Task DiscordCommands_CommandErrored(CommandsNextExtension sender, CommandErrorEventArgs e)
        {
            _logger.LogError(e.Exception, "Error in Discord command '{}' from message '{}'", e.Command.Name,
                e.Context.Message.Content);
            await Task.CompletedTask;
        }

        /// <summary>
        ///     Called when Discord is initialized, connected, and ready for commands.
        /// </summary>
        private async Task Discord_Ready(DiscordClient sender, ReadyEventArgs e)
        {
            _logger.LogInformation("Discord is ready. Yay!");

            if (Settings.GameStatusMessages.Count > 1)
            {
                if (_statusTimer == null)
                {
                    _statusTimer = new Timer
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
                _logger.LogInformation("Only one activity message exists; setting it");
                await Discord.UpdateStatusAsync(_activityList[0]);
            }
            else
            {
                _logger.LogWarning("No activity messages exist; not setting one");
            }
        }

        private async Task Discord_Resumed(DiscordClient sender, ReadyEventArgs e)
        {
            if (Settings.GameStatusMessages.Count == 1)
            {
                _logger.LogInformation("Only one activity message exists; setting it");
                await Discord.UpdateStatusAsync(_activityList[0]);
            }
        }

        /// <summary>
        ///     Called when the <see cref="_statusTimer" /> event is fired.
        ///     This method should update the currently displayed DiscordActivity based on <see cref="_activityList" />
        /// </summary>
        private async void StatusTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _displayedActivity++;

            if (_displayedActivity >= _activityList.Count)
            {
                _displayedActivity = 0;
            }

            await Discord.UpdateStatusAsync(_activityList[_displayedActivity]);
        }

        private void OnShutdown()
        {
            Discord?.DisconnectAsync().Wait();
        }
    }
}
