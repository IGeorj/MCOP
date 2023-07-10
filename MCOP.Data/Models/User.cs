﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCOP.Data.Models
{
    public class User
    {
        [Key]
        public ulong Id { get; set; }

        public ICollection<GuildUser> GuildUsers { get; set; }
    }
}
