using Quartz;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using Autofac.Integration.WebApi;
using Autofac;
using GeoTagMap.Models;
using GeoTagMap.WebApi.Controllers;
using System.Collections.Generic;

namespace GeoTagMap.WebApi.Jobs
{
    public class JambaseEventsJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            var resolver = GlobalConfiguration.Configuration.DependencyResolver as AutofacWebApiDependencyResolver;
            if (resolver != null)
            {
                using (var scope = resolver.Container.BeginLifetimeScope())
                {
                    var eventController = scope.Resolve<ApiEventDataInsertController>();
                    IHttpActionResult result = await eventController.GetJambaseEventsAsync();

                    if (result is OkNegotiatedContentResult<IEnumerable<(EventModel, TicketInformationModel, List<PerformerModel>, LocationModel, GeoLocation)>> okResult)
                    {
                        var events = okResult.Content;
                    }
                }
            }
        }
    }
}
