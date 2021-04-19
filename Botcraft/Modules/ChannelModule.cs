using Botcraft.Database.Entities;
using Botcraft.Services;
using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Botcraft.Modules
{
    public class ChannelModule : ModuleBase<SocketCommandContext>
    {


        private readonly ServerServices _serverServices;
        private readonly ChannelServices _channelServices;
        public ChannelModule(ServerServices serverServices , ChannelServices channelServices)
        {
            _serverServices = serverServices;
            _channelServices = channelServices;
        }

        [Command("welcome")]
        [RequireUserPermission(Discord.GuildPermission.Administrator)]
        public async Task Welcome(string option = null, string value = null)
        {
            if (option == null && value == null)
            {
                var fetchedChannelId = await _serverServices.GetWelcomeAsync(Context.Guild.Id);
                if (fetchedChannelId == 0)
                {
                    await ReplyAsync("There has not been set a welcome channel yet!");
                    return;
                }
                var fetchedChannel = Context.Guild.GetTextChannel(fetchedChannelId);
                if (fetchedChannel == null)
                {
                    await ReplyAsync("There has not been set a welcome channel yet!");
                    await _serverServices.ClearWelcomeAsync(Context.Guild.Id);
                    return;
                }
                var fetchedBackground = await _serverServices.GetBackgroundAsync(Context.Guild.Id);
                if (fetchedBackground != null)
                {
                    await ReplyAsync($"The channel used for the welcome module is{fetchedChannel.Mention}.\n The background is set to {fetchedBackground}. ");
                }
                else
                {
                    await ReplyAsync($"The channel used for the welcome module is{fetchedChannel.Mention}.");
                }
                return;
            }
            if (option == "channel" && value != null)
            {
                if (!MentionUtils.TryParseChannel(value, out ulong parseId))
                {
                    await ReplyAsync("Please pass in a valid channel!");
                    return;
                }
                var parsedChannel = Context.Guild.GetTextChannel(parseId);
                if (parsedChannel == null)
                {
                    await ReplyAsync("Please pass in a valid channel!");
                    return;
                }
                await _serverServices.ModifyWelcomeAsync(Context.Guild.Id, parseId);
                await ReplyAsync($"Successfully modified the welcome channel to {parsedChannel.Mention}.");
                return;
            }
            if (option == "background" && value != null)
            {
                if (value == "clear")
                {
                    await _serverServices.ClearBackgroundAsync(Context.Guild.Id);
                    await ReplyAsync("Successfully cleared the background for this server.");
                    return;
                }
                await _serverServices.ModifyBackgroundAsync(Context.Guild.Id, value);
                await ReplyAsync($"Successfully modified the background to {value}.");
                return;
            }
            if (option == "clear" && value == null)
            {
                await _serverServices.ClearWelcomeAsync(Context.Guild.Id);
                await ReplyAsync("Successfully cleared the welcome channel.");
                return;
            }

            await ReplyAsync("Wrong command options!");
        }
        [Command("logs")]
        [RequireUserPermission(Discord.GuildPermission.Administrator)]
        public async Task Logs(string value = null)
        {
            if (value == null)
            {
                var fetchedChannelId = await _serverServices.GetLogsAsync(Context.Guild.Id);
                if (fetchedChannelId == 0)
                {
                    await ReplyAsync("There has not been set a logs channel yet!");
                    return;
                }
                var fetchedChannel = Context.Guild.GetTextChannel(fetchedChannelId);
                if (fetchedChannel == null)
                {
                    await ReplyAsync("There has not been set a logs channel yet!");
                    await _serverServices.ClearLogsAsync(Context.Guild.Id);
                    return;
                }
                await ReplyAsync($"The channel used for the logs module is{fetchedChannel.Mention}.");
                return;
            }
            if (value != "clear")
            {
                if (!MentionUtils.TryParseChannel(value, out ulong parseId))
                {
                    await ReplyAsync("Please pass in a valid channel!");
                    return;
                }
                var parsedChannel = Context.Guild.GetTextChannel(parseId);
                if (parsedChannel == null)
                {
                    await ReplyAsync("Please pass in a valid channel!");
                    return;
                }
                await _serverServices.ModifyLogsAsync(Context.Guild.Id, parseId);
                await ReplyAsync($"Successfully modified the logs channel to {parsedChannel.Mention}.");
                return;
            }
            if (value == "clear")
            {
                await _serverServices.ClearLogsAsync(Context.Guild.Id);
                await ReplyAsync("Successfully cleared the logs channel.");
                return;
            }

            await ReplyAsync("Wrong command options!");
        }

        [Command("set-join-message", RunMode = RunMode.Async)]
        [Alias("set-join")]
        [Summary("Change the greeting message for when someone joins the server")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task ChangeGreeting([Remainder] string args = null)
        {
            var embed = new EmbedBuilder();
            StringBuilder sb = new StringBuilder();
            if (!string.IsNullOrEmpty(args))
            {
                embed.Title = $"Join greeting change for {Context.Guild.Name}";
                sb.AppendLine("New message:");
                sb.AppendLine(args);
                using (var db = new DiscbotContext())
                {
                    try
                    {
                        var guildGreetingInfo = db.ServerGreetings.AsEnumerable().Where(g => g.DiscordGuildId == (long)Context.Guild.Id).FirstOrDefault();
                        if (guildGreetingInfo != null)
                        {
                            guildGreetingInfo.Greeting = args.Trim();
                            guildGreetingInfo.SetById = (long)Context.User.Id;
                            guildGreetingInfo.SetByName = Context.User.Username;
                            guildGreetingInfo.TimeSet = DateTime.Now;
                        }
                        else
                        {
                            db.ServerGreetings.Add(new ServerGreeting
                            {
                                DiscordGuildId = (long)Context.Guild.Id,
                                Greeting = args.Trim(),
                                SetById = (long)Context.User.Id,
                                SetByName = Context.User.Username,
                                TimeSet = DateTime.Now
                            });
                        }
                        await db.SaveChangesAsync();
                    }
                    catch (Exception)
                    {
                        embed.Title = $"Error changing message";
                        sb.AppendLine($"{Context.User.Mention},");
                        sb.AppendLine($"I've encounted an error, please contact the owner for help.");
                    }
                }
            }
            else
            {
                embed.Title = $"Error changing message";
                sb.AppendLine($"{Context.User.Mention},");
                sb.AppendLine($"Please provided a message!");
            }
            embed.Description = sb.ToString();
            embed.WithColor(new Color(0, 255, 0));
            embed.ThumbnailUrl = Context.Guild.IconUrl;
            await _channelServices.Reply(Context, embed);
        }

        [Command("set-leave-message", RunMode = RunMode.Async)]
        [Alias("set-part")]
        [Summary("Change the message displayed when someone leaves the server")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task ChangeParting([Remainder] string args = null)
        {
            var embed = new EmbedBuilder();
            StringBuilder sb = new StringBuilder();
            if (!string.IsNullOrEmpty(args))
            {
                embed.Title = $"Parting message change for {Context.Guild.Name}";
                sb.AppendLine("New message:");
                sb.AppendLine(args);
                using (var db = new DiscbotContext())
                {
                    try
                    {
                        var guildGreetingInfo = db.ServerGreetings.AsEnumerable().Where(g => g.DiscordGuildId == (long)Context.Guild.Id).FirstOrDefault();
                        if (guildGreetingInfo != null)
                        {
                            guildGreetingInfo.PartingMessage = args.Trim();
                            guildGreetingInfo.SetById = (long)Context.User.Id;
                            guildGreetingInfo.SetByName = Context.User.Username;
                            guildGreetingInfo.TimeSet = DateTime.Now;
                        }
                        else
                        {
                            db.ServerGreetings.Add(new ServerGreeting
                            {
                                DiscordGuildId = (long)Context.Guild.Id,
                                PartingMessage = args.Trim(),
                                SetById = (long)Context.User.Id,
                                SetByName = Context.User.Username,
                                TimeSet = DateTime.Now
                            });
                        }
                        await db.SaveChangesAsync();
                    }
                    catch (Exception)
                    {
                        embed.Title = $"Error changing message";
                        sb.AppendLine($"{Context.User.Mention},");
                        sb.AppendLine($"I've encounted an error, please contact the owner for help.");
                    }
                }
            }
            else
            {
                embed.Title = $"Error changing message";
                sb.AppendLine($"{Context.User.Mention},");
                sb.AppendLine($"Please provided a message!");
            }
            embed.Description = sb.ToString();
            embed.WithColor(new Color(0, 255, 0));
            embed.ThumbnailUrl = Context.Guild.IconUrl;
            await _channelServices.Reply(Context, embed);
        }

        [Command("toggle-greetings", RunMode = RunMode.Async)]
        [Summary("Toogle greeting users that join/leave this server")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task ToggleGreetings()
        {
            var embed = new EmbedBuilder();
            StringBuilder sb = new StringBuilder();
            using (var db = new DiscbotContext())
            {
                try
                {
                    var currentSetting = db.ServerGreetings.AsEnumerable().Where(g => g.DiscordGuildId == (long)Context.Guild.Id).FirstOrDefault();
                    if (currentSetting != null)
                    {
                        if (currentSetting.GreetUsers == true)
                        {
                            currentSetting.GreetUsers = false;
                            sb.AppendLine("Greetings have been disabled!");
                        }
                        else
                        {
                            currentSetting.GreetUsers = true;
                            currentSetting.GreetingChannelId = (long)Context.Channel.Id;
                            currentSetting.GreetingChannelName = Context.Channel.Name;
                            sb.AppendLine("Greetings have been enabled!");
                        }
                    }
                    else
                    {
                        db.ServerGreetings.Add(new ServerGreeting
                        {
                            DiscordGuildId = (long)Context.Guild.Id,
                            GreetingChannelId = (long)Context.Channel.Id,
                            GreetingChannelName = Context.Channel.Name,
                            GreetUsers = true
                        });
                        sb.AppendLine("Greetings have been enabled!");
                    }
                    await db.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error toggling greetings -> [{ex.Message}]!");
                }
            }
            embed.Title = $"User greeting settings for {Context.Guild.Name}";
            embed.Description = sb.ToString();
            embed.WithColor(new Color(0, 255, 0));
            embed.ThumbnailUrl = Context.Guild.IconUrl;
            await _channelServices.Reply(Context, embed);
        }
        [Command("clear", RunMode = RunMode.Async)]
        [Summary("Clear an amount of messages in the channel")]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task ClearMessage([Remainder] int numberOfMessages = 5)
        {
            if (numberOfMessages > 25)
            {
                numberOfMessages = 25;
            }
            var messagesToDelete = await Context.Channel.GetMessagesAsync(numberOfMessages).FlattenAsync();
            await (Context.Channel as ITextChannel).DeleteMessagesAsync(messagesToDelete);
        }
    }
}
