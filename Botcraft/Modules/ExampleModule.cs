using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Botcraft.Models;
using Botcraft.Utilities;
using Botcraft.Common;

namespace Botcraft.Modules
{
    public class ExampleModule : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger<ExampleModule> _logger;
        private readonly Images _images;

        public ExampleModule(ILogger<ExampleModule> logger , Images images )
        {
            _logger = logger;
            _images = images;
        }
        [Command("info")]
        public async Task Info(SocketGuildUser user = null)
        {
            if (user == null)
            {
                var builder = new EmbedBuilder()
                                .WithThumbnailUrl(Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl())
                                .WithDescription("In this message you can see stuff")
                                .WithColor(new Color(50, 80, 110))
                                .AddField("User ID : ", Context.User.Id, true)
                                .AddField("Discriminator", Context.User.Discriminator, true)
                                .AddField("Created at", Context.User.CreatedAt.ToString("dd/MM/yyyy"), true)
                                .AddField("Joined at ", (Context.User as SocketGuildUser).JoinedAt.Value.ToString("dd/MM/yyyy"), true)
                                .AddField("Roles", string.Join(" ", (Context.User as SocketGuildUser).Roles.Select(x => x.Mention)))
                                .WithCurrentTimestamp();
                var embed = builder.Build();
                await Context.Channel.SendMessageAsync(null, false, embed);
            }
            else
            {
                var builder = new EmbedBuilder()
                               .WithThumbnailUrl(Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl())
                               .WithDescription($"In this message you can see stuff about {user.Username}")
                               .WithColor(new Color(50, 80, 110))
                               .AddField("User ID : ", user.Id, true)
                               .AddField("Discriminator", user.Discriminator, true)
                               .AddField("Created at", user.CreatedAt.ToString("dd/MM/yyyy"), true)
                               .AddField("Joined at ", user.JoinedAt.Value.ToString("dd/MM/yyyy"), true)
                               .AddField("Roles", string.Join(" ", user.Roles.Select(x => x.Mention)))
                               .WithCurrentTimestamp();
                var embed = builder.Build();
                await Context.Channel.SendMessageAsync(null, false, embed);
            }
        }
        [Command("math")]
        public async Task MathAsync([Remainder] string math)
        {
            var dt = new DataTable();
            var result = dt.Compute(math, null);

            await Context.Channel.SendSuccessAsync("Success", $"The result was {result}");
        }
        
        [Command("image",RunMode = RunMode.Async)]
        public async Task Image(SocketGuildUser user)
        {
            var path = await _images.CreateImageAsync(user);
            await Context.Channel.SendFileAsync(path);
            File.Delete(path);
        }
    }
}