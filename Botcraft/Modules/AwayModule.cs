using Botcraft.Database.Entities;
using Botcraft.Handler;
using Botcraft.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Botcraft.Modules
{
    public class AwayModule : ModuleBase<SocketCommandContext>
    {
        private static bool _isLinked = false;
        private static ChannelServices _channelServices = null;
        private readonly ILogger _logger;
        //Work on way to do this when bot starts
        public AwayModule( ILogger<AwayModule> logger)
        {
            _logger = logger;
            if (!_isLinked)
            {
                CommandHandler._client.MessageReceived += AwayMentionFinder;
                _logger.LogInformation($"Hooked into message received for away commands.");
            }
            _isLinked = true;
            if (_channelServices == null)
            {
                _channelServices = new ChannelServices();
            }
        }

        [Command("away", RunMode = RunMode.Async)]
        [Alias("afk")]
        [Summary("Set yourself as away, replying to anyone that @mentions you")]
        public async Task SetAway([Remainder] string input)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                var message = input;
                var user = Context.User;
                string userName = string.Empty;
                string userMentionName = string.Empty;
                if (user != null)
                {
                    userName = user.Username;
                    userMentionName = user.Mention;
                }
                var data = new AwayServices();
                var away = new AwaySystem();
                var attempt = data.GetAwayUser(userName);

                if (string.IsNullOrEmpty(message.ToString()))
                {
                    message = "No message set!";
                }
                if (attempt != null)
                {
                    away.UserName = attempt.UserName;
                    away.Status = attempt.Status;
                    if ((bool)away.Status)
                    {
                        sb.AppendLine($"You're already away, **{userMentionName}**!");
                    }
                    else
                    {
                        sb.AppendLine($"Marking you as away, **{userMentionName}**, with the message: *{message.ToString()}*");
                        away.Status = true;
                        away.Message = message;
                        away.UserName = userName;
                        away.TimeAway = DateTime.Now;

                        var awayData = new AwayServices();
                        awayData.SetAwayUser(away);
                    }
                }
                else
                {
                    sb.AppendLine($"Marking you as away, **{userMentionName}**, with the message: *{message.ToString()}*");
                    away.Status = true;
                    away.Message = message;
                    away.UserName = userName;
                    away.TimeAway = DateTime.Now;

                    var awayData = new AwayServices();
                    awayData.SetAwayUser(away);
                }
                await _channelServices.Reply(Context, sb.ToString());
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Something went wrong setting you away :(");
                _logger.LogError($"Away command error {ex.Message}");
                await _channelServices.Reply(Context, sb.ToString());
            }
        }

        [Command("back", RunMode = RunMode.Async)]
        [Summary("Set yourself as back from being away")]
        public async Task SetBack(bool forced = false, IGuildUser forceUser = null)
        {
            try
            {
                IGuildUser user = null;
                StringBuilder sb = new StringBuilder();
                var data = new AwayServices();
                if (forced)
                {
                    user = forceUser;
                }
                else
                {
                    user = Context.User as IGuildUser;
                }

                string userName = string.Empty;
                string userMentionName = string.Empty;
                if (user != null)
                {
                    userName = user.Username;
                    userMentionName = user.Mention;
                }
                var attempt = data.GetAwayUser(userName);
                var away = new AwaySystem();

                if (attempt != null)
                {
                    away.UserName = attempt.UserName;
                    away.Status = attempt.Status;
                    if (!(bool)away.Status)
                    {
                        sb.AppendLine($"You're not even away yet, **{userMentionName}**");
                    }
                    else
                    {
                        away.Status = false;
                        away.Message = string.Empty;
                        var awayData = new AwayServices();
                        awayData.SetAwayUser(away);
                        string awayDuration = string.Empty;
                        if (attempt.TimeAway.HasValue)
                        {
                            var awayTime = DateTime.Now - attempt.TimeAway;
                            if (awayTime.HasValue)
                            {
                                awayDuration = $"**{awayTime.Value.Days}** days, **{awayTime.Value.Hours}** hours, **{awayTime.Value.Minutes}** minutes, and **{awayTime.Value.Seconds}** seconds";
                            }
                        }
                        if (forced)
                        {
                            sb.AppendLine($"You're now set as back **{userMentionName}** (forced by: **{Context.User.Username}**)!");
                        }
                        else
                        {
                            sb.AppendLine($"You're now set as back, **{userMentionName}**!");
                        }
                        sb.AppendLine($"You were away for: [{awayDuration}]");
                    }
                    await _channelServices.Reply(Context, sb.ToString());
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Something went wrong marking you as back :(");
                _logger.LogError($"Back command error {ex.Message}");
                await _channelServices.Reply(Context, sb.ToString());
            }
        }

        [Command("set-back", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task SetBack(IGuildUser user)
        {
            await SetBack(forced: true, forceUser: user);
        }

        private async Task AwayMentionFinder(SocketMessage messageDetails)
        {
            await Task.Run(async () =>
            {
                var message = messageDetails as SocketUserMessage;
                if (!messageDetails.Author.IsBot)
                {
                    var userMentioned = messageDetails.MentionedUsers.ToList();
                    if (userMentioned != null)
                    {
                        foreach (var user in userMentioned)
                        {
                            var awayData = new AwayServices();
                            var awayUser = awayData.GetAwayUser(user.Username);
                            if (awayUser != null)
                            {
                                string awayDuration = string.Empty;
                                if (awayUser.TimeAway.HasValue)
                                {
                                    var awayTime = DateTime.Now - awayUser.TimeAway;
                                    if (awayTime.HasValue)
                                    {
                                        awayDuration = $"**{awayTime.Value.Days}** days, **{awayTime.Value.Hours}** hours, **{awayTime.Value.Minutes}** minutes, and **{awayTime.Value.Seconds}** seconds";
                                    }
                                }
                                _logger.LogInformation($"Mentioned user {user.Username} -> {awayUser.UserName} -> {awayUser.Status}");
                                if ((bool)awayUser.Status)
                                {
                                    if (user.Username == (awayUser.UserName))
                                    {
                                        SocketGuild guild = (message.Channel as SocketGuildChannel)?.Guild;
                                        EmbedBuilder embed = new EmbedBuilder();
                                        embed.WithColor(new Color(0, 71, 171));

                                        if (!string.IsNullOrWhiteSpace(guild.IconUrl))
                                        {
                                            embed.ThumbnailUrl = user.GetAvatarUrl();
                                        }

                                        embed.Title = $":clock: {awayUser.UserName} is away! :clock:";
                                        embed.Description = $"Since: **{awayUser.TimeAway}\n**Duration: {awayDuration}\nMessage: {awayUser.Message}";
                                        await messageDetails.Channel.SendMessageAsync("", false, embed.Build());
                                    }
                                }
                            }
                        }
                    }
                }
            });
        }
    }
}
