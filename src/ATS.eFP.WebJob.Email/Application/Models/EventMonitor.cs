using System;
using Newtonsoft.Json;

namespace ATS.eFP.WebJob.Email.Application.Models
{
    [Serializable]
    public class EventMonitor
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }
}
