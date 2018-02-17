using System.IO;
using System.Runtime.Serialization.Json;
using ATS.eFP.Entities.Workorder;
using ATS.eFP.WebJob.Email.Application.Models;
using FluentAssertions;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace ATS.eFP.WebJob.Email.Tests
{
    public class FunctionTests
    {
        public FunctionTests()
        {

        }

        [Fact]
        public void CanDeserializeToObjects()
        {
            var sut = new MessageData<object>
            {
                Entity = new
                {
                    Workorder = new Workorder
                    {
                        Id = "123"
                    },
                    To = "3092026577"
                }
            };

            var json = JsonConvert.SerializeObject(sut);

            var message = new BrokeredMessage(json, new DataContractJsonSerializer(typeof(MessageData<object>), new[] { typeof(Workorder), typeof(string) }));

            Stream stream = message.GetBody<Stream>();
            StreamReader reader = new StreamReader(stream);
            string messageData = reader.ReadToEnd();

            var deserializedObject = JsonConvert.DeserializeObject<string>(messageData);
            var parsed = JObject.Parse(deserializedObject);
            var to = parsed["Entity"]["To"].ToObject<string>();
            var workorder = parsed["Entity"]["Workorder"].ToObject<Workorder>();
            to.Should().NotBeNull();
            workorder.Should().NotBeNull();
        }
    }
}
