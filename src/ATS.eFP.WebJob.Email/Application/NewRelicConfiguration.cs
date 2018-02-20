namespace ATS.eFP.WebJob.Email.Application
{
    public static class NewRelicConfiguration
    {
        public static void Initialize()
        {
            NewRelic.Api.Agent.NewRelic.SetApplicationName(Settings.NewRelicConfig);
            NewRelic.Api.Agent.NewRelic.StartAgent();
        }
    }
}
