using Newtonsoft.Json;

namespace Mesawer.InfrastructureLayer.Models
{
    public class FacebookResponse
    {
        public string Id { get; set; }

        public string Email { get; set; }

        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }

        public PictureObject Picture { get; set; }

        public class PictureObject
        {
            public DataObject Data { get; set; }

            public class DataObject
            {
                public string Url { get; set; }
            }
        }
    }
}
