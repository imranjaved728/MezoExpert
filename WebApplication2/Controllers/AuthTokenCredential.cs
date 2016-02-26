using System.Collections.Generic;
using PayPal.Api;

namespace WebApplication2.Controllers
{
    internal class AuthTokenCredential : OAuthTokenCredential
    {
        public AuthTokenCredential(string clientId = "", string clientSecret = "", Dictionary<string, string> config = null) : base(clientId, clientSecret, config)
        {
        }
    }
}