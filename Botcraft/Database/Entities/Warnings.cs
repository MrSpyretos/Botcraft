using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Botcraft.Database.Entities
{
    public partial class Warnings
    {
        [Key]
        public long Id { get; set; }
        public long ServerId { get; set; }
        public string ServerName { get; set; }
        public long UserWarnedId { get; set; }
        public string UserWarnedName { get; set; }
        public long IssuerId { get; set; }
        public string IssuerName { get; set; }
        public Nullable<System.DateTime> TimeIssued { get; set; }
        public int NumWarnings { get; set; }
    }
}
