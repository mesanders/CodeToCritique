﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using ATS.eFP.Entities.Common;
using ATS.eFP.Entities.Product;
using ATS.eFP.Entities.Workorder;
using ATS.eFP.WebJob.Email.Application;
using ATS.eFP.WebJob.Email.Application.Configuration;
using ATS.eFP.WebJob.Email.Application.Localization;
using ATS.eFP.WebJob.Email.Application.Models;
using IvanAkcheurov.Commons;
using Twilio;

namespace ATS.eFP.WebJob.Email.Services
{
    public class MailService : IDisposable
    {
        public SmtpClient SmtpClient;

        public MailService()
        {
            SmtpClient = new SmtpClient("smtp.sendgrid.net", 587)
            {
                Credentials = new NetworkCredential
                {
                    UserName = Settings.EmailConfig.SendGridUserName,
                    Password = Settings.EmailConfig.SendGridPassword
                }
            };
        }

        public void CreateTwilioClient()
        {
            TwilioClient.Init("AC1091607564f3e4cbef1215b9873768ef", "1ef98c704d0a1d8fd96e073dd486f938");
        }

        private MailMessage ConstructHtmlMessage(string to)
        {
            return new MailMessage
            {
                To = { to },
                From = new MailAddress("noreply@advancedtech.com"),
                IsBodyHtml = true
            };
        }

        public string SmsSublocation(Workorder workorder, Product product)
        {
            var message = $"{DetermineEscalationCompleted(workorder)}-" +
                  $"{LocalizedText.WO}{workorder.Id}@{LocalizedText.Site}{workorder.Site.Name}-" +
                  $"{LocalizedText.Subloc}{product.Id ?? LocalizedText.NA }-" +
                  $"{LocalizedText.BldgLoc}{product.BuildingLocation ?? LocalizedText.NA}-" +
                  $"{LocalizedText.EquipDescr}{product.Description ?? LocalizedText.NA}-"+
                  $"{LocalizedText.EquipCrit}{product.CriticalityId}-" +
                  $"{LocalizedText.Notes}{GroupNotes(workorder) ?? LocalizedText.NA}";

            return TruncateMessage(message);
        }

        public string SmsEquipment(Workorder workorder, Product product)
        {
            var message = $"{DetermineEscalationCompleted(workorder)}-" +
                  $"{LocalizedText.WO}{workorder.Id}@{LocalizedText.Site}{workorder.Site.Name}-" +
                  $"{LocalizedText.AssetId}{product.AssetId}-" +
                  $"{LocalizedText.Subloc}{product.Id ?? LocalizedText.NA }-" +
                  $"{LocalizedText.BldgLoc}{product.BuildingLocation ?? LocalizedText.NA}-" +
                  $"{LocalizedText.EquipDescr}{product.Description ?? LocalizedText.NA}-" +
                  $"{LocalizedText.OpStatus}{product.OperatingStatusId ?? LocalizedText.NA}-" +
                  $"{LocalizedText.EquipCrit}{product.CriticalityId}-" +
                  $"{LocalizedText.Notes}{GroupNotes(workorder) ?? LocalizedText.NA}";

            return TruncateMessage(message);
        }

        public MailMessage WorkorderMail(Workorder workorder, Product product, string recipient, string templateKey, string status)
        {
            workorder.Status = LocalizeWorkorderStatus(workorder.Status);
            product.OperatingStatusId = LocalizeEquipmentStatus(product.OperatingStatusId);
            var wrapper = new WorkorderNotificationWrapper
            {
                Workorder = workorder,
                Product = product,
                FooterButton = Settings.EmailConfig.FooterButton,
                HeaderHeadset = Settings.EmailConfig.HeaderHeadset,
                HeaderLogo = Settings.EmailConfig.HeaderLogo,
                PortalLink = Settings.EmailConfig.PortalLink
            };

            foreach (var task in workorder.Tasks)
            {
                foreach (var note in task.Notes)
                {
                    wrapper.NotesAndRemarks.Add(note.Text);
                }
            }

            if (wrapper.NotesAndRemarks.Count == 0)
            {
                wrapper.NotesAndRemarks.Add(LocalizedText.NA);
            }

            if (status == "COMPLETE")
            {
                wrapper.Subject = string.Format(LocalizedText.SubjectCompleted, workorder.Id);
                wrapper.SubjectSub = string.Format(LocalizedText.WorkOrderCompleted, workorder.Id);
            }
            else
            {
                wrapper.Subject = string.Format(LocalizedText.SubjectCreated, workorder.Id);
                wrapper.SubjectSub = string.Format(LocalizedText.WorkOrderCreated, workorder.Id);
            }

            return ConstrutMail(wrapper, recipient, wrapper.Subject, templateKey);
        }
        public MailMessage EscalationMail(Workorder workorder, Product product, EventMonitor eventMonitor, TimeZones timeZoneId, string receipient, string templateKey)
        {
            workorder.Status = LocalizeWorkorderStatus(workorder.Status);
            product.OperatingStatusId = LocalizeEquipmentStatus(product.OperatingStatusId);
            var wrapper = new EscalationWrapper
            {
                Workorder = workorder,
                Product = product,
                EventMonitor = eventMonitor,
                WorkorderCreated = LocalTimeFormat(timeZoneId, (DateTime)workorder.Created),
                EscalatedAt = LocalTimeFormat(timeZoneId, DateTime.Now.ToUniversalTime()),
                AssignedTech = workorder.Tasks?.FirstOrDefault()?.AssignedPerson?.FullName,
                TaskTemplateId = workorder.Tasks?.FirstOrDefault()?.TaskTemplateId,
                Subject = $"{product.AssetId} @ {product.Site?.Name} {DetermineEscalationCompleted(workorder)}",
                SubjectHeader = DetermineEscalationCompleted(workorder),
                SubjectSub = product.Group?.Name == "SUBLOCATION" ? LocalizedText.NoEquip : $"{product.Id} @ {workorder.Site.Name}",
                FooterButton = Settings.EmailConfig.FooterButton,
                HeaderHeadset = Settings.EmailConfig.HeaderHeadset,
                HeaderLogo = Settings.EmailConfig.HeaderLogo,
                PortalLink = Settings.EmailConfig.PortalLink
            };

            foreach (var task in workorder.Tasks)
            {
                foreach (var note in task.Notes)
                {
                    wrapper.NotesAndRemarks.Add(note.Text);
                }
            }

            if (wrapper.NotesAndRemarks.Count == 0)
            {
                wrapper.NotesAndRemarks.Add(LocalizedText.NoRepairRemarks);
            }

            return ConstrutMail(wrapper, receipient, wrapper.Subject, templateKey);
        }

