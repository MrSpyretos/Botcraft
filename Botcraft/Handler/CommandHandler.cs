using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Botcraft.Models;
using Victoria;
using Botcraft.Services;
using Botcraft.Utilities;
using Botcraft.Database.Entities;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Botcraft.Common;

namespace Botcraft.Handler
{
    public class CommandHandler : InitializedService
    {
        private readonly ILogger _logger;
        private readonly IServiceProvider _provider;
        public static DiscordSocketClient _client;
        private readonly CommandService _service;
        private readonly IConfiguration _config;
        private readonly ServerServices _serverServices;
        private readonly AutoRoleServices _autoRoleServices;
        private readonly LavaNode _lavaNode;
        private readonly Images _images;
        public static List<Mute> Mutes = new List<Mute>();
        private string previousMessage;
        public CommandHandler(IServiceProvider provider, DiscordSocketClient client,
            CommandService service, IConfiguration config,
            ServerServices serverServices, AutoRoleServices autoRoleServices,
            LavaNode lavaNode,
            Images images)
        {
            _logger = provider.GetRequiredService<ILogger<CommandHandler>>();
            _provider = provider;
            _client = client;
            _service = service;
            _config = config;
            _serverServices = serverServices;
            _autoRoleServices = autoRoleServices;
            _lavaNode = lavaNode;
            _images = images;
        }

        public override async Task InitializeAsync(CancellationToken cancellationToken)
        {
            _client.MessageReceived += OnMessageReceived;
            //_client.ChannelCreated += OnChannelCreated;
            //_client.JoinedGuild += OnJoinedGuild;
            _client.ReactionAdded += OnReactionAdded;
            _client.UserJoined += OnUserJoined;
            _client.Ready += OnReadyAsync ;
            _client.JoinedGuild += OnJoinedGuild;
            _client.LeftGuild += OnLeftGuild;
            _client.MessageReceived += WordFinder;

            var newTask = new Task(async () => await MuteHandler());
            newTask.Start();

            _service.CommandExecuted += OnCommandExecuted;

            await _service.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);
        }
        private async Task WordFinder(SocketMessage messageDetails)
        {
            await Task.Run(async () =>
            {
                var message = messageDetails as SocketUserMessage;
                if (!messageDetails.Author.IsBot)
                {
                    List<WordList> serverWordList = null;
                    using (var db = new DiscbotContext())
                    {
                        SocketGuild guild = (message.Channel as SocketGuildChannel)?.Guild;
                        serverWordList = db.WordList.AsEnumerable().Where(w => w.ServerId == (long)guild.Id).ToList();
                    }
                    bool wordFound = false;
                    foreach (var singleWord in serverWordList)
                    {
                        foreach (var content in messageDetails.Content.ToLower().Split(' '))
                        {
                            if (singleWord.Word.ToLower().Contains(content))
                            {
                                wordFound = true;
                            }
                        }
                    }
                    if (wordFound)
                    {
                        await messageDetails.DeleteAsync();
                    }
                }
            });
        }
        private async Task MuteHandler()
        {
            List<Mute> Remove = new List<Mute>();
            foreach(var mute in Mutes)
            {
                if (DateTime.Now < mute.End)
                {
                    continue;
                }
                var guild = _client.GetGuild(mute.Guild.Id);
                if (guild.GetRole(mute.Role.Id) == null)
                {
                    Remove.Add(mute);
                    continue;
                }
                var role = guild.GetRole(mute.Role.Id);
                if (guild.GetUser(mute.User.Id) == null)
                {
                    Remove.Add(mute);
                    continue;
                }
                var user = guild.GetUser(mute.User.Id);
                if (role.Position > guild.CurrentUser.Hierarchy)
                {
                    Remove.Add(mute);
                    continue;
                }
                await user.RemoveRoleAsync(mute.Role);
                Remove.Add(mute);
            }
            Mutes = Mutes.Except(Remove).ToList();

            await Task.Delay(1 * 60 * 1000);
            await MuteHandler();
        }
        private async Task OnLeftGuild(SocketGuild arg)
        {
            await _client.SetGameAsync($"over {_client.Guilds.Count} servers", null, ActivityType.Watching);
        }

        private async Task OnJoinedGuild(SocketGuild arg)
        {
            await _client.SetGameAsync($"over {_client.Guilds.Count} servers", null, ActivityType.Watching);
        }

        private async Task OnUserJoined(SocketGuildUser arg)
        {
           var newTask =  new Task(async () => await HandleUserJoined(arg));
           newTask.Start();
        }
        private async Task HandleUserJoined(SocketGuildUser arg)
        {
            var roles = await _autoRoleServices.TakeAutoRolesAsync(arg.Guild);
            if (roles.Count >0)
                    await arg.AddRolesAsync(roles);
            var channelId = await _serverServices.GetWelcomeAsync(arg.Guild.Id);
            if (channelId == 0)
                return;
            var channel = arg.Guild.GetTextChannel(channelId);
            if (channel == null)
            {
                await _serverServices.ClearWelcomeAsync(arg.Guild.Id);
                return;
            }
            var background = await _serverServices.GetBackgroundAsync(arg.Guild.Id);
            string path = await _images.CreateImageAsync(arg,background);
            await channel.SendFileAsync(path, null);
            System.IO.File.Delete(path);
        }
        private async Task OnReadyAsync()
        {
            // Avoid calling ConnectAsync again if it's already connected 
            // (It throws InvalidOperationException if it's already connected).
            if (!_lavaNode.IsConnected)
            {
                await _lavaNode.ConnectAsync();
            }

            // Other ready related stuff
        }
        //private async Task OnReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        //{
        //    // test the message id of the message that you want to check
        //    if (arg3.MessageId != 123456) return;
        //    // copy paste emote iwth backslash on discord 
        //    if (arg3.Emote.Name != " ") return;
        //    //finds the role id that belongs to this emote
        //    var role = (arg2 as SocketGuildChannel).Guild.Roles.FirstOrDefault(x => x.Id == 123456);
        //    await (arg3.User.Value as SocketGuildUser).AddRoleAsync(role);

