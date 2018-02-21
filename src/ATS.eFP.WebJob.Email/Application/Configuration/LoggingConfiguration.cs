using System.Configuration;

namespace ATS.eFP.WebJob.Email.Application.Configuration
{
    public class LoggingConfiguration : ConfigurationSection
    {
        [ConfigurationProperty("LogEntriesKey", IsRequired = true)]
        public string LogentriesKey
        {
            get { return this["LogEntriesKey"].ToString(); }
            set { this["LogEntriesKey"] = value; }
        }
    }
}
