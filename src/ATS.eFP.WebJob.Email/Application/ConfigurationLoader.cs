using System.IO;
using NTextCat;

namespace ATS.eFP.WebJob.Email.Application
{
    public static class ConfigurationLoader
    {
        public static RankedLanguageIdentifier Identifier { get; internal set; }

        public static void Initialize()
        {
            var file = new FileInfo(@".\Application\Configuration\Core14.profile.xml");
            using (var readStream = File.Open(file.FullName, FileMode.Open))
            {
                var factory = new RankedLanguageIdentifierFactory();
                Identifier = factory.Load(readStream);
            }
        }
    }
}
