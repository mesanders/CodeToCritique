using System;
using System.Collections.ObjectModel;
using ATS.eFP.Entities.Common;
using ATS.eFP.Entities.Person;
using ATS.eFP.Entities.Product;
using ATS.eFP.Entities.Site;
using ATS.eFP.Entities.Task;
using ATS.eFP.Entities.Workorder;
using ATS.eFP.WebJob.Email.Application;
using ATS.eFP.WebJob.Email.Application.Configuration;
using ATS.eFP.WebJob.Email.Application.Models;
using ATS.eFP.WebJob.Email.Services;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using Xunit;

namespace ATS.eFP.WebJob.Email.Tests
{
    public class EmailTests
    {
        private MailService _mailService;
        private CultureService _cultureService;
        private Workorder _workorder;
        private Product _equipment;
        private Product _sublocation;
        private EventMonitor _eventMonitor;
        public EmailTests()
        {
            Settings.Initialize();
            TemplateConfiguration.Initialize();
            _cultureService = new CultureService();
            _mailService = new MailService(_cultureService);
            _workorder = CreateWorkorderMock();
            _equipment = CreateEquipmentMock();
            _sublocation = CreateSublocationMock();
            _eventMonitor = CreateEventMonitorMock();
        }

        private EventMonitor CreateEventMonitorMock()
        {
            return new EventMonitor
            {
                Description = "Event Monitor Description",
                Name = "Boundy"
            };
        }

        private Product CreateEquipmentMock()
        {
            return new Product
            {
                Id = "832726",
                AssetId = "BT-1234",
                Description = "Grinder",
                BuildingLocation = "KK",
                OperatingStatusId = "UP",
                CriticalityId = "1",
                SublocationId = "product sub id"
            };
        }

        private Product CreateSublocationMock()
        {
            return new Product
            {
                Id = "Assembly",
                AssetId = "BT-1234",
                Description = "this description",
                BuildingLocation = "",
                OperatingStatusId = "UP",
                CriticalityId = "1",
                SublocationId = "product sub id",
                Group = new ProductGroup
                {
                    Name = "SUBLOCATION"
                }
            };
        }

        private Workorder CreateWorkorderMock()
        {
            return new Workorder
            {
                Id = "123435",
                CustomerProblemDescription = "This is a test",
                Tasks = new ObservableCollection<Task>
                {
                    new Task
                    {
                        ProductId = "productId 123",
                        ProductAssetId = "product asset id 123",
                        AssignedPerson = new Person(),
                        Notes = new ObservableCollection<Note>
                        {
                            new Note
                            {
                                Text = "Bacon ipsum dolor amet filet mignon strip steak chuck andouille corned beef, pastrami shank pig picanha short loin pork porchetta meatball jowl. Ground round prosciutto kielbasa tail cow fatback corned beef picanha porchetta pig. Alcatra kielbasa sirloin turkey porchetta meatloaf. Ball tip turkey capicola andouille strip steak corned beef pork belly brisket. Leberkas meatloaf chicken, cupim pork loin capicola ribeye chuck bresaola beef prosciutto. Turkey ground round spare ribs, pork loin pork boudin bacon pancetta." +

                                "Buffalo beef ribs brisket swine hamburger pork belly. Picanha bresaola rump drumstick short loin beef boudin ground round. Chicken shoulder beef pig frankfurter porchetta, tongue short ribs turkey swine brisket jowl tenderloin. Strip steak landjaeger andouille short loin pork belly capicola turkey." +

                                "Pastrami cow kevin turkey jerky tongue. Meatball jerky biltong short loin shankle andouille. Buffalo tongue ground round shoulder, salami chuck porchetta strip steak tri-tip fatback pig sausage hamburger tail frankfurter. Frankfurter prosciutto cupim capicola, drumstick pork salami corned beef picanha jowl. Ground round short ribs andouille ham hock sausage. Capicola kevin buffalo, brisket meatball landjaeger pork loin jerky pig. Hamburger spare ribs meatball jerky capicola rump venison." +

                                "Sausage kielbasa ham meatball frankfurter. Cow andouille bresaola jowl, kielbasa ball tip picanha shoulder salami ribeye. Boudin strip steak tenderloin porchetta, rump jerky flank salami. Capicola frankfurter flank venison picanha." +

                                "Picanha short loin kevin pastrami. Burgdoggen salami shankle, tri-tip short ribs kevin tongue. Pork loin ham hock doner bacon turducken. Andouille tongue tenderloin fatback, ham landjaeger turducken ground round. Brisket ribeye ham pork cow sausage. Corned beef burgdoggen pancetta picanha, turducken pork chop jerky ground round kevin jowl tongue shoulder pork belly."
                            }
                        }
                    }
                },
                PriorityId = "1",
                WorkorderUnits = new ObservableCollection<WorkorderUnit>
                {
                    new WorkorderUnit
                    {
                        MalfunctionStart = DateTime.Today.AddDays(-1)
                    }
                },
                Status = "Open",
                SiteId = "ST90000436",
                Site = new Site
                {
                    Name = "Test Name"
                },
                SublocationId = "workorder sub id",
                Created = DateTime.UtcNow
            };
        }

        [Fact]
        public void SendWorkorderEquipmentTemplate()
        {
            var mail = _mailService.WorkorderMail(_workorder, _equipment, "jboundy@advancedtech.com", "TemplateWorkorderEquipment", "OPEN");
            _mailService.SmtpClient.Send(mail);
        }

        [Fact]
        public void SendWorkorderSublocationTemplate()
        {
            var mail = _mailService.WorkorderMail(_workorder, _sublocation, "jboundy@advancedtech.com", "TemplateWorkorderSublocation", "OPEN");
            _mailService.SmtpClient.Send(mail);
        }

        [Fact]
        public void SendEscalationEquipmentTemplate()
        {
            var timezone = new TimeZones
            {
                InfoId = "Eastern Standard Time"
            };

            var mail = _mailService.EscalationMail(_workorder, _equipment, _eventMonitor, timezone, "jboundy@advancedtech.com", "TemplateEscalationEquipment");
            _mailService.SmtpClient.Send(mail);
        }

        [Fact]
        public void SendEscalationSublocationTemplate()
        {
            var timezone = new TimeZones
            {
                InfoId = "Eastern Standard Time"
            };

            var mail = _mailService.EscalationMail(_workorder, _sublocation, _eventMonitor, timezone, "jboundy@advancedtech.com", "TemplateEscalationSublocation");
            _mailService.SmtpClient.Send(mail);
        }

        [Fact]
        public void SendEsclationEquipmentSMSTemplate()
        {
            TwilioClient.Init("AC1091607564f3e4cbef1215b9873768ef", "1ef98c704d0a1d8fd96e073dd486f938");
            var mail = _mailService.SmsEquipment(_workorder, _equipment);
            var result = MessageResource.Create(new PhoneNumber("+13092026577"),
                from: new PhoneNumber("+13094200014"),
                body: mail);
        }
    }
}
