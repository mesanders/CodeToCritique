using System;
using System.Globalization;
using System.IO;
using System.Linq;
using ATS.eFP.WebJob.Email.Application.Localization;
using FluentAssertions;
using NTextCat;
using Xunit;

namespace ATS.eFP.WebJob.Email.Tests
{
    public class LanguageTests
    {
        private RankedLanguageIdentifier _identifier;

        [Fact]
        public void RetriveLanguageDataFromString()
        {
            var spanishMessage = "como esta por que";
            var englishMessage = "why are we doing this";

            var file = new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Configuration\Core14.profile.xml"));

            using (var readStream = File.Open(file.FullName, FileMode.Open))
            {
                var sut = new RankedLanguageIdentifierFactory();
                _identifier = sut.Load(readStream);
            }

            var spanishLanguageIdentifier = _identifier.Identify(spanishMessage);
            var englishLanguageIdentifier = _identifier.Identify(englishMessage);
            var mostCertainSpaLanguage = spanishLanguageIdentifier.FirstOrDefault();
            var mostCertainEngLanguage = englishLanguageIdentifier.FirstOrDefault();

            var theSpaLanguage = mostCertainSpaLanguage.Item1.Iso639_3;
            var theEngLanguage = mostCertainEngLanguage.Item1.Iso639_3;

            theSpaLanguage.Should().Be("es-MX");
            theEngLanguage.Should().Be("en-US");
        }

        [Fact]
        public void ChangeCurrentCulture()
        {
            var spanishMessage = "No stock disponible en la maquina por favor";

            var file = new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Configuration\Core14.profile.xml"));

            using (var readStream = File.Open(file.FullName, FileMode.Open))
            {
                var sut = new RankedLanguageIdentifierFactory();
                _identifier = sut.Load(readStream);
            }

            var spanishLanguageIdentifier = _identifier.Identify(spanishMessage);
            var mostCertainSpaLanguage = spanishLanguageIdentifier.FirstOrDefault();
            var theSpaLanguage = mostCertainSpaLanguage.Item1.Iso639_3;

            CultureInfo ci = new CultureInfo(theSpaLanguage);
            System.Threading.Thread.CurrentThread.CurrentUICulture = ci;
            System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(ci.Name);
            System.Threading.Thread.CurrentThread.CurrentCulture.Name.Should().Be("es-MX", ci.Name);
        }

        [Fact]
        public void WillUseSpanishLocalizationGivenCurrentThreadUICulture()
        {
            CultureInfo ci = new CultureInfo("es-MX");
            System.Threading.Thread.CurrentThread.CurrentUICulture = ci;
            System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(ci.Name);

            var text = LocalizedText.DescriptionIssue;

            text.Should().NotBeNullOrEmpty();
        }
    }
}
