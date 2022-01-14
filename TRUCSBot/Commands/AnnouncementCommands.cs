using System;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace TRUCSBot.Commands
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class AnnouncementCommands : BaseCommandModule
    {
        [Command("queue")]
        [Description("Queue an announcement")]
        [RequirePermissions(Permissions.MentionEveryone)]
        public async Task QueueAnnouncement(CommandContext ctx,
            [Description("Date to send")] string date,
            [Description("The time to send at")] string time,
            [RemainingText] [Description("The actual message")] string message)
        {
            var del = ctx.Message.DeleteAsync();

            var res = DateTime.TryParse(date, out var d);
            if (!res)
            {
                d = new DateTime();
            }

            d = date?.ToLower() switch
            {
                "today" => DateTime.Now,
                "tomorrow" => DateTime.Now.AddDays(1),
                _ => d
            };

            res = TimeSpan.TryParse(time, out var t);
            if (!res)
            {
                t = new TimeSpan(8, 0, 0);
            }

            t = time?.ToLower() switch
            {
                "now" => DateTime.Now.TimeOfDay,
                _ => t
            };

            var actualDate = new DateTime(d.Year, d.Month, d.Day, t.Hours, t.Minutes, t.Seconds);

            var pm = ctx.Member.SendMessageAsync("Queued an announcement at " + actualDate.ToLongDateString() + " " +
                                                 actualDate.ToLongTimeString() + " with the content: \n\n" + message);

            if (actualDate <= DateTime.Now.AddSeconds(5))
            {
                // post now
                await ctx.Guild.Channels.First(x => x.Value.Name == "announcements").Value.SendMessageAsync(message);
            }
            else
            {
                var timer = new Timer();
                timer.Interval = (actualDate - DateTime.Now).TotalMilliseconds;
                Application.Current.AnnouncementTimers.Add(timer);
                timer.Elapsed += async (s, _) =>
                {
                    await ctx.Guild.Channels.First(x => x.Value.Name == "announcements").Value
                        .SendMessageAsync(message);
                    Application.Current.AnnouncementTimers.Remove((Timer)s);
                    ((Timer)s).Stop();
                };
                timer.Start();
            }

            await Task.WhenAll(pm, del);
        }
    }
}
