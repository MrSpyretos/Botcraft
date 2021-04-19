using Discord;
using Discord.WebSocket;
using Botcraft;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Botcraft.Database.Entities;

namespace Botcraft.Services
{

    public class RankServices
    {
        private readonly DiscbotContext _context;
        public RankServices(DiscbotContext context)
        {
            _context = context;
        }
        public async Task<List<IRole>> TakeRanksAsync(IGuild guild)
        {
            var roles = new List<IRole>();
            var invalidRanks = new List<Rank>();

            var ranks = await GetRanksAsync(guild.Id);

            foreach(var rank in ranks)
            {
                var role = guild.Roles.FirstOrDefault(x => x.Id == rank.RoleId);
                if (role == null)
                {
                    invalidRanks.Add(rank);
                }
                else
                {
                    var currentUser = await guild.GetCurrentUserAsync();
                    var hierarchy = (currentUser as SocketGuildUser).Hierarchy;
                    if (role.Position > hierarchy)
                    {
                        invalidRanks.Add(rank);
                    }
                    else
                    {
                        roles.Add(role);
                    }
                }
            }
            if (invalidRanks.Count > 0)
            {
                await ClearRankAsync(invalidRanks);
            }
            return roles;
        }
        public async Task<List<Rank>> GetRanksAsync(ulong id)
        {
            var ranks = await _context.Ranks
                .AsAsyncEnumerable()
                .Where(x => x.ServerId == id)
                .ToListAsync();

            return await Task.FromResult(ranks);
        }

        public async Task AddRankAsync(ulong id, ulong roleId)
        {
            var server = await _context.Servers
                .FindAsync(id);
            if (server == null)
            {
                _context.Add(new Server { Id = id });
            }
            _context.Add(new Rank { RoleId = roleId, ServerId = id });
            await _context.SaveChangesAsync();
        }
        public async Task RemoveRankAsync(ulong id, ulong roleId)
        {
            var rank = await _context.Ranks
                .AsAsyncEnumerable()
                .Where(x => x.RoleId == roleId)
                .FirstOrDefaultAsync();
            _context.Remove(rank);
            await _context.SaveChangesAsync();
        }

        public async Task ClearRankAsync(List<Rank> ranks)
        {
            _context.RemoveRange(ranks);
            await _context.SaveChangesAsync();
        }
    }
}
