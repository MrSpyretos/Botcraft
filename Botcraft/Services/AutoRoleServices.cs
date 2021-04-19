using Discord;
using Discord.WebSocket;
using Botcraft;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Botcraft.Database.Entities;

namespace Botcraft.Services
{
    public class AutoRoleServices
    {
        private readonly DiscbotContext _context;
        public AutoRoleServices( DiscbotContext context)
        {
            _context = context;
        }
        public async Task<List<IRole>> TakeAutoRolesAsync(IGuild guild)
        {
            var roles = new List<IRole>();
            var invalidAutoRoles = new List<AutoRole>();

            var autoRoles = await GetAutoRolesAsync(guild.Id);

            foreach (var autoRole in autoRoles)
            {
                var role = guild.Roles.FirstOrDefault(x => x.Id == autoRole.RoleId);
                if (role == null)
                {
                    invalidAutoRoles.Add(autoRole);
                }
                else
                {
                    var currentUser = await guild.GetCurrentUserAsync();
                    var hierarchy = (currentUser as SocketGuildUser).Hierarchy;
                    if (role.Position > hierarchy)
                    {
                        invalidAutoRoles.Add(autoRole);
                    }
                    else
                    {
                        roles.Add(role);
                    }
                }
            }
            if (invalidAutoRoles.Count > 0)
            {
                await ClearAutoRoleAsync(invalidAutoRoles);
            }
            return roles;
        }
        public async Task<List<AutoRole>> GetAutoRolesAsync(ulong id)
        {
            var autoRoles = await _context.AutoRoles
                .AsAsyncEnumerable()
                .Where(x => x.ServerId == id)
                .ToListAsync();

            return await Task.FromResult(autoRoles);
        }

        public async Task AddAutoRoleAsync(ulong id, ulong roleId)
        {
            var server = await _context.Servers
                .FindAsync(id);
            if (server == null)
            {
                _context.Add(new Server { Id = id });
            }
            _context.Add(new AutoRole { RoleId = roleId, ServerId = id });
            await _context.SaveChangesAsync();
        }
        public async Task RemoveAutoRoleAsync(ulong id, ulong roleId)
        {
            var autoRole = await _context.AutoRoles
                .AsAsyncEnumerable()
                .Where(x => x.RoleId == roleId)
                .FirstOrDefaultAsync();
            _context.Remove(autoRole);
            await _context.SaveChangesAsync();
        }

        public async Task ClearAutoRoleAsync(List<AutoRole> autoRoles)
        {
            _context.RemoveRange(autoRoles);
            await _context.SaveChangesAsync();
        }
    }
}
