using System;
using System.Collections.Generic;
using System.Text;

namespace Botcraft.Database.Entities
{
    public class AutoRole
    {
        public int Id { get; set; }
        public ulong RoleId { get; set; }
        public ulong ServerId { get; set; }
    }
}
