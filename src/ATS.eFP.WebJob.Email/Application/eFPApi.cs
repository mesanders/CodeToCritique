using System;
using ATS.eFP.API.Client;

namespace ATS.eFP.WebJob.Email.Application
{
    public static class eFPApi
    {
        public static IApiODataClient ODataApiClient;

        static eFPApi()
        {
            var connectionManager = ApiConnectionManagerFactory.Configure(new Uri(Settings.WebJobAuthConfig.TokenAuthority),
                Settings.WebJobAuthConfig.ClientId,
                Settings.WebJobAuthConfig.ClientSecret,
                Settings.WebJobAuthConfig.ServiceAccountName,
                Settings.WebJobAuthConfig.ServiceAccountPassword, "openid profile");

            ODataApiClient = connectionManager.CreateODataClient(new Uri(Settings.WebJobAuthConfig.ODataApiUri));
        }
    }
}
