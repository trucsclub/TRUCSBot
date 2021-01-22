using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace TRUCSBot.Commands
{
    public class InteractionCommands : BaseCommandModule
    {
        [Command("echo")]
        [Description("Makes the bot echo something back to you")]
        public async Task Echo(CommandContext ctx, [RemainingText] string content)
        {
            await ctx.RespondAsync(content);
        }

        [Command("sucks")]
        [Description("Responds with it's opinion about a programming language")]
        public async Task Sucks(CommandContext ctx, [RemainingText, Description("The language that you think sucks")] string language)
        {
            if (language.Equals("C#", StringComparison.InvariantCultureIgnoreCase) || language.Equals("CSharp", StringComparison.InvariantCultureIgnoreCase))
            {
                await ctx.RespondAsync("No, you suck.");
            }
            else
            {
                await ctx.RespondAsync("You know what doesn't suck? C#!");
            }
        }

        [Hidden]
        [Command("navyseal")]
        [Aliases("gorillawarfare")]
        [Description("Output the navy seals copypasta")]
        public async Task NavySeal(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await ctx.RespondAsync("What the fuck did you just fucking say about me, you little bitch? I’ll have you know I graduated top of my class in the Navy Seals, and I’ve been involved in numerous secret raids on Al-Quaeda, and I have over 300 confirmed kills. I am trained in gorilla warfare and I’m the top sniper in the entire US armed forces. You are nothing to me but just another target. I will wipe you the fuck out with precision the likes of which has never been seen before on this Earth, mark my fucking words. You think you can get away with saying that shit to me over the Internet? Think again, fucker. As we speak I am contacting my secret network of spies across the USA and your IP is being traced right now so you better prepare for the storm, maggot. The storm that wipes out the pathetic little thing you call your life. You’re fucking dead, kid. I can be anywhere, anytime, and I can kill you in over seven hundred ways, and that’s just with my bare hands. Not only am I extensively trained in unarmed combat, but I have access to the entire arsenal of the United States Marine Corps and I will use it to its full extent to wipe your miserable ass off the face of the continent, you little shit. If only you could have known what unholy retribution your little “clever” comment was about to bring down upon you, maybe you would have held your fucking tongue. But you couldn’t, you didn’t, and now you’re paying the price, you goddamn idiot. I will shit fury all over you and you will drown in it. You’re fucking dead, kiddo.");
        }

        [Command("people")]
        public async Task People(CommandContext ctx)
        {
            await ctx.RespondAsync($"There are {ctx.Guild.MemberCount} people on this server.");
        }
    }
}
