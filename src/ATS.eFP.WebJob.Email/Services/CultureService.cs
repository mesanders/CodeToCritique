using System;
using System.Globalization;
using System.Linq;
using ATS.eFP.Entities.Common;
using ATS.eFP.Entities.Workorder;
using ATS.eFP.WebJob.Email.Application;
using ATS.eFP.WebJob.Email.Application.Localization;
using IvanAkcheurov.Commons;

namespace ATS.eFP.WebJob.Email.Services
{
    public class CultureService
    {
        public CultureService()
        {
            
        }

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

        public DateTime LocalTimeConversion(TimeZones workorderTimeZone, DateTime utcTime)
        {
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(workorderTimeZone.InfoId);
            return TimeZoneInfo.ConvertTimeFromUtc(utcTime, timeZoneInfo);
        }

        public string LocalizedTimeZone(TimeZones workorderTimeZone)
        {
            switch (workorderTimeZone.InfoId)
            {
                case "Central Standard Time":
                    return LocalizedText.CentralTime;

                case "Eastern Standard Time":
                    return LocalizedText.EasternTime;

                case "Pacific Standard Time":
                    return LocalizedText.PacificTime;

                case "Mountain Standard Time":
                    return LocalizedText.MountainTime;
            }

            return workorderTimeZone.InfoId;
        }

        public string LocalizeEquipmentStatus(string equipmentStatus)
        {
            switch (equipmentStatus)
            {
                case "DECOMMISSIONED":
                    return LocalizedText.EquipStatusDecom;
                case "DOWN":
                    return LocalizedText.EquipStatusDown;
                case "IDLE":
                    return LocalizedText.EquipStatusIdle;
                case "REDUCED":
                    return LocalizedText.EquipStatusReduced;
                case "SCRAPPED":
                    return LocalizedText.EquipStatusScrapped;
                case "UP":
                    return LocalizedText.EquipStatusUp;
            }

            return equipmentStatus;
        }

        public string LocalizeWorkorderStatus(string workorderStatus)
        {
            switch (workorderStatus)
            {
                case "CANCELED":
                    return LocalizedText.WOCancel;
                case "CLOSED":
                    return LocalizedText.WOClosed;
                case "COMPLETE":
                    return LocalizedText.WOComplete;
                case "DISCREPANCY":
                    return LocalizedText.WODiscrep;
                case "HOLD":
                    return LocalizedText.WOHold;
                case "INVESTIGATE":
                    return LocalizedText.WOInvestigate;
                case "OPEN":
                    return LocalizedText.WOOpen;
                case "QUOTE":
                    return LocalizedText.WOQuote;
            }

            return workorderStatus;
        }

        public string DetermineEscalationCompleted(Workorder workorder) =>
            workorder.Status == "COMPLETE" ? LocalizedText.EscalationCompleted : LocalizedText.EscalationOpen;
    }
}
