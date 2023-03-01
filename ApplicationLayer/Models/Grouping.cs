using System.Collections.Generic;
using JetBrains.Annotations;

namespace Mesawer.ApplicationLayer.Models;

[PublicAPI]
public class Grouping<T>
{
    public int Key { get; set; }

    public List<T> Items { get; set; } = new();
}
