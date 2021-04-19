using Botcraft.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Botcraft.Modules
{
    public class RankModule : ModuleBase<SocketCommandContext>
    {
        private readonly RankServices _rankServices;
        public RankModule(RankServices rankServices)
        {
            _rankServices = rankServices;
        }
        [Command("rank", RunMode = RunMode.Async)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task Rank([Remainder] string identifier)
        {
            await Context.Channel.TriggerTypingAsync();
            var ranks = await _rankServices.TakeRanksAsync(Context.Guild);

            IRole role;
            if (ulong.TryParse(identifier, out ulong roleId))
            {
                var roleById = Context.Guild.Roles.FirstOrDefault(x => x.Id == roleId);
                if (roleById == null)
                {
                    await ReplyAsync("That role does not exist!");
                    return;
                }
                role = roleById;
            }
            else
            {
                var roleByName = Context.Guild.Roles.FirstOrDefault(x => string.Equals(x.Name, identifier, StringComparison.CurrentCultureIgnoreCase));
                if (roleByName == null)
                {
                    await ReplyAsync("That role doesn't exist!");
                    return;
                }
                role = roleByName;
            }
            if (ranks.All(x => x.Id != role.Id))
            {
                await ReplyAsync("That role doesn't exist!");
            }
            if ((Context.User as SocketGuildUser).Roles.Any(x => x.Id == role.Id))
            {
                await (Context.User as SocketGuildUser).RemoveRoleAsync(role);
                await ReplyAsync($"Successfully removed the rank{role.Mention} from you.");
                return;
            }
            await (Context.User as SocketGuildUser).AddRoleAsync(role);
            await ReplyAsync($"Successfully added the rank{role.Mention} from you.");
        }
        [Command("ranks", RunMode = RunMode.Async)]
        public async Task Ranks()
        {
            var ranks = await _rankServices.TakeRanksAsync(Context.Guild);
            if (ranks.Count == 0)
            {
                await ReplyAsync("No ranks in this server!");
                return;
            }

            await Context.Channel.TriggerTypingAsync();

            string description = "This message lists all ranks !\n In order to add a rank you can use the name or ID of the rank. ";
            foreach (var rank in ranks)
            {
                description += $"\n{rank.Mention}({rank.Id})";
            }

            await ReplyAsync(description);
        }

        [Command("addrank", RunMode = RunMode.Async)]
        [RequireUserPermission(Discord.GuildPermission.Administrator)]
        [RequireUserPermission(Discord.GuildPermission.ManageRoles)]
        public async Task AddRank([Remainder] string name)
        {
            await Context.Channel.TriggerTypingAsync();
            var ranks = await _rankServices.TakeRanksAsync(Context.Guild);

            var role = Context.Guild.Roles.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
            if (role == null)
            {
                await ReplyAsync("Role doesn't exist");
                return;
            }
            if (role.Position > Context.Guild.CurrentUser.Hierarchy)
            {
                await ReplyAsync("That role has a higher position than the bot !");
                return;
            }
            if (ranks.Any(x => x.Id == role.Id))
            {
                await ReplyAsync("That role is already a rank !");
                return;
            }
            await _rankServices.AddRankAsync(Context.Guild.Id, role.Id);
            await ReplyAsync($"The role {role.Mention} has been added to the ranks !");
        }
        [Command("delrank", RunMode = RunMode.Async)]
        [RequireUserPermission(Discord.GuildPermission.Administrator)]
        [RequireUserPermission(Discord.GuildPermission.ManageRoles)]
        public async Task DelRank([Remainder] string name)
        {
            await Context.Channel.TriggerTypingAsync();
            var ranks = await _rankServices.TakeRanksAsync(Context.Guild);
            var role = Context.Guild.Roles.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
            if (role == null)
            {
                await ReplyAsync("Role doesn't exist");
                return;
            }
            if (ranks.All(x => x.Id != role.Id))
            {
                await ReplyAsync("Tha role is not a rank yet");
                return;
            }
            await _rankServices.RemoveRankAsync(Context.Guild.Id, role.Id);
            await ReplyAsync($"The role {role.Mention}has been removed from the ranks !");
        }
    }
}
