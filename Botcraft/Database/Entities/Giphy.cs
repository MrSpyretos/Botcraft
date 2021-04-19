﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Botcraft.Database.Entities
{
    public partial class Giphy
    {
        [Key]
        public long Id { get; set; }
        public string ServerName { get; set; }
        public Nullable<long> ServerId { get; set; }
        public Nullable<bool> GiphyEnabled { get; set; }
    }
}
