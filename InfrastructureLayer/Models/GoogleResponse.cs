using Newtonsoft.Json;

namespace Mesawer.InfrastructureLayer.Models;

public class GoogleResponse
{
    [JsonProperty("sub")]
    public string Id { get; set; }

    public string Email { get; set; }

    [JsonProperty("given_name")]
    public string FirstName { get; set; }

    [JsonProperty("family_name")]
    public string LastName { get; set; }

    public string Picture { get; set; }
}