        //}
        private async Task OnReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            // test the message id of the message that you want to check
            if (arg3.MessageId != 123456) return;
            // copy paste emote iwth backslash on discord 
            if (arg3.Emote.Name != " ") return;
            var channel = await (arg2 as ITextChannel).Guild.GetTextChannelAsync(123456);
            await channel.SendMessageAsync($"{arg3.User.Value.Mention} reacted");

        }

        //private async Task OnJoinedGuild(SocketGuild arg)
        //{
        //    await arg.DefaultChannel.SendMessageAsync("Thank you for the invite !");
        //}

        //private async Task OnChannelCreated(SocketChannel arg)
        //{
        //    if ((arg as ITextChannel) == null) return;
        //    var channel = arg as ITextChannel;
        //    await channel.SendMessageAsync("Welcome to a new channel!");
        //}

        private async Task OnMessageReceived(SocketMessage arg)
        {
            if (!(arg is SocketUserMessage message)) return;
            if (message.Source != MessageSource.User) return;
            string[] filters = new string[] { "discord.gg", "mlk", "swear" };
            if(message.Content.Split(" ").Intersect(filters).Any())
            {
                await message.DeleteAsync();
                await message.Channel.SendMessageAsync($"{message.Author.Mention} you cannot write such stuff!");
                return;
            }
            if (message.Content.Contains("https://discord.gg/"))
            {
                if(!(message.Channel as SocketGuildUser).Guild.GetUser(message.Author.Id).GuildPermissions.Administrator)
                {
                    await message.DeleteAsync();
                    await message.Channel.SendMessageAsync($"{message.Author.Mention} you cannot send Discord invite links!");
                    return;
                }
            }
            if (message.Content == previousMessage)
            {
                await message.DeleteAsync();
                await message.Channel.SendMessageAsync($"{message.Author.Mention} please dont spam!");
                return;
            }
            previousMessage = message.Content;
            var argPos = 0;
            var prefix = await _serverServices.GetGuildPrefix((message.Channel as SocketGuildChannel).Guild.Id) ?? "!";
            if (!message.HasStringPrefix(prefix, ref argPos) && !message.HasMentionPrefix(_client.CurrentUser, ref argPos)) return;

            var context = new SocketCommandContext(_client, message);
            await _service.ExecuteAsync(context, argPos, _provider);
        }
        private async Task HandleGreeting(SocketGuildUser user)
        {
            await Task.Run(async () =>
            {
                ServerGreeting shouldGreet = GetGreeting(user);
                if (shouldGreet != null && shouldGreet.GreetUsers == true)
                {
                    var sb = new StringBuilder();
                    ISocketMessageChannel messageChannel = null;
                    try
                    {
                        if (shouldGreet.GreetingChannelId != 0)
                        {
                            messageChannel = user.Guild.GetChannel((ulong)shouldGreet.GreetingChannelId) as ISocketMessageChannel;
                        }
                        else
                        {
                            messageChannel = user.Guild.DefaultChannel as ISocketMessageChannel;
                        }
                        var embed = new EmbedBuilder();
                        embed.Title = $"[{user.Username}] has joined [**{user.Guild.Name}**]!";
                        sb.AppendLine($"{user.Mention}");
                        if (string.IsNullOrEmpty(shouldGreet.Greeting))
                        {
                            sb.AppendLine($"Welcome them! :hugging:");
                            sb.AppendLine($"(or not, :shrug:)");
                        }
                        else
                        {
                            sb.AppendLine($"{shouldGreet.Greeting}");
                        }
                        embed.Description = sb.ToString();
                        embed.ThumbnailUrl = user.GetAvatarUrl();
                        embed.WithColor(new Color(0, 255, 0));
                        await messageChannel.SendMessageAsync("", false, embed.Build());
                    }
                    catch (Exception ex)
                    {
                        if (messageChannel != null)
                        {
                            _logger.LogError($"Error with channel -> [{messageChannel.Name}] on [{user.Guild.Name}] -> [{user.Guild.Id}] -> [{ex.Message}]");
                        }
                        else
                        {
                            _logger.LogError($"Error with no channel -> [{user.Guild.Name}] -> [{user.Guild.Id}] -> [{ex.Message}]");
                        }
                    }
                }
            });
        }

        private ServerGreeting GetGreeting(SocketGuildUser user)
        {
            ServerGreeting shouldGreet = null;
            var guildId = user.Guild.Id;
            using (var db = new DiscbotContext())
            {
                shouldGreet = db.ServerGreetings.AsEnumerable().Where(g => g.DiscordGuildId == (long)guildId).FirstOrDefault();
            }
            return shouldGreet;
        }
        private async Task OnCommandExecuted(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (command.IsSpecified && !result.IsSuccess) await (context.Channel as ISocketMessageChannel).SendErrorAsync("Error", result.ErrorReason);
        }
    }
}