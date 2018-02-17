using System.Configuration;

namespace ATS.eFP.WebJob.Email.Application.Configuration
{
    public class LoggingConfiguration : ConfigurationSection
    {
        [ConfigurationProperty("LogentriesKey", IsRequired = true)]
        public string LogentriesKey
        {
            get { return this["LogentriesKey"].ToString(); }
            set { this["LogentriesKey"] = value; }
        }
    }
}
