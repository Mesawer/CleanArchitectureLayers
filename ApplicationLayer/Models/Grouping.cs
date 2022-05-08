using System.Collections.Generic;

namespace Mesawer.ApplicationLayer.Models;

public class Grouping<T>
{
    public int Key { get; set; }

    public List<T> Items { get; set; }
}
