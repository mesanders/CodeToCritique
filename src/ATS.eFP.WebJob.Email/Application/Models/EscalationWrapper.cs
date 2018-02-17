namespace ATS.eFP.WebJob.Email.Application.Models
{
    public class EscalationWrapper : WorkorderNotificationWrapper
    {
        public EventMonitor EventMonitor { get; set; }
        public string WorkorderCreated { get; set; }
        public string EscalatedAt { get; set; }
        public string AssignedTech { get; set; }
        public string MalfunctionStart { get; set; }
        public string MalfunctionStop { get; set; }
        public string TaskTemplateId { get; set; }
    }
}
