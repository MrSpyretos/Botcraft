using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Botcraft.Database.Entities
{
    public partial class WordList
    {
        [Key]
        public long Id { get; set; }
        public long ServerId { get; set; }
        public string ServerName { get; set; }
        public string Word { get; set; }
        public long SetById { get; set; }
    }
}
