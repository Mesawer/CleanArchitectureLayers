using System;
using Mesawer.ApplicationLayer.Extensions;
using Mesawer.ApplicationLayer.Interfaces;

namespace Mesawer.InfrastructureLayer.Services;

public class DateTimeService : IDateTime
{
    public DateTime Now => DateTime.UtcNow;
    public DateTimeOffset EETNow => DateTime.UtcNow.ToDateTimeOffset(TimeSpan.FromHours(2));
}
