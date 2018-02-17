using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RazorEngine.Compilation;
using RazorEngine.Compilation.ReferenceResolver;

namespace ATS.eFP.WebJob.Email.Application.Configuration
{
    public class CustomReferenceResolver : IReferenceResolver
    {
        public string FindLoaded(IEnumerable<string> refs, string find)
        {
            return refs.First(r => r.EndsWith(System.IO.Path.DirectorySeparatorChar + find));
        }

        public IEnumerable<CompilerReference> GetReferences(TypeContext context, IEnumerable<CompilerReference> includeAssemblies = null)
        {
            // If you include mscorlib here the compiler is called with /nostdlib.
            IEnumerable<string> loadedAssemblies = new UseCurrentAssembliesReferenceResolver()
                .GetReferences(context, includeAssemblies)
                .Select(r => r.GetFile())
                .ToArray();

            foreach (var assm in loadedAssemblies)
            {
                yield return CompilerReference.From(assm);
            }

            // Pre-load template models
            Assembly.Load(typeof(ATS.eFP.Entities.Workorder.Workorder).Assembly.GetName());
            Assembly.Load(typeof(ATS.eFP.Entities.Product.Product).Assembly.GetName());
            Assembly.Load(typeof(ATS.eFP.WebJob.Email.Application.Models.WorkorderNotificationWrapper).Assembly.GetName());
        }
    }
}
