using Autofac;
using Microsoft.Azure.WebJobs.Host;

namespace ATS.eFP.WebJob.Email.Application.Dependencies
{
    public class WebJobActivator : IJobActivator
    {
        private readonly IContainer _container;
        public WebJobActivator(IContainer container)
        {
            _container = container;
        }

        public T CreateInstance<T>()
        {
            return _container.Resolve<T>();
        }
    }
}
