using ATS.eFP.WebJob.Email.Services;
using Autofac;

namespace ATS.eFP.WebJob.Email.Application.Dependencies
{
    public static class WebJobRegistration
    {
        public static void Initialize(ContainerBuilder builder)
        {
            builder.RegisterType<ApiService>().InstancePerLifetimeScope();
            builder.RegisterType<CultureService>().InstancePerLifetimeScope();
            builder.RegisterType<Functions>().InstancePerLifetimeScope();
        }
    }
}
