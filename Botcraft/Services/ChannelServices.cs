﻿using Botcraft.Database.Entities;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Botcraft.Services
{
    public class ChannelServices
    {
        //if (Context.Channel is IDMChannel)
        //{
        //    await ReplyAsync("DM");
        //}
        //else if (Context.Channel is IGuildChannel)
        //{
        //    await ReplyAsync("Channel");
        //}
        public async Task SetGuildBotChannelAsync(ulong channelId, string channelName, ulong userId, string userName, string guildName, ulong guildId)
        {
            await Task.Run(async () =>
            {
                using (var db = new DiscbotContext())
                {
                    var currentChannel = db.ChannelOutputs.FirstOrDefault(o => o.ServerId == (long)guildId);
                    if (currentChannel == null)
                    {
                        var createChannel = new ChannelOutput
                        {
                            ChannelId = (long)channelId,
                            ChannelName = channelName,
                            ServerId = (long)guildId,
                            ServerName = guildName,
                            SetById = (long)userId,
                            SetByName = userName,
                            SetTime = DateTime.Now
                        };
                        db.ChannelOutputs.Add(createChannel);
                    }
                    else
                    {
                        currentChannel.ChannelId = (long)channelId;
                        currentChannel.ChannelName = channelName;
                        currentChannel.ServerId = (long)guildId;
                        currentChannel.ServerName = guildName;
                        currentChannel.SetById = (long)userId;
                        currentChannel.SetByName = userName;
                        currentChannel.SetTime = DateTime.Now;
                    }
                    await db.SaveChangesAsync();
                }
            });
        }

        public ChannelOutput GetGuildBotChannel(ulong guildId)
        {
            ChannelOutput outputChannel = new ChannelOutput();

            using (var db = new DiscbotContext())
            {
                outputChannel = db.ChannelOutputs.FirstOrDefault(o => o.ServerId == (long)guildId);
            }
            if (outputChannel == null)
            {
                outputChannel = new ChannelOutput();
            }

            return outputChannel;
        }

        public async Task Reply(ICommandContext context, EmbedBuilder embed)
        {
            await Task.Run(async () =>
            {
                ChannelOutput replyChannel;
                var guildInfo = context.Guild;
                if (guildInfo == null)
                {
                    replyChannel = new ChannelOutput();
                }
                else
                {
                    replyChannel = GetGuildBotChannel(context.Guild.Id);
                }
                if (!string.IsNullOrEmpty(replyChannel.ChannelName))
                {
                    var messageChannel = await context.Client.GetChannelAsync((ulong)replyChannel.ChannelId) as ISocketMessageChannel;
                    await messageChannel.SendMessageAsync("", false, embed.Build());
                }
                else
                {
                    await context.Channel.SendMessageAsync("", false, embed.Build());
                }
            });
        }

        public async Task Reply(ICommandContext context, string message)
        {
            await Task.Run(async () =>
            {
                ChannelOutput replyChannel;
                var guildInfo = context.Guild;
                if (guildInfo == null)
                {
                    replyChannel = new ChannelOutput();
                }
                else
                {
                    replyChannel = GetGuildBotChannel(context.Guild.Id);
                }
                if (!string.IsNullOrEmpty(replyChannel.ChannelName))
                {
                    var messageChannel = await context.Client.GetChannelAsync((ulong)replyChannel.ChannelId) as ISocketMessageChannel;
                    await messageChannel.SendMessageAsync(message);
                }
                else
                {
                    await context.Channel.SendMessageAsync(message);
                }
            });
        }

        //public async Task SetLoadingEmoji(ICommandContext context)
        //{
        //    //await context.Message.AddReactionAsync(EmojiExtensions.FromText(":cd:"));
        //}

        //public async Task SetDoneEmoji(ICommandContext context)
        //{
        //    ChannelPermissions botPerms = await GetBotPerms(context);
        //    if (botPerms.ManageMessages)
        //    {
        //        await context.Message.RemoveAllReactionsAsync();
        //    }
        //    //await context.Message.AddReactionAsync(EmojiExtensions.FromText(":white_check_mark:"));
        //}

        private async Task<ChannelPermissions> GetBotPerms(ICommandContext context)
        {
            var botUser = await context.Guild.GetUserAsync(291289031822802944);
            var perms = botUser.GetPermissions(context.Channel as IGuildChannel);
            return perms;
        }
    }
}
