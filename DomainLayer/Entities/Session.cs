using System;

namespace Mesawer.DomainLayer.Entities
{
    public class Session
    {
        public string Id { get; set; }

        public string Token { get; set; }

        public string RefreshToken { get; set; }

        public string MacAddress { get; set; }

        public DateTimeOffset LastLogin { get; set; }
    }
}
