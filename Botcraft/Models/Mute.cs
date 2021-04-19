using Discord;
using Discord.WebSocket;
using System;

namespace Botcraft.Models
{
    public class Mute
    {
        public SocketGuild Guild;
        public SocketGuildUser User;
        public IRole Role;
        public DateTime End;
    }
}
