using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace TRUCSBot.Commands
{
    public class AnnouncementCommands
    {
        [Command("queue"), Description("Queue an announcement"), RequirePermissions(DSharpPlus.Permissions.MentionEveryone)]
        public async Task QueueAnnouncement(CommandContext ctx,
            [Description("Date to send")] string date,
            [Description("The time to send at")] string time,
            [RemainingText, Description("The actual message")] string message)
        {
            var del = ctx.Message.DeleteAsync();

            var res = DateTime.TryParse(date, out DateTime d);
            if (!res)
                d = new DateTime();

            if (date.ToLower() == "today")
                d = DateTime.Now;
            if (date.ToLower() == "tomorrow")
                d = DateTime.Now.AddDays(1);

            res = TimeSpan.TryParse(time, out TimeSpan t);
            if (!res)
                t = new TimeSpan(8, 0, 0);

            if (time.ToLower() == "now")
                t = DateTime.Now.TimeOfDay;

            var actualDate = new DateTime(d.Year, d.Month, d.Day, t.Hours, t.Minutes, t.Seconds);

            var pm = ctx.Member.SendMessageAsync("Queued an announcement at " + actualDate.ToLongDateString() + " " + actualDate.ToLongTimeString() + " with the content: \n\n" + message);

            if (actualDate <= DateTime.Now.AddSeconds(5))
            {
                //post now
                await ctx.Guild.Channels.First(x => x.Name == "announcements").SendMessageAsync(message);
            }
            else
            {
                var timer = new Timer();
                timer.Interval = (actualDate - DateTime.Now).TotalMilliseconds;
                Application.Current.AnnouncementTimers.Add(timer);
                timer.Elapsed += async (s, e) =>
                {
                    await ctx.Guild.Channels.First(x => x.Name == "announcements").SendMessageAsync(message);
                    Application.Current.AnnouncementTimers.Remove((Timer)s);
                    ((Timer)s).Stop();
                };
                timer.Start();
            }

            await Task.WhenAll(pm, del);
        }
    }
}
