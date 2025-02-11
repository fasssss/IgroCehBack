﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public interface IBaseEntity
    {
        public string Id { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
    }
}
