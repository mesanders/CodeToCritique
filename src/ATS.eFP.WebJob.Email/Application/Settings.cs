using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ATS.eFP.WebJob.Email.Application.Configuration;

namespace ATS.eFP.WebJob.Email.Application
{
    public static class Settings
    {
        public static EmailConfiguration EmailConfig { get; internal set; }
        public static WebJobAuthConfiguration WebJobAuthConfig { get; internal set; }
        public static LoggingConfiguration LoggingConfig { get; internal set; }

        public static void Initialize()
        {
            EmailConfig = ConfigurationManager.GetSection("EmailConfiguration") as EmailConfiguration;
            WebJobAuthConfig = ConfigurationManager.GetSection("WebJobAuthConfiguration") as WebJobAuthConfiguration;
            LoggingConfig = ConfigurationManager.GetSection("LoggingConfiguration") as LoggingConfiguration;
        }
    }
}
