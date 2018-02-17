using System.Globalization;
using System.Linq;
using ATS.eFP.WebJob.Email.Application;
using IvanAkcheurov.Commons;

namespace ATS.eFP.WebJob.Email.Services
{
    public class CultureService
    {
        public void SetCulture(string message, string localeCode)
        {
            string language = string.Empty;

            if (!localeCode.IsNullOrEmpty())
            {
                language = localeCode;
            }
            else
            {
                var languageIdentifier = ConfigurationLoader.Identifier.Identify(message);
                var mostCertainLanguage = languageIdentifier.FirstOrDefault();

                language = mostCertainLanguage.Item1.Iso639_3;

                language = !language.Contains("es") ? "en-US" : "es-MX";
            }

            CultureInfo ci = new CultureInfo(language);
            System.Threading.Thread.CurrentThread.CurrentUICulture = ci;
            System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(ci.Name);
        }
    }
}
