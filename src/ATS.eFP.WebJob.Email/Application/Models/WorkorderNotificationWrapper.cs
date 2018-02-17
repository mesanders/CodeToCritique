using System.Collections.Generic;
using ATS.eFP.Entities.Product;
using ATS.eFP.Entities.Workorder;

namespace ATS.eFP.WebJob.Email.Application.Models
{
    public class WorkorderNotificationWrapper
    {
        public Workorder Workorder { get; set; }
        public Product Product { get; set; }
        public List<string> NotesAndRemarks { get; set; } = new List<string>();
        public string HeaderLogo { get; set; }
        public string HeaderHeadset { get; set; }
        public string FooterButton { get; set; }
        public string PortalLink { get; set; }
        public string Subject { get; set; }
        public string SubjectSub { get; set; }
    }
}
