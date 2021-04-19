using System;
using System.Collections.Generic;
using System.Text;

namespace Botcraft.Database.Entities
{
    public class Server
    {
        public ulong Id { get; set; }
        public string Prefix { get; set; }
        public ulong Welcome { get; set; }
        public string Background { get; set; }
        public ulong Logs { get; set; }
    }
}
