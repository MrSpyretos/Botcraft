using Botcraft.Handler;
using Discord;
using Discord.Commands;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Botcraft.Modules
{
    public class MemeModule : ModuleBase<SocketCommandContext>
    {

        [Command("meme", RunMode = RunMode.Async)]
        [Alias("reddit")]
        public async Task Meme(string subreddit = null)
        {

            var client = new HttpClient();
            var result = await client.GetStringAsync($"http://reddit.com/r/{subreddit ?? "memes"}/random.json?limit=1");
            if (!result.StartsWith("["))
            {
                await Context.Channel.SendMessageAsync("This subreddit doesn't exist!");
                return;
            }
            JArray arr = JArray.Parse(result);
            JObject post = JObject.Parse(arr[0]["data"]["children"][0]["data"].ToString());
            if (post["over_18"].ToString() == "True" && !(Context.Channel as ITextChannel).IsNsfw)
            {
                await ReplyAsync("The subreddit contains NSFW content , while this is a SFW channel.");
            }
            var builder = new EmbedBuilder()
                .WithImageUrl(post["url"].ToString())
                .WithColor(new Color(20, 40, 80))
                .WithTitle(post["title"].ToString())
                .WithUrl("http://reddit.com" + post["permalink"].ToString())
                .WithFooter($"{post["num_comments"]} ups {post["ups"]}");
            var embed = builder.Build();
            await Context.Channel.SendMessageAsync(null, false, embed);
        }
        [Command("ping", RunMode = RunMode.Async)]
        public async Task PingAsync()
        {
            await ReplyAsync("Pong!");
        }
        [Command("random", RunMode = RunMode.Async)]
        public async Task Random()
        {
            string[] responses = { "first", "second", "third" };
            await ReplyAsync(responses[new Random().Next(0, responses.Count())]);
        }

        [Command("echo", RunMode = RunMode.Async)]
        public async Task EchoAsync([Remainder] string text)
        {
            await ReplyAsync(text);
        }
        [Command("8ball", RunMode = RunMode.Async)]
        private async Task eightball(params string[] args)
        {
            string[] eightballquotes = { "As I see it, yes.", "Ask again later.", "Better not tell you now.", "Cannot predict now.", "Concentrate and ask again.", "Don’t count on it.", "Don’t count on it.", "It is certain.",
                "It is decidedly so.", "Most likely.", "My reply is no.", "My sources say no.", "Outlook not so good.", "Outlook good.", "Reply hazy, try again.", "Signs point to yes", "Very doubtful", "Without a doubt.",
            "Yes.", "Yes - definitely.", "You may rely on it"};
            Random rand = new Random();
            int index = rand.Next(eightballquotes.Length);
            if (args.Length == 0)
            {
                await ReplyAsync("You have to say something in order to recieve a prediction!");
            }
            else
            {
                await ReplyAsync($"{eightballquotes[index]}");
            }
        }
        [Command("slap", RunMode = RunMode.Async)]
        private async Task slap(params string[] args)
        {
            if (args.Length > 2 | args.Length == 2)
            {
                await ReplyAsync("Only mention one user!");
            }
            if (args.Length == 0)
            {
                await ReplyAsync($"<@{Context.User.Id}>... Slapped themself?");
            }
            if (args.Length == 1)
            {
                if (args[0].Contains("@everyone") | args[0].Contains("@here"))
                {
                    await ReplyAsync("Tsk Tsk");
                }
                else
                {
                    if (args[0].Contains("@"))
                    {
                        ulong CLIENTID = MentionUtils.ParseUser(args[0]);
                        string[] listofslaps = { $"<@{Context.User.Id}> just slapped {CommandHandler._client.GetUser(CLIENTID).Username}!", $"<@{Context.User.Id}> slaps {CommandHandler._client.GetUser(CLIENTID).Username} around with a large trout!" };
                        Random rand = new Random();
                        int index = rand.Next(listofslaps.Length);
                        await ReplyAsync($"{listofslaps[index]}");
                    }
                    else
                    {
                        await ReplyAsync("You need to mention someone to slap them!");
                    }
                }
            }
        }
        
    }
}
