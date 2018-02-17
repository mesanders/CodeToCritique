using System.Threading.Tasks;
using ATS.eFP.Entities.Common;
using ATS.eFP.Entities.Product;
using ATS.eFP.Entities.Workorder;
using ATS.eFP.WebJob.Email.Application;
using Task = ATS.eFP.Entities.Task.Task;

namespace ATS.eFP.WebJob.Email.Services
{
    public class ApiService
    {
        public async Task<Product> ProductData(string productId)
        {
            return await eFPApi.ODataApiClient.For<Product>()
                .Filter(x => x.Id == productId)
                .Expand(x => x.Sublocation)
                .Expand(x => x.Group)
                .FindEntryAsync();
        }

        public async Task<TimeZones> TimeZoneData(string timeZone)
        {
            return await eFPApi.ODataApiClient.For<TimeZones>()
                .Filter(x => x.TimeZone == timeZone)
                .Select(x => x.InfoId)
                .FindEntryAsync();
        }

        public async Task<Workorder> WorkorderData(string id)
        {
            return await eFPApi.ODataApiClient.For<Workorder>()
                .Filter(x => x.Id == id)
                .Expand(x => x.Site)
                .Expand(x => x.Tasks)
                .Expand(x => x.Contacts)
                .FindEntryAsync();
        }

        public async Task<Task> TaskData(decimal id)
        {
            return await eFPApi.ODataApiClient.For<Task>()
                .Filter(x => x.Id == id)
                .Expand(x => x.Notes)
                .Expand(x => x.AssignedPerson)
                .FindEntryAsync();
        }
    }
}
