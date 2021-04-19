using Botcraft.Services;
using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Botcraft.Modules
{
    public class AutoRoleModule : ModuleBase<SocketCommandContext>
    {

        private readonly AutoRoleServices _autoRoleServices;

        public AutoRoleModule(AutoRoleServices autoRoleServices)
        {
            _autoRoleServices = autoRoleServices;
        }

        [Command("autoroles", RunMode = RunMode.Async)]
        [RequireUserPermission(Discord.GuildPermission.Administrator)]
        public async Task AutoRoles()
        {
            var autoRoles = await _autoRoleServices.TakeAutoRolesAsync(Context.Guild);
            if (autoRoles.Count == 0)
            {
                await ReplyAsync("No autoroles in this server!");
                return;
            }

            await Context.Channel.TriggerTypingAsync();

            string description = "This message lists all ranks !\n In order to remove an autorole you can use the name or ID of the rank. ";
            foreach (var autoRole in autoRoles)
            {
                description += $"\n{autoRole.Mention}({autoRole.Id})";
            }

            await ReplyAsync(description);
        }

        [Command("addautorole", RunMode = RunMode.Async)]
        [RequireUserPermission(Discord.GuildPermission.Administrator)]
        [RequireUserPermission(Discord.GuildPermission.ManageRoles)]
        public async Task AddAutoRolek([Remainder] string name)
        {
            await Context.Channel.TriggerTypingAsync();
            var autoRoles = await _autoRoleServices.TakeAutoRolesAsync(Context.Guild);

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
            if (autoRoles.Any(x => x.Id == role.Id))
            {
                await ReplyAsync("That role is already a autorole !");
                return;
            }
            await _autoRoleServices.AddAutoRoleAsync(Context.Guild.Id, role.Id);
            await ReplyAsync($"The role {role.Mention} has been added to the autoroles !");
        }
        [Command("delautorole", RunMode = RunMode.Async)]
        [RequireUserPermission(Discord.GuildPermission.Administrator)]
        [RequireUserPermission(Discord.GuildPermission.ManageRoles)]
        public async Task DelAutoRole([Remainder] string name)
        {
            await Context.Channel.TriggerTypingAsync();
            var autoRoles = await _autoRoleServices.TakeAutoRolesAsync(Context.Guild);
            var role = Context.Guild.Roles.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
            if (role == null)
            {
                await ReplyAsync("Role doesn't exist");
                return;
            }
            if (autoRoles.All(x => x.Id != role.Id))
            {
                await ReplyAsync("Tha role is not an autorole yet");
                return;
            }
            await _autoRoleServices.RemoveAutoRoleAsync(Context.Guild.Id, role.Id);
            await ReplyAsync($"The role {role.Mention}has been removed from the autoroles !");
        }






    }
}
