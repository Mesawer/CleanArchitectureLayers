using System.Collections.Generic;

namespace Mesawer.ApplicationLayer.Models;

public class EmailTemplate
{
    public string Head { get; set; }

    public string Content { get; set; }

    public string Url { get; set; }

    public Dictionary<string, object> ViewData { get; set; }
}
