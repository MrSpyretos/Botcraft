using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Botcraft.Modules
{
    public class InteractiveModule : InteractiveBase
    {
        private readonly CommandService _service;
        public InteractiveModule(CommandService service)
        {
            _service = service;
        }
        // DeleteAfterAsync will send a message and asynchronously delete it after the timeout has popped
        // This method will not block.
        [Command("delete")]
        public async Task<RuntimeResult> Test_DeleteAfterAsync()
        {
            await ReplyAndDeleteAsync("this message will delete in 10 seconds", timeout: new TimeSpan(0, 0, 10));
            //await ReplyAndDeleteAsync("this message will delete in 10 seconds", timeout: TimeSpan.FromSeconds(10));
            return Ok();
        }

        // NextMessageAsync will wait for the next message to come in over the gateway, given certain criteria
        // By default, this will be limited to messages from the source user in the source channel
        // This method will block the gateway, so it should be ran in async mode.
        [Command("next", RunMode = RunMode.Async)]
        public async Task Test_NextMessageAsync()
        {
            await ReplyAsync("What is 2+2?");
            //var response = await NextMessageAsync();
            var response = await NextMessageAsync(true, true, new TimeSpan(0, 0, 10));
            if (response != null)
            {
                if (response.Content == "4")
                    await ReplyAsync("Correct , the answer is 4 !");
                else
                    await ReplyAsync("Wrong! The answer was 4 !");
            }
            else
                await ReplyAsync("You did not reply before the timeout");
        }

        // PagedReplyAsync will send a paginated message to the channel
        // You can customize the paginator by creating a PaginatedMessage object
        // You can customize the criteria for the paginator as well, which defaults to restricting to the source user
        // This method will not block.
        [Command("paginator")]
        [Summary("This will create a paginator.")]
        public async Task Test_Paginator()
        {
            //List<string> Pages = new List<string>();
            //Pages.Add("**Help command**\n `!help` - This is the help command");
            var pages = new[] { "**Help**\n\n !help - Show the help command",
                "**Help**\n\n !prefix - View or change the prefix",
                "**Help**\n\n`!ping` - Get the current latency"};
            PaginatedMessage paginatedMessage = new PaginatedMessage()
            {
                Pages = pages,
                Options = new PaginatedAppearanceOptions()
                {
                    InformationText = "Thsi is a test",
                    Info = new Emoji("👍")
                },
                Color = new Discord.Color(33, 176, 252),
                Title = "A nice paginator"
            };
            await PagedReplyAsync(paginatedMessage);
        }
        [Command("help")]
        public async Task Help()
        {
            List<string> Pages = new List<string>();
            foreach (var module in _service.Modules)
            {
                string page = $"**{module.Name}**\n\n";
                foreach (var command in module.Commands)
                {
                    page += $"`!{command.Name}` - {command.Summary ?? "No description provided."}\n";
                }
                Pages.Add(page);
            }
            await PagedReplyAsync(Pages);
        }
    }
}
