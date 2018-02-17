using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using RazorEngine.Text;

namespace ATS.eFP.WebJob.Email.Application.Configuration
{
    public static class TemplateConfiguration
    {
        static TemplateConfiguration()
        {
            var config = new TemplateServiceConfiguration
            {
                ReferenceResolver = new CustomReferenceResolver(),
                EncodedStringFactory = new HtmlEncodedStringFactory(),
                TemplateManager = new DelegateTemplateManager()
            };

            Engine.Razor = RazorEngineService.Create(config);
        }

        public static void Initialize()
        {
            CompileTemplates();
        }

        public static void Initialize(string template)
        {
            CompileTemplates(template);
        }

        public static string RunTemplate<T>(T model, string template)
        {
            var templateKey = Engine.Razor.GetKey(template);
            if (!Engine.Razor.IsTemplateCached(templateKey, typeof(T)))
            {
                Initialize(template);
            }

            return Engine.Razor.Run(templateKey, typeof(T), model);
        }

        private static void CompileTemplates(string template = "")
        {
            var assembly = Assembly.GetExecutingAssembly();

            // validate full template namespace to avoid potential conflicts with other loaded MVC views at runtime
            var templateRegEx = new System.Text.RegularExpressions.Regex("^ATS.eFP.WebJob.Email.Application.Templates.*cshtml$");

            var resourceFiles = assembly.GetManifestResourceNames()
                                        .Where(resourceName => string.IsNullOrEmpty(template)
                                                            ? templateRegEx.IsMatch(resourceName)
                                                            : resourceName.Equals($"ATS.eFP.WebJob.Email.Application.Templates.{template}.cshtml"))
                                        .ToList();

            var templateCompileExceptions = new List<Exception>();

            foreach (var resourceFile in resourceFiles)
            {
                var templateNameParts = resourceFile.Split('.');

                if (templateNameParts.Length != 8)
                {
                    templateCompileExceptions.Add(new Exception($"Invalid template name attempted: {resourceFile}"));
                    continue;
                }

                var templateName = templateNameParts[templateNameParts.Length - 2];

                if (Engine.Razor.IsTemplateCached(templateName, Type.GetType(templateName)))
                    continue;

                var resourceStream = assembly.GetManifestResourceStream(resourceFile);

                if (resourceStream != null)
                {
                    using (var textReader = new StreamReader(resourceStream))
                    {
                        try
                        {
                            if (textReader.Peek() != -1)
                            {
                                var templateContent = textReader.ReadToEnd();

                                Engine.Razor.AddTemplate(templateName, templateContent);
                                Engine.Razor.Compile(templateName, null);
                            }
                        }
                        catch (Exception ex)
                        {
                            templateCompileExceptions.Add(new Exception($"Failed to initialize template named: {templateName}", ex));
                        }
                    }
                }
                else
                {
                    templateCompileExceptions.Add(new Exception("Failed to obtain template resource stream."));
                }
            }

            if (templateCompileExceptions.Any())
            {
                throw new AggregateException("Templates failed to compile.", templateCompileExceptions);
            }
        }
    }
}
