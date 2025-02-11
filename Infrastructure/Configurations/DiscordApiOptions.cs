﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Configurations
{
    public class DiscordApiOptions
    {
        public string Address { get; set; }
        public string ImagesBaseUrl { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string RedirectUri { get; set; }
        public int RefreshTokenLiveDays { get; set; }
    }
}
