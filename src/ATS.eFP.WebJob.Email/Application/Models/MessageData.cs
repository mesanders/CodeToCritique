using System;
using Newtonsoft.Json;

namespace ATS.eFP.WebJob.Email.Application.Models
{
    [Serializable]
    public class MessageData<T>
    {
        public MessageData()
        {

        }

        [JsonProperty("Entity")]
        public T Entity { get; set; }
    }
}