        private MailMessage ConstrutMail<T>(T model, string receipient, string subject, string templateKey)
        {
            SanitizeObject(model);
            var mail = ConstructHtmlMessage(receipient);
            mail.Subject = subject;
            mail.Body = TemplateConfiguration.RunTemplate(model, templateKey);

            return mail;
        }

        private string TruncateMessage(string message)
        {
            var compiledString = new StringBuilder();
            if (message.Length <= 160)
            {
                return message;
            }

            var split = message.Split('-');
            var stringCollection = string.IsNullOrWhiteSpace(split.LastOrDefault()) ?
                split.Take(split.Count() - 2).ToStrings() : split.Take(split.Count() - 1).ToStrings();
            foreach (var item in stringCollection)
            {
                if (compiledString.Length <= 160)
                {
                    if (stringCollection.LastOrDefault() != item)
                    {
                        compiledString.Append(item);
                        compiledString.Append('-');
                    }
                    else
                    {
                        compiledString.Append(item);
                    }                   
                }
            }

            return TruncateMessage(compiledString.ToString());
        }

        private string GroupNotes(Workorder workorder)
        {
            string notes = string.Empty;
            foreach (var task in workorder.Tasks.OrderByDescending(x => x.Id))
            {
                foreach (var note in task.Notes.OrderByDescending(x => x.Id))
                {
                    if (!string.IsNullOrEmpty(note.Text))
                    {
                        notes += note.Text;
                        notes += "|";
                    }
                }
            }
            return notes;
        }

        private string LocalTimeFormat(TimeZones workorderTimeZone, DateTime utcTime)
        {
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(workorderTimeZone.InfoId);
            DateTime userTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, timeZoneInfo);

            switch (workorderTimeZone.InfoId)
            {
                case "Central Standard Time":
                    return $"{userTime} {LocalizedText.CentralTime}";

                case "Eastern Standard Time":
                    return $"{userTime} {LocalizedText.EasternTime}";

                case "Pacific Standard Time":
                    return $"{userTime} {LocalizedText.PacificTime}";

                case "Mountain Standard Time":
                    return $"{userTime} {LocalizedText.MountainTime}";
            }

            return $"{userTime} {workorderTimeZone.InfoId}";
        }

        private string LocalizeEquipmentStatus(string equipmentStatus)
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

        private string LocalizeWorkorderStatus(string workorderStatus)
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

        private string DetermineEscalationCompleted(Workorder workorder) => 
            workorder.Status == "COMPLETE" ? LocalizedText.EscalationCompleted : LocalizedText.EscalationOpen;
        

        private void SanitizeObject(object root)
        {
            foreach (PropertyInfo property in root.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(prop => !prop.PropertyType.IsValueType))
            {
                if (property.PropertyType == typeof(string) && property.GetValue(root) == null)
                {
                    property.SetValue(root, LocalizedText.NA);
                }
                else if (property.PropertyType == typeof(string) && property.GetValue(root) != null)
                {
                }
                else
                {
                    var properties = property.DeclaringType.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);

                    if (properties != null && properties.Length > 0 && (property.PropertyType.AssemblyQualifiedName.Contains("System.Collections.ObjectModel") ||
                        property.PropertyType.AssemblyQualifiedName.Contains("System.Collections.Generic.List")))
                    {
                        var collectionProperties = property.GetValue(root) as IEnumerable<object>;
                        foreach (var item in collectionProperties)
                        {
                            SanitizeObject(item);
                        }
                    }
                    else
                    {
                        var value = property.GetValue(root);
                        if (value != null)
                        {
                            SanitizeObject(value);
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
            SmtpClient?.Dispose();
        }
    }
}
