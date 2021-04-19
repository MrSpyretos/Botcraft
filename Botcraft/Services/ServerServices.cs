using Discord;
using System.Threading.Tasks;
using Botcraft.Models;
using Botcraft;
using System.Linq;
using Botcraft.Database.Entities;
using Botcraft.Common;

namespace Botcraft.Services
{

    public class ServerServices
    {
        private readonly DiscbotContext _context;
        public ServerServices( DiscbotContext context)
        {
            _context = context; 
        }
        public async Task SendLogAsync(IGuild guild , string title , string description)
        {
           var channelId= await GetLogsAsync(guild.Id);
            if (channelId == 0)
                return;
            var fetchedChannel = await guild.GetTextChannelAsync(channelId);
            if(fetchedChannel == null)
            {
                await ClearLogsAsync(guild.Id);
                return;
            }
            await fetchedChannel.SendLogAsync(title, description);
        }
        public async Task ModifyGuildPrefix(ulong id, string prefix)
        {
            var server = await _context.Servers
                .FindAsync(id);

            if (server == null)
            {
                _context.Add(new Server { Id = id, Prefix = prefix });
            }
            else
            {
                server.Prefix = prefix;
            }
            await _context.SaveChangesAsync();
        }
        public async Task<string> GetGuildPrefix(ulong id)
        {
            var prefix = await _context.Servers
                .AsAsyncEnumerable()
                .Where(x => x.Id == id)
                .Select(x => x.Prefix)
                .FirstOrDefaultAsync();

            return await Task.FromResult(prefix);
        }
        public async Task ModifyWelcomeAsync(ulong id, ulong channelId)
        {
            var server = await _context.Servers
                .FindAsync(id);

            if (server == null)
            {
                _context.Add(new Server { Id = id, Welcome = channelId });
            }
            else
            {
                server.Welcome = channelId;
            }
            await _context.SaveChangesAsync();
        }
        public async Task ClearWelcomeAsync(ulong id)
        {
            var server = await _context.Servers
                .FindAsync(id);

            server.Welcome = 0;
            await _context.SaveChangesAsync();
        }
        public async Task<ulong> GetWelcomeAsync(ulong id)
        {
            var server = await _context.Servers
                .FindAsync(id);
            if (server == null)
            {
                _context.Add(new Server { Id = id });
                server = await _context.Servers
                .FindAsync(id);
            }
            return await Task.FromResult(server.Welcome);
        }
        public async Task ModifyBackgroundAsync(ulong id, string url)
        {
            var server = await _context.Servers
                .FindAsync(id);

            if (server == null)
            {
                _context.Add(new Server { Id = id, Background = url });
            }
            else
            {
                server.Background = url;
            }
            await _context.SaveChangesAsync();
        }
        public async Task ClearBackgroundAsync(ulong id)
        {
            var server = await _context.Servers
                .FindAsync(id);

            server.Background = null;
            await _context.SaveChangesAsync();
        }
        public async Task<string> GetBackgroundAsync(ulong id)
        {
            var server = await _context.Servers
                .FindAsync(id);

            return await Task.FromResult(server.Background);
        }

        public async Task ModifyLogsAsync(ulong id, ulong channelId)
        {
            var server = await _context.Servers
                .FindAsync(id);

            if (server == null)
            {
                _context.Add(new Server { Id = id, Logs = channelId });
            }
            else
            {
                server.Logs = channelId;
            }
            await _context.SaveChangesAsync();
        }
        public async Task ClearLogsAsync(ulong id)
        {
            var server = await _context.Servers
                .FindAsync(id);

            server.Logs = 0;
            await _context.SaveChangesAsync();
        }
        public async Task<ulong> GetLogsAsync(ulong id)
        {
            var server = await _context.Servers
                .FindAsync(id);
            if (server == null)
            {
                _context.Add(new Server { Id = id });
                server = await _context.Servers
                .FindAsync(id);
            }
            return await Task.FromResult(server.Logs);
        }
    }
}
