using System;
using Mesawer.ApplicationLayer.Interfaces;

namespace Mesawer.InfrastructureLayer.Services
{
    public class DateTimeService : IDateTime
    {
        public DateTime Now => DateTime.UtcNow;
    }
}
