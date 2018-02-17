using System.Configuration;

namespace ATS.eFP.WebJob.Email.Application.Configuration
{
    public class EmailConfiguration : ConfigurationSection
    {
        [ConfigurationProperty("SendGridUserName", IsRequired = true)]
        public string SendGridUserName
        {
            get { return this["SendGridUserName"].ToString(); }
            set { this["SendGridUserName"] = value; }
        }

        [ConfigurationProperty("SendGridPassword", IsRequired = true)]
        public string SendGridPassword
        {
            get { return this["SendGridPassword"].ToString(); }
            set { this["SendGridPassword"] = value; }
        }

        [ConfigurationProperty("HeaderLogo", IsRequired = true)]
        public string HeaderLogo
        {
            get { return this["HeaderLogo"].ToString(); }
            set { this["HeaderLogo"] = value; }
        }

        [ConfigurationProperty("HeaderHeadset", IsRequired = true)]
        public string HeaderHeadset
        {
            get { return this["HeaderHeadset"].ToString(); }
            set { this["HeaderHeadset"] = value; }
        }

        [ConfigurationProperty("FooterButton", IsRequired = true)]
        public string FooterButton
        {
            get { return this["FooterButton"].ToString(); }
            set { this["FooterButton"] = value; }
        }

        [ConfigurationProperty("PortalLink", IsRequired = true)]
        public string PortalLink
        {
            get { return this["PortalLink"].ToString(); }
            set { this["PortalLink"] = value; }
        }
    }
}
