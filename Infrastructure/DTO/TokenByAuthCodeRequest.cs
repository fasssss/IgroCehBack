using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.DTO
{
    public class TokenByAuthCodeRequest
    {
        public string grant_type = "authorization_code";
        public string code;
        public string redirect_uri;
    }
}
