using Botcraft.Common;
using Botcraft.Database.Entities;
using Botcraft.Handler;
using Botcraft.Models;
using Botcraft.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Botcraft.Modules
{
    public class ConfigurationModule : ModuleBase<SocketCommandContext>
    {
        private readonly ServerServices _serverServices;
        private readonly ChannelServices _channelServices;
        private readonly ConfigurationServices _configurationServices;
        public ConfigurationModule(ServerServices serverServices, ChannelServices channelServices, ConfigurationServices configurationServices)
        {
            _serverServices = serverServices;
            _configurationServices = configurationServices;
            _channelServices = channelServices;
        }
        [Command("set-status", RunMode = RunMode.Async)]
        [RequireOwner]
        public async Task SetStatus([Remainder] string args = null)
        {
            await CommandHandler._client.SetGameAsync(args);
        }
        [Command("cprefix", RunMode = RunMode.Async)]
        [RequireUserPermission(Discord.GuildPermission.Administrator)]
        public async Task Prefix(string prefix = null)
        {
            if (prefix == null)
            {
                var guildPrefix = await _serverServices.GetGuildPrefix(Context.Guild.Id) ?? "!";
                await ReplyAsync($"The current prefix of this bot is `{guildPrefix}` .");
                return;
            }
            if (prefix.Length > 8)
            {
                await ReplyAsync("The lenght is out of bounds1");
                return;
            }
            await _serverServices.ModifyGuildPrefix(Context.Guild.Id, prefix);
            await ReplyAsync($"The prefix has been adjusted to `{prefix}` . ");
            await _serverServices.SendLogAsync(Context.Guild, "Prefix adjusted", $"{Context.User.Mention} modified the prefix to `{prefix}`");
        }

        [Command("purge")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task Purge(int amount)
        {
            var messages = await Context.Channel.GetMessagesAsync(amount + 1).FlattenAsync();
            await (Context.Channel as SocketTextChannel).DeleteMessagesAsync(messages);

            var message = await Context.Channel.SendMessageAsync($"{messages.Count()} messages deleted");
            await Task.Delay(2500);
            await message.DeleteAsync();
        }
        [Command("Show-Servers")]
        [Summary("Show the servers the bot is in")]
        [RequireOwner]
        public async Task ListGuilds()
        {
            StringBuilder sb = new StringBuilder();
            var guilds = CommandHandler._client.Guilds.ToList();
            foreach (var guild in guilds)
            {
                sb.AppendLine($"Name: {guild.Name} Id: {guild.Id} Owner: {guild.Owner}");
            }
            await _channelServices.Reply(Context, sb.ToString());
        }
        [Command("Announce", RunMode = RunMode.Async)]
        [RequireOwner]
        public async Task AnnounceMessage([Remainder] string message)
        {
            var guilds = CommandHandler._client.Guilds.ToList();
            foreach (var guild in guilds)
            {
                var messageChannel = guild.DefaultChannel as ISocketMessageChannel;
                if (messageChannel != null)
                {
                    var embed = new EmbedBuilder();
                    embed.Title = "NinjaBot Announcement";
                    embed.Description = message;
                    embed.ThumbnailUrl = Context.User.GetAvatarUrl();
                    await messageChannel.SendMessageAsync("", false, embed.Build());
                    System.Threading.Thread.Sleep(5000);
                }
            }
        }
        [Command("server")]
        public async Task Server()
        {
            var builder = new EmbedBuilder()
                .WithThumbnailUrl(Context.Guild.IconUrl)
                .WithDescription("Server info")
                .WithTitle($"{Context.Guild.Name} Info")
                .WithColor(new Color(15, 30, 45))
                .AddField("Created at ", Context.Guild.CreatedAt.ToString("dd/MM/yyyy"), true)
                .AddField("Member Count ", (Context.Guild as SocketGuild).MemberCount + " members", true)
                .AddField("Online Users : ", (Context.Guild as SocketGuild).Users.Where(x => x.Status != UserStatus.Offline).Count() + " members", true);
            var embed = builder.Build();
            await Context.Channel.SendMessageAsync(null, false, embed);
        }
        [Command("mute", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task Mute(SocketGuildUser user, int minutes, [Remainder] string reason = null)
        {
            if (user.Hierarchy > Context.Guild.CurrentUser.Hierarchy)
            {
                await Context.Channel.SendErrorAsync("Invalid user", "That user has a higher position than the bot.");
                return;
            }
            var role = (Context.Guild as IGuild).Roles.FirstOrDefault(x => x.Name == "Muted");
            if (role == null)
            {
                role = await Context.Guild.CreateRoleAsync("Muted", new GuildPermissions(sendMessages: false), null, false, null);
            }
            if (role.Position > Context.Guild.CurrentUser.Hierarchy)
            {
                await Context.Channel.SendErrorAsync("Invalid permissions", "The muted role has a higher position than the bot.");
                return;
            }
            if (user.Roles.Contains(role))
            {
                await Context.Channel.SendErrorAsync("Already muted", "That user is already muted.");
                return;
            }
            await role.ModifyAsync(x => x.Position = Context.Guild.CurrentUser.Hierarchy);

            foreach (var channel in Context.Guild.TextChannels)
            {
                if (channel.GetPermissionOverwrite(role).HasValue || channel.GetPermissionOverwrite(role).Value.SendMessages == PermValue.Allow)
                {
                    await channel.AddPermissionOverwriteAsync(role, new OverwritePermissions(sendMessages: PermValue.Deny));
                }
            }
            CommandHandler.Mutes.Add(new Mute { Guild = Context.Guild, User = user, End = DateTime.Now + TimeSpan.FromMinutes(minutes), Role = role });
            await user.AddRoleAsync(role);
            await Context.Channel.SendSuccessAsync($"Muted {user.Username}", $"Duration: {minutes} minutes\n Reason:{reason ?? "None"}");
        }
        [Command("unmute", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task Unmute(SocketGuildUser user)
        {
            var role = (Context.Guild as IGuild).Roles.FirstOrDefault(x => x.Name == "Muted");
            if (role == null)
            {
                await Context.Channel.SendErrorAsync("Not muted", "This user isn't muted yet.");
                return;
            }
            if (role.Position > Context.Guild.CurrentUser.Hierarchy)
            {
                await Context.Channel.SendErrorAsync("Invalid permissions", "The muted role has a higher position than the bot.");
                return;
            }
            if (user.Roles.Contains(role))
            {
                await Context.Channel.SendErrorAsync("Not muted", "This user isn't muted yet.");
                return;
            }
            await user.RemoveRoleAsync(role);
            await Context.Channel.SendSuccessAsync($"Unmuted {user.Username}", $"Successfully unmuted the user.");
        }
        [Command("slowmode", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [RequireBotPermission(GuildPermission.ManageChannels)]
        public async Task Slowmode(int interval = 0)
        {
            await (Context.Channel as SocketTextChannel).ModifyAsync(x => x.SlowModeInterval = interval);
            await ReplyAsync($"The slowmode interval was adjusted to {interval} seconds.");
        }
        [Command("kick", RunMode = RunMode.Async)]
        [Summary("Kick someone, not nice... but needed sometimes")]
        [RequireBotPermission(GuildPermission.KickMembers)]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task KickUser(IGuildUser user, [Remainder] string reason = null)
        {
            var embed = new EmbedBuilder();
            embed.ThumbnailUrl = user.GetAvatarUrl();
            StringBuilder sb = new StringBuilder();
            try
            {
                //await user.SendMessageAsync($"You've been kicked from [**{Context.Guild.Name}**] by [**{Context.User.Username}**]: [**{reason}**]");
                await user.KickAsync();
                embed.Title = $"Kicking {user.Username}";
                if (string.IsNullOrEmpty(reason))
                {
                    reason = "Buh bye.";
                }
                sb.AppendLine($"Reason: [**{reason}**]");
            }
            catch (Exception ex)
            {
                embed.Title = $"Error attempting to kick {user.Username}";
                sb.AppendLine($"[{ex.Message}]");
            }
            embed.Description = sb.ToString();
            embed.WithColor(new Color(0, 0, 255));
            await _channelServices.Reply(Context, embed);
        }
        [Command("ban", RunMode = RunMode.Async)]
        [Summary("Ban someone, not nice... but needed sometimes")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task BanUser(IGuildUser user, [Remainder] string args = null)
        {
            int pruneDays = 0;
            string reason = "Buy bye!";
            if (args != null)
            {
                try
                {
                    pruneDays = int.Parse(args.Split(" ")[0]);
                }
                catch (Exception ex)
                {
                    pruneDays = 0;
                }
                var numArgs = args.Split(" ").Count();
                if (numArgs > 1)
                {
                    int iValue = 0;
                    if (pruneDays > 0)
                    {
                        iValue = 1;
                    }
                    reason = string.Empty;
                    for (int i = iValue; i <= numArgs - 1; i++)
                    {
                        if (i + 1 == numArgs - 1)
                        {
                            reason += $"{args.Split(" ")[i]}";
                        }
                        else
                        {
                            reason += $" {args.Split(" ")[i]} ";
                        }
                    }
                    reason = reason.Trim();
                }
                else if (pruneDays == 0)
                {
                    reason = args;
                }
            }
            var embed = new EmbedBuilder();
            embed.ThumbnailUrl = user.GetAvatarUrl();
            StringBuilder sb = new StringBuilder();
            try
            {
                //await user.SendMessageAsync($"You have been banned from [**{Context.Guild.Name}**] -> [**{reason}**]");
                await Context.Guild.AddBanAsync(user, pruneDays, reason);
                embed.Title = $"Banning {user.Username}";
                sb.AppendLine($"Reason: [**{reason}**]");
            }
            catch (Exception ex)
            {
                embed.Title = $"Error attempting to ban {user.Username}";
                sb.AppendLine($"[{ex.Message}]");
            }
            embed.Description = sb.ToString();
            embed.WithColor(new Color(0, 0, 255));
            await _channelServices.Reply(Context, embed);
        }

        [Command("unban", RunMode = RunMode.Async)]
        [Summary("Unban someone... whew!")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task UnBanUser(string user)
        {
            var embed = new EmbedBuilder();
            embed.ThumbnailUrl = Context.Guild.IconUrl;
            StringBuilder sb = new StringBuilder();
            var currentBans = await Context.Guild.GetBansAsync();
            var bannedUser = currentBans.Where(c => c.User.Username.Contains(user)).FirstOrDefault();
            if (bannedUser != null)
            {
                try
                {
                    await Context.Guild.RemoveBanAsync(bannedUser.User.Id);
                    embed.Title = $"UnBanning {bannedUser.User.Username}";
                }
                catch (Exception ex)
                {
                    embed.Title = $"Error attempting to unban {bannedUser.User.Username}";
                    sb.AppendLine($"[{ex.Message}]");
                }
            }
            else
            {
                embed.Title = $"{user} not found!";
                sb.AppendLine($"Unable to find [{user}] in the ban list!");
            }
            embed.Description = sb.ToString();
            embed.WithColor(new Color(0, 0, 255));
            await _channelServices.Reply(Context, embed);
        }
        [Command("list-bans", RunMode = RunMode.Async)]
        [Summary("List the users currently banned on the server")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task ListBans()
        {
            var embed = new EmbedBuilder();
            embed.ThumbnailUrl = Context.Guild.IconUrl;
            StringBuilder sb = new StringBuilder();
            try
            {
                embed.Title = $"User bans on {Context.Guild.Name}";
                var bans = await Context.Guild.GetBansAsync();
                if (bans.Count > 0)
                {
                    foreach (var ban in bans)
                    {
                        string reason = ban.Reason;
                        if (string.IsNullOrEmpty(reason))
                        {
                            reason = "/shrug";
                        }
                        sb.AppendLine($":black_medium_small_square: **{ban.User.Username}** (*{reason}*)");
                    }
                }
                else
                {
                    sb.AppendLine($"Much empty, such space!");
                }

            }
            catch (Exception ex)
            {
                embed.Title = $"Error attempting to list bans for **{Context.Guild.Name}**";
                sb.AppendLine($"[{ex.Message}]");
            }
            embed.Description = sb.ToString();
            embed.WithColor(new Color(0, 0, 255));
            //await _client.Log("test")
            await _channelServices.Reply(Context, embed);
        }
        [Command("Leave-Server")]
        [Summary("Leave a server")]
        [RequireOwner]
        public async Task LeaveServer([Remainder] ulong serverId)
        {
            await CommandHandler._client.GetGuild(serverId).LeaveAsync();
        }
        [Command("set-note", RunMode = RunMode.Async)]
        [Alias("snote")]
        [Summary("Set a note associated with a discord server")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task SetNote([Remainder] string note)
        {
            string result = await _configurationServices.SetNoteInfo(Context, note);
            var embed = new EmbedBuilder();
            embed.Title = $":notepad_spiral:Notes for {Context.Guild.Name}:notepad_spiral:";
            embed.Description = result;
            embed.ThumbnailUrl = Context.Guild.IconUrl;
            embed.WithColor(new Color(0, 255, 0));
            await _channelServices.Reply(Context, embed);
        }

        [Command("get-note", RunMode = RunMode.Async)]
        [Alias("note")]
        [Summary("Get a note associated with a discord server")]
        public async Task GetNote()
        {
            string note = await _configurationServices.GetNoteInfo(Context);
            var embed = new EmbedBuilder();
            embed.Title = $":notepad_spiral:Notes for {Context.Guild.Name}:notepad_spiral:";
            embed.ThumbnailUrl = Context.Guild.IconUrl;
            embed.Description = note;
            embed.WithColor(new Color(0, 255, 0));
            await _channelServices.Reply(Context, embed);
        }
        [Command("warn", RunMode = RunMode.Async)]
        [Summary("Send a warning message to a user")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task WarnUser(IGuildUser user, [Remainder] string message = null)
        {
            int numWarnings = 0;
            var currentWarnings = await _configurationServices.GetWarning(Context, user);
            var warnMessage = new StringBuilder();
            if (message == null)
            {
                warnMessage.AppendLine($"{user.Mention},");
                message = $":warning: You have been issued a warning from: {Context.User.Username}! :warning:";
            }
            else
            {
                warnMessage.AppendLine($":warning: {user.Mention}, you have been issued the following warning (from: {Context.User.Username}) :warning:");

            }
            if (currentWarnings != null)
            {
                numWarnings = currentWarnings.NumWarnings + 1;
            }
            else
            {
                numWarnings = 1;
            }
            warnMessage.AppendLine(message);
            switch (numWarnings)
            {
                case 1:
                    {
                        warnMessage.AppendLine("This is your first warning. At three warnings, you will be kicked!");
                        break;
                    }
                case 2:
                    {
                        warnMessage.AppendLine("This is your second warning. At three warnings, you will be kicked!");
                        break;
                    }
                case 3:
                    {
                        warnMessage.AppendLine("This was your final warning, goodbye!");
                        break;
                    }
            }
            try
            {
                _configurationServices.AddWarning(Context, user);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Unable to log warning in database -> [{ex.Message}]!");
            }
            await user.SendMessageAsync(warnMessage.ToString());
            await _channelServices.Reply(Context, warnMessage.ToString());
            if (numWarnings >= 3)
            {
                await KickUser(user, "Maximum number of warnings reached!");
                _configurationServices.ResetWarnings(currentWarnings);
            }
        }

        [Command("reset-warnings", RunMode = RunMode.Async)]
        [Summary("Reset warnings for a user")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task ResetWarning(IGuildUser user)
        {
            var warnings = await _configurationServices.GetWarning(Context, user);
            if (warnings != null)
            {
                _configurationServices.ResetWarnings(warnings);
                await _channelServices.Reply(Context, $"Warnings reset for **{user.Username}**");
            }
            else
            {
                await _channelServices.Reply(Context, $"No warnings found for **{user.Username}**!");
            }
        }
        [Command("add-word")]
        public async Task AddWord([Remainder] string word)
        {
            var sb = new StringBuilder();
            using (var db = new DiscbotContext())
            {
                var words = db.WordList.AsEnumerable().Where(w => w.ServerId == (long)Context.Guild.Id).ToList();
                bool wordFound = false;
                foreach (var singleWord in words)
                {
                    if (singleWord.Word.ToLower().Contains(word.ToLower()))
                    {
                        wordFound = true;
                    }
                }
                if (wordFound)
                {
                    sb.AppendLine($"[{word}] is already in the list!");
                }
                else
                {
                    sb.AppendLine($"Adding [{word}] to the list!");
                    db.Add(new WordList
                    {
                        ServerId = (long)Context.Guild.Id,
                        ServerName = Context.Guild.Name,
                        Word = word,
                        SetById = (long)Context.User.Id
                    });
                    await db.SaveChangesAsync();
                }

            }
            await _channelServices.Reply(Context, sb.ToString());
        }
    }
}
