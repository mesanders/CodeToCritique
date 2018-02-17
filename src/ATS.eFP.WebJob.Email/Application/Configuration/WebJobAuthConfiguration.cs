using System.Configuration;

namespace ATS.eFP.WebJob.Email.Application.Configuration
{
    public class WebJobAuthConfiguration : ConfigurationSection
    {
        [ConfigurationProperty("ClientId", IsRequired = true)]
        public string ClientId
        {
            get { return this["ClientId"].ToString(); }
            set { this["ClientId"] = value; }
        }

        [ConfigurationProperty("ClientSecret", IsRequired = true)]
        public string ClientSecret
        {
            get { return this["ClientSecret"].ToString(); }
            set { this["ClientSecret"] = value; }
        }

        [ConfigurationProperty("ServiceAccountName", IsRequired = true)]
        public string ServiceAccountName
        {
            get { return this["ServiceAccountName"].ToString(); }
            set { this["ServiceAccountName"] = value; }
        }

        [ConfigurationProperty("ServiceAccountPassword", IsRequired = true)]
        public string ServiceAccountPassword
        {
            get { return this["ServiceAccountPassword"].ToString(); }
            set { this["ServiceAccountPassword"] = value; }
        }

        [ConfigurationProperty("TokenAuthority", IsRequired = true)]
        public string TokenAuthority
        {
            get { return this["TokenAuthority"].ToString(); }
            set { this["TokenAuthority"] = value; }
        }

        [ConfigurationProperty("ODataApiUri", IsRequired = true)]
        public string ODataApiUri
        {
            get { return this["ODataApiUri"].ToString(); }
            set { this["ODataApiUri"] = value; }
        }

    }
}
