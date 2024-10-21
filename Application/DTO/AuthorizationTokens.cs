using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO
{
    public class AuthorizationTokens
    {
        public string AccessToken { get; set; }
        public string TokenType { get; set; }
        public int ExpiresIn { get; set; }
        public string RefreshToken { get; set; }
        public string Scope { get; set; }
    }
}
