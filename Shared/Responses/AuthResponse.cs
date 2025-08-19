using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Responses
{
    public class AuthResponse
    {

        public string Token { get; set; }
        public DateTime Expiration { get; set; }
        public UserInfo UserInfo { get; set; }

    }
}
