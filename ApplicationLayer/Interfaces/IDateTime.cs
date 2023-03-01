using System;

namespace Mesawer.ApplicationLayer.Interfaces;

public interface IDateTime
{
    DateTime Now { get; }

    // ReSharper disable once InconsistentNaming
    DateTimeOffset EETNow { get; }
}
