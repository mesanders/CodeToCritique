using System;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using ATS.eFP.Entities.Common;
using ATS.eFP.Entities.Product;
using ATS.eFP.Entities.Workorder;
using ATS.eFP.WebJob.Email.Application.Models;
using ATS.eFP.WebJob.Email.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.ServiceBus.Messaging;
using NewRelic.Api.Agent;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace ATS.eFP.WebJob.Email
{
    public class Functions
    {
        private CultureService _cultureService;
        private MailService _mailService;
        private ApiService _apiService;
        public Functions(CultureService cultureService, MailService mailService, ApiService apiService)
        {
            _cultureService = cultureService;
            _mailService = mailService;
            _apiService = apiService;
        }

        //todo: this handler is technically turned off
        public async Task OnWorkOrderCreate([ServiceBusTrigger("efp-api-create_workorder-topic", "efp-api-wocreate-sub-send-email")]BrokeredMessage message,
            TraceWriter trace)
        {
            message.Complete();
            try
            {
                JObject deserializedObject = DeserializeBrokeredMessage<MessageData<object>>(message).Entity as JObject;
                Workorder deserializedWorkorder = deserializedObject.ToObject<Workorder>();
                Workorder workorder = await _apiService.WorkorderData(deserializedWorkorder.Id);

                if (deserializedWorkorder.Tasks?.FirstOrDefault()?.Notes.Count > 0)
                {
                    workorder.Tasks.FirstOrDefault().Notes = deserializedWorkorder.Tasks.FirstOrDefault().Notes;
                }

                if (workorder.SiteId == null)
                {
                    LogData(trace, $"No SiteId provided for Workorder: {workorder.Id} sending to {workorder.Contacts.FirstOrDefault().Email ?? "no address"}", false);
                }
                else
                {
                    var product = await _apiService.ProductData(workorder.Tasks.FirstOrDefault().ProductId);

                    if (product == null)
                    {
                        LogData(trace, $"Product in database does not exist problem. Product id: {workorder.Tasks.FirstOrDefault().ProductId}", false);
                    }
                    else
                    {
                        var templateKey = product.Group.Name == "SUBLOCATION" ? "TemplateWorkorderSublocation" : "TemplateWorkorderEquipment";

                        _cultureService.SetCulture(workorder.CustomerProblemDescription.ToLower(), workorder.Site.LocaleCode.ToLower());

                        foreach (var contact in workorder.Contacts)
                        {
                            if (contact.Email != null)
                            {
                                LogData(trace, $"Sending message to {contact.Email} regarding Workorder: {workorder.Id}", false);

                                var mail = _mailService.WorkorderMail(workorder, product, contact.Email, templateKey, workorder.Status);
                                _mailService.SmtpClient.SendAsync(mail, null);

                                LogData(trace, $"Sent message to {contact.Email} regarding Workorder: {workorder.Id}", false);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogData(trace, $"Message:{ex.Message} InnerException:{ex.InnerException} StackTrace:{ex.StackTrace}", false);
            }

            LogData(trace, "Workorder Create Sending Finished", false);
        }

        [Transaction(Web = true)]
        public async Task OnWorkOrderUpdate([ServiceBusTrigger("efp-api-update_workorder-topic", "efp-api-woupdate-sub-send-email")]BrokeredMessage message,
           TraceWriter trace)
        {
            message.Complete();
            try
            {
                JObject deserializedObject = DeserializeBrokeredMessage<MessageData<object>>(message).Entity as JObject;
                Workorder deserializedWorkorder = deserializedObject.ToObject<Workorder>();
                Workorder workorder = _apiService.WorkorderData(deserializedWorkorder.Id).Result;
                var taskData = workorder == null ? _apiService.TaskData(Convert.ToDecimal(deserializedWorkorder.Id)).Result : _apiService.TaskData(workorder.Tasks.FirstOrDefault().Id).Result;
                workorder = _apiService.WorkorderData(taskData.WorkorderId).Result;

                bool allTasksComplete = workorder.Tasks.All(task => task.TaskStatusId == "COMPLETED");

                if (allTasksComplete)
                {
                    if (workorder.SiteId == null)
                    {
                        LogData(trace, $"No SiteId provided for Workorder: {workorder.Id} sending to {workorder.Contacts.FirstOrDefault().Email ?? "no address"}", true);
                    }
                    else
                    {
                        var product = await _apiService.ProductData(workorder.Tasks.FirstOrDefault().ProductId);

                        if (product == null)
                        {
                            LogData(trace, $"Product in database does not exist problem retrieving id: {workorder.Tasks.FirstOrDefault().ProductId}", true);
                        }
                        else
                        {
                            var templateKey = product.Group.Name == "SUBLOCATION" ? "TemplateWorkorderSublocation" : "TemplateWorkorderEquipment";

                            _cultureService.SetCulture(workorder.CustomerProblemDescription.ToLower(),
                                workorder.Site.LocaleCode.ToLower());

                            if (taskData?.Notes.Count > 0)
                            {
                                workorder.Tasks.FirstOrDefault().Notes = taskData.Notes;
                            }

                            foreach (var contact in workorder.Contacts)
                            {
                                if (contact.Email != null)
                                {
                                    LogData(trace, $"Sending message to {contact.Email} regarding Workorder: {workorder.Id}", false);

                                    var mail = _mailService.WorkorderMail(workorder, product, contact.Email, templateKey, workorder.Status);
                                    _mailService.SmtpClient.SendAsync(mail, null);

                                    LogData(trace, $"Send message to {contact.Email} regarding Workorder: {workorder.Id}", false);
                                }
                            }
                        }
                    }
                }
                else
                {
                    LogData(trace, $"Workorder {workorder.Id} is not completed", true);
                }
            }
            catch (Exception ex)
            {
                LogData(trace, $"Message:{ex.Message} InnerException:{ex.InnerException} StackTrace:{ex.StackTrace}", true);
            }

            LogData(trace, "Workorder Complete Sending Finished", false);
        }

        [Transaction(Web = true)]
        public async Task OnControlCenterEscalation([ServiceBusTrigger("efp-api-escalation-topic", "efp-api-escalation-sub-send-notification")] BrokeredMessage message,
            TraceWriter trace)
        {
            message.Complete();
            try
            {
                var deserializedObject = DeserializeBrokeredMessage<string>(message);
                var parsedJObject = JObject.Parse(deserializedObject);
                string recepient = parsedJObject["Entity"]["To"].ToObject<string>();
                Workorder deserializedWorkorder = parsedJObject["Entity"]["Workorder"].ToObject<Workorder>();
                Product product = parsedJObject["Entity"]["Product"].ToObject<Product>();
                EventMonitor eventMonitor = parsedJObject["Entity"]["EventMonitor"].ToObject<EventMonitor>();

                var workorder = await _apiService.WorkorderData(deserializedWorkorder.Id);
                TimeZones timezoneId = await _apiService.TimeZoneData(workorder.Site.TimeZone);
                _cultureService.SetCulture(workorder.CustomerProblemDescription, workorder.Site.LocaleCode.ToLower());

                bool sublocation = product.Group.Name == "SUBLOCATION";

                if (!recepient.Contains("@"))
                {
                    _mailService.CreateTwilioClient();

                    var body = sublocation
                        ? _mailService.SmsSublocation(workorder, product)
                        : _mailService.SmsEquipment(workorder, product);

                    var result = await MessageResource.CreateAsync(new PhoneNumber(recepient),
                        from: new PhoneNumber("+13094200014"),
                        body: body);

                    LogData(trace, $"Sent sms to {result.To} for workorder: {workorder.Id} for escalation: {eventMonitor.Name}", false);
                }
                else
                {
                    var templateKey = sublocation ? "TemplateEscalationSublocation" : "TemplateEscalationEquipment";
                    MailMessage mail = _mailService.EscalationMail(workorder, product, eventMonitor, timezoneId, recepient, templateKey);
                    _mailService.SmtpClient.SendAsync(mail, null);
                    LogData(trace, $"Sent mail to {recepient} for workorder: {workorder.Id} for escalation: {eventMonitor.Name}", false);
                }
            }
            catch (Exception ex)
            {
                LogData(trace, $"Message:{ex.Message} InnerException:{ex.InnerException} StackTrace:{ex.StackTrace}", true);
            }

            LogData(trace, "Escalation Alert Sending Finished", false);
        }

        private T DeserializeBrokeredMessage<T>(BrokeredMessage message)
        {
            Stream stream = message.GetBody<Stream>();
            StreamReader reader = new StreamReader(stream);
            string messageData = reader.ReadToEnd();

            return JsonConvert.DeserializeObject<T>(messageData);
        }

        private void LogData(TraceWriter trace, string message, bool error)
        {
            if (error)
            {
                Log.Error(message);
                trace.Error(message);
            }
            else
            {
                Log.Information(message);
                trace.Info(message);
            }
        }
    }
}
