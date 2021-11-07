using System;

namespace Domain.Models.Auth
{
    public class JwtConfig
    {
        public string Secret { get; set; }
        public TimeSpan TokenLifetime { get; set; }
    }
}