using System;
using System.Diagnostics;
using ATS.eFP.WebJob.Email.Application;
using ATS.eFP.WebJob.Email.Application.Configuration;
using ATS.eFP.WebJob.Email.Application.Dependencies;
using Autofac;
using Microsoft.Azure.WebJobs;
using Serilog;
using Serilog.Core;

namespace ATS.eFP.WebJob.Email
{
    class Program
    {
        static void Main()
        {
            Settings.Initialize();
            NewRelicConfiguration.Initialize();

            var levelSwitch = new LoggingLevelSwitch();
            var logger = new LoggerConfiguration().MinimumLevel.ControlledBy(levelSwitch).WriteTo.Logentries(Settings.LoggingConfig.LogentriesKey).CreateLogger();

            Log.Logger = logger;

            Console.WriteLine("Starting email webjob");
            Log.Information("Starting email webjob");

            ConfigurationLoader.Initialize();
            TemplateConfiguration.Initialize();
            var container = new ContainerBuilder();
            WebJobRegistration.Initialize(container);

            var config = new JobHostConfiguration
            {
                JobActivator = new WebJobActivator(container.Build()),
                Tracing = { ConsoleLevel = TraceLevel.Info }
            };

            config.UseServiceBus();

            var host = new JobHost(config);

            host.RunAndBlock();

            Console.WriteLine("Successfully started email webjob");
        }
    }
}
